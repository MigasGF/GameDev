using UnityEngine;
using UnityEngine.UI; // Para a barra de vida

public class PlayerMovement : MonoBehaviour
{
    private float rotationVelocity;
    public float speed = 5f;
    public float rotationSpeed = 720f;
    
    private Animator anim;
    private CharacterController controller;

    // --- SISTEMA DE VIDA E ATAQUE ---
    public Slider barraVidaPlayer; // Arrasta o teu HUD para aqui
    public float vidaAtual = 100f;
    public float danoDoAtaque = 35f; // Quanto dano a espada do cavaleiro dá
    public float alcanceDoAtaque = 2.5f; // Distância a que a espada chega
    private bool estaMorto = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        anim.applyRootMotion = false;

        if (barraVidaPlayer != null)
        {
            barraVidaPlayer.maxValue = 100f;
            barraVidaPlayer.value = vidaAtual;
        }
    }

    void Update()
    {
        if (estaMorto) return; // Se o Cavaleiro morrer, não pode andar nem atacar

        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            anim.SetTrigger("jump");
        }

        if (Input.GetMouseButtonDown(0)) 
        {
            anim.SetTrigger("bash");
            AtacarInimigos(); // Dispara o código de verificar quem está perto
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(h, 0f, v).normalized;

        if (direction.magnitude >= 0.1f)
        {
            anim.SetBool("isRunning", true);

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationVelocity, 0.05f);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            controller.Move(direction * speed * Time.deltaTime);
        }
        else
        {
            anim.SetBool("isRunning", false);
        }
    }

    void AtacarInimigos()
    {
        // Encontra todos os esqueletos que existam na cena (útil se tiveres mais que um!)
        InteligenciaEsqueleto[] esqueletos = FindObjectsByType<InteligenciaEsqueleto>(FindObjectsSortMode.None);
        
        foreach (InteligenciaEsqueleto esqueleto in esqueletos)
        {
            // Calcula a distância entre ti e o esqueleto
            float distancia = Vector3.Distance(transform.position, esqueleto.transform.position);
            
            // Se estiver a menos de 2.5 metros (alcanceDoAtaque)... PIMBA!
            if (distancia <= alcanceDoAtaque)
            {
                esqueleto.ReceberDano(danoDoAtaque);
            }
        }
    }

    // O esqueleto chama esta função quando te ataca
    public void ReceberDano(float dano)
    {
        if (estaMorto) return;

        vidaAtual -= dano;
        if (barraVidaPlayer != null) barraVidaPlayer.value = vidaAtual;

        if (vidaAtual <= 0)
        {
            estaMorto = true;
            anim.SetTrigger("die"); // Tens de ter um trigger "die" no Animator do Knight para ele cair morto
            controller.enabled = false;
            Debug.Log("Morreste!");
        }
    }
}