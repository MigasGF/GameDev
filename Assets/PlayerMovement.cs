using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    private float rotationVelocity;
    public float speed = 5f;
    public float rotationSpeed = 720f;
    
    private Animator anim;
    private CharacterController controller;
    private Transform mainCamera;

    // --- SISTEMA DE VIDA E ATAQUE ---
    public Slider barraVidaPlayer;
    public float vidaAtual = 100f;
    public float danoDoAtaque = 35f;
    public float alcanceDoAtaque = 2.5f;
    private bool estaMorto = false;

    float velocidadeVertical;
    public float forcaSaltoAgua = 6f;
    float forcaSalto = 5f;
    float gravidade = -15f; 

    // Limite de queda do jogador (Nível da Água)
    private float limiteInferiorY = -0.1f;

    // --- REFERÊNCIA PARA A BÓIA ---
    public GameObject boia;

    public Collider colliderEspada;
    public SwordDamage scriptEspada;
    public bool estaAAtacar = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main.transform; 
        anim.applyRootMotion = false;

        if (barraVidaPlayer != null)
        {
            barraVidaPlayer.maxValue = 100f;
            barraVidaPlayer.value = vidaAtual;
        }

        // Garante que a bóia começa invisível quando o jogo arranca
        if (boia != null) boia.SetActive(false);

        if (colliderEspada != null) colliderEspada.enabled = false;
    }

    void Update()
    {
        if (estaMorto) return;

        // --- 1. SISTEMA DE ATAQUE ---
        if (Input.GetMouseButtonDown(0) && !estaAAtacar) 
        {
            StartCoroutine(RotinaDeAtaque()); 
        }

        // --- 2. SISTEMA DE GRAVIDADE, SALTO E ÁGUA ---
        bool noFundoDoPoco = transform.position.y <= limiteInferiorY;

        // Ativa a bóia e a animação APENAS se estiver na água (limite -0.3)
        if (noFundoDoPoco)
        {
            if (boia != null) boia.SetActive(true);
            anim.SetBool("inWater", true);
        }
        else
        {
            if (boia != null) boia.SetActive(false);
            anim.SetBool("inWater", false);
        }

        // Se chegou à água, prendemos a posição
        if (noFundoDoPoco)
        {
            controller.enabled = false;
            transform.position = new Vector3(transform.position.x, limiteInferiorY, transform.position.z);
            controller.enabled = true;

            if (velocidadeVertical < 0)
            {
                velocidadeVertical = 0f;
            }
        }

        // Aplicamos o salto se estiver a tocar no chão normal OU na água
        if (controller.isGrounded || noFundoDoPoco)
        {
            if (controller.isGrounded && !noFundoDoPoco)
            {
                velocidadeVertical = -2f; 
            }
            
            if (Input.GetKeyDown(KeyCode.Space)) 
            {
                if (noFundoDoPoco)
                {
                    velocidadeVertical = forcaSaltoAgua;
                }
                else
                {
                    velocidadeVertical = forcaSalto;
                }

                anim.SetTrigger("jump");
                
                // Desativa a bóia e a animação imediatamente ao saltar para a transição ser fluída
                if (boia != null) boia.SetActive(false);
                anim.SetBool("inWater", false);
            }
        }

        if (!noFundoDoPoco || velocidadeVertical > 0)
        {
            velocidadeVertical += gravidade * Time.deltaTime;
        }

// --- 3. SISTEMA DE MOVIMENTO (Baseado na Câmara) ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(h, 0f, v).normalized;

        Vector3 movimentoFinal = Vector3.zero;

        if (direction.magnitude >= 0.1f)
        {
            anim.SetBool("isRunning", true);

            // 1. Calcula a direção real para onde ele se vai mover (relativa à câmara)
            float anguloMovimento = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCamera.eulerAngles.y;
            Vector3 direcaoDoMovimento = Quaternion.Euler(0f, anguloMovimento, 0f) * Vector3.forward;

            // 2. Decide para onde o boneco deve OLHAR
            if (v < 0) 
            {
                // Se carregou no 'S' (v é negativo), obrigamos o boneco a olhar para a frente da câmara
                float anguloOlhar = mainCamera.eulerAngles.y; 
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, anguloOlhar, ref rotationVelocity, 0.25f);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }
            else
            {
                // Se carregou no W, A ou D, roda o corpo normalmente para a direção do movimento
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, anguloMovimento, ref rotationVelocity, 0.25f);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }

            // 3. Aplica a velocidade à direção calculada
            movimentoFinal = direcaoDoMovimento * speed;
        }
        else
        {
            anim.SetBool("isRunning", false);
        }

        // --- 4. APLICAR TODO O MOVIMENTO DE UMA VEZ ---
        movimentoFinal.y = velocidadeVertical; 
        controller.Move(movimentoFinal * Time.deltaTime); 
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
        // --- ATAQUE AO BOSS ---
        BossController boss = FindObjectOfType<BossController>();
        if (boss != null)
        {
            // Criamos posições virtuais no chão (Y = 0) para o jogo não se deixar enganar pela altura do Boss!
            Vector3 posicaoPlayerNoChao = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 posicaoBossNoChao = new Vector3(boss.transform.position.x, 0, boss.transform.position.z);
            
            // Agora a distância é medida apenas a direito, sem contar com a altura
            float distanciaBoss = Vector3.Distance(posicaoPlayerNoChao, posicaoBossNoChao);
            
            // Se o Collider dele continuar largo, aumenta este + 5.0f para + 7.0f ou mais!
            if (distanciaBoss <= (alcanceDoAtaque + 5.0f)) 
            {
                boss.ReceberDano(danoDoAtaque);
                Debug.Log("Acertaste no Boss! Distância: " + distanciaBoss);
            }
        }
        
        IntelligenceBoss bossGolem = FindObjectOfType<IntelligenceBoss>();
        if (bossGolem != null)
        {
            // Usamos a tua mesma lógica de posições no chão (Y = 0)
            Vector3 posicaoPlayerNoChao = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 posicaoBossNoChao = new Vector3(bossGolem.transform.position.x, 0, bossGolem.transform.position.z);
            
            float distanciaBoss = Vector3.Distance(posicaoPlayerNoChao, posicaoBossNoChao);
            
            // Margem de acerto do Golem
            if (distanciaBoss <= (alcanceDoAtaque + 5.0f)) 
            {
                bossGolem.ReceberDano(danoDoAtaque);
                Debug.Log("Acertaste no Golem! Distância: " + distanciaBoss);
            }
        }
    }

    public void ReceberDano(float dano, Transform atacante)
    {
        if (estaMorto) return;

        // A Matemática do Escudo acontece aqui: verifica o mesmo botão direito do teu outro script
        if (Input.GetMouseButton(1))
        {
            Vector3 direcaoDoAtaque = (atacante.position - transform.position).normalized;
            direcaoDoAtaque.y = 0; 
            float angulo = Vector3.Angle(transform.forward, direcaoDoAtaque);

            // Se o inimigo atacar num ângulo de 70 graus pela frente, o dano é anulado
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

    System.Collections.IEnumerator RotinaDeAtaque()
    {
        estaAAtacar = true;
        
        if (scriptEspada != null) scriptEspada.PrepararNovoAtaque(); 
        if (colliderEspada != null) colliderEspada.enabled = true; 
        
        anim.SetTrigger("bash");
        
        yield return new WaitForSeconds(0.5f); 
        
        AtacarInimigos(); 
        
        yield return new WaitForSeconds(0.5f); 
        
        if (colliderEspada != null) colliderEspada.enabled = false; 
        estaAAtacar = false;
    }
}