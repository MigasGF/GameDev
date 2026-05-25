using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    private float rotationVelocity;
    public float speed = 5f;
    public float rotationSpeed = 720f;
    
    private Animator anim;
    private CharacterController controller;
    private Transform mainCamera; // <- Adicionada a referência à câmara

    // --- SISTEMA DE VIDA E ATAQUE ---
    public Slider barraVidaPlayer;
    public float vidaAtual = 100f;
    public float danoDoAtaque = 35f;
    public float alcanceDoAtaque = 2.5f;
    private bool estaMorto = false;

    float velocidadeVertical;
    float forcaSalto = 5f;
    float gravidade = -15f; // Aumentei um pouco para o salto ser mais rápido e realista

    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main.transform; // <- O Unity encontra a câmara principal automaticamente
        anim.applyRootMotion = false;

        if (barraVidaPlayer != null)
        {
            barraVidaPlayer.maxValue = 100f;
            barraVidaPlayer.value = vidaAtual;
        }
    }

    void Update()
    {
        if (estaMorto) return;

        // --- 1. SISTEMA DE ATAQUE ---
        if (Input.GetMouseButtonDown(0)) 
        {
            anim.SetTrigger("bash");
            AtacarInimigos(); 
        }

        // --- 2. SISTEMA DE GRAVIDADE E SALTO ---
        // Só aplicamos o salto se o cavaleiro estiver a tocar no chão
        if (controller.isGrounded)
        {
            velocidadeVertical = -2f; // Uma força mínima para o manter colado ao chão nas descidas
            
            if (Input.GetKeyDown(KeyCode.Space)) 
            {
                velocidadeVertical = forcaSalto;
                anim.SetTrigger("jump");
            }
        }
        // A gravidade puxa-o sempre para baixo ao longo do tempo
        velocidadeVertical += gravidade * Time.deltaTime;

        // --- 3. SISTEMA DE MOVIMENTO (Baseado na Câmara) ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(h, 0f, v).normalized;

        // Vetor final que vai juntar a direção e o salto
        Vector3 movimentoFinal = Vector3.zero;

        if (direction.magnitude >= 0.1f)
        {
            anim.SetBool("isRunning", true);

            // A MAGIA ACONTECE AQUI: Somamos a rotação da câmara ao movimento do jogador
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCamera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationVelocity, 0.25f);
            
            // Roda o boneco para a direção certa
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Transforma esse ângulo num vetor de movimento para a frente
            Vector3 direcaoDoMovimento = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            movimentoFinal = direcaoDoMovimento * speed;
        }
        else
        {
            anim.SetBool("isRunning", false);
        }

        // --- 4. APLICAR TODO O MOVIMENTO DE UMA VEZ ---
        movimentoFinal.y = velocidadeVertical; // Juntamos a força do salto/gravidade ao vetor
        controller.Move(movimentoFinal * Time.deltaTime); // Mandamos o CharacterController andar
    }

    void AtacarInimigos()
    {
        InteligenciaEsqueleto[] esqueletos = FindObjectsByType<InteligenciaEsqueleto>(FindObjectsSortMode.None);
        
        foreach (InteligenciaEsqueleto esqueleto in esqueletos)
        {
            float distancia = Vector3.Distance(transform.position, esqueleto.transform.position);
            
            if (distancia <= alcanceDoAtaque)
            {
                esqueleto.ReceberDano(danoDoAtaque);
            }
        }
    }

    public void ReceberDano(float dano, Transform atacante)
    {
        if (estaMorto) return;

        if (Input.GetMouseButton(1))
        {
            Vector3 direcaoDoAtaque = (atacante.position - transform.position).normalized;
            direcaoDoAtaque.y = 0; 
            float angulo = Vector3.Angle(transform.forward, direcaoDoAtaque);

            if (angulo <= 70f)
            {
                Debug.Log("Bloqueaste o ataque com o escudo de frente!");
                return; 
            }
            else
            {
                Debug.Log("Ai! Levaste dano pelas costas ou lado!");
            }
        }

        vidaAtual -= dano;
        if (barraVidaPlayer != null) barraVidaPlayer.value = vidaAtual;

        if (vidaAtual <= 0)
        {
            estaMorto = true;
            anim.SetTrigger("die"); 
            controller.enabled = false;
            Debug.Log("Morreste!");
        }
    }
}