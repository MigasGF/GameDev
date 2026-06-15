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
    public float vidaMaxima = 100f;
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

    [Header("Feedback de Dano")]
    public Image telaDeDano; // A imagem que vai piscar
    public Color corDoDano = new Color(1f, 0f, 0f, 0.4f); // Vermelho com alguma transparência
    public float velocidadeRecuperacao = 4f; // Quão rápido o vermelho desaparece
    private bool levouPancada = false;
    public bool estaInvencivel = false;

    [Header("Poderes Mágicos")]
    public bool poderCorteAr = false;
    public GameObject prefabCorteAr;
    public Transform pontoDisparoCorte;

[Header("Sistema de Postura / Escudo")]
    public GameObject objetoBarraBloqueio; 
    public Image mascaraDaBarra;   // Controla o Fill (Arrastar o "Fill Mask")
    public RawImage imagemDaBarra; // Controla a Cor (Arrastar o "Fill Image")
    
    public float posturaAtual = 0f;
    public float posturaMaxima = 100f;
    public float custoPosturaPorAtaque = 25f; 
    public float velocidadeRegeneracao = 15f; 
    private bool bloqueioQuebrado = false;

    [Header("Cores da Barra")]
    public Color corNormalBarra = Color.yellow;
    public Color corBarraQuebrada = Color.red;
    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main.transform;
        anim.applyRootMotion = false;

        if (barraVidaPlayer != null)
        {
            barraVidaPlayer.maxValue = vidaMaxima;
            barraVidaPlayer.value = vidaAtual;
        }

        if (objetoBarraBloqueio != null) objetoBarraBloqueio.SetActive(false);
        
        if (mascaraDaBarra != null) mascaraDaBarra.fillAmount = 0f;
        if (imagemDaBarra != null) imagemDaBarra.color = corNormalBarra;

        anim.updateMode = AnimatorUpdateMode.UnscaledTime;

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
            velocidadeVertical += gravidade * Time.unscaledDeltaTime;
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

            // 2. Decide para onde o boneco deve OLHAR (imune ao Time Stop!)
            if (v < 0)
            {
                // Anda para trás, olha para a frente
                float anguloOlhar = mainCamera.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, anguloOlhar, ref rotationVelocity, 0.25f, Mathf.Infinity, Time.unscaledDeltaTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }
            else
            {
                // Anda para a frente/lados, roda normalmente
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, anguloMovimento, ref rotationVelocity, 0.25f, Mathf.Infinity, Time.unscaledDeltaTime);
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
        controller.Move(movimentoFinal * Time.unscaledDeltaTime);

        if (telaDeDano != null)
        {
            if (levouPancada)
            {
                // Fica vermelho instantaneamente!
                telaDeDano.color = corDoDano;
                levouPancada = false;
            }
            else
            {
                // Vai desvanecendo suavemente de volta para transparente (Color.clear)
                telaDeDano.color = Color.Lerp(telaDeDano.color, Color.clear, velocidadeRecuperacao * Time.unscaledDeltaTime);
            }
        }
        // Regeneração
        if (!bloqueioQuebrado && posturaAtual > 0)
        {
            posturaAtual -= velocidadeRegeneracao * Time.unscaledDeltaTime;
            if (posturaAtual < 0) posturaAtual = 0;
        }

        // Atualiza a UI: A MÁSCARA controla o tamanho
        if (mascaraDaBarra != null)
        {
            mascaraDaBarra.fillAmount = posturaAtual / posturaMaxima;
        }

        // Esconde ou mostra a barra inteira
        if (objetoBarraBloqueio != null)
        {
            objetoBarraBloqueio.SetActive(posturaAtual > 0);
        }
    }



    public void ReceberDano(float dano, Transform atacante)
    {
        if (estaMorto || estaInvencivel) return;

        // A Matemática do Escudo acontece aqui: verifica o mesmo botão direito do teu outro script
        // Só bloqueia se carregar no botão E se o bloqueio não estiver quebrado!
        if (Input.GetMouseButton(1) && !bloqueioQuebrado)
        {
            Vector3 direcaoDoAtaque = (atacante.position - transform.position).normalized;
            direcaoDoAtaque.y = 0;
            float angulo = Vector3.Angle(transform.forward, direcaoDoAtaque);

            if (angulo <= 70f)
            {
                Debug.Log("Bloqueaste o ataque com o escudo de frente!");

                // --- NOVO: Aumenta a barra amarela ---
                posturaAtual += custoPosturaPorAtaque;

                // Se a barra encher, ativa a punição de 5 segundos
                if (posturaAtual >= posturaMaxima)
                {
                    StartCoroutine(RotinaQuebraBloqueio());
                }

                return; // Anula o dano
            }
            else
            {
                Debug.Log("Ai! Levaste dano pelas costas ou lado!");
            }
        }
        levouPancada = true;
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

        if (scriptEspada != null) scriptEspada.PrepararNovoAtaque(); // Limpa a lista!
        if (colliderEspada != null) colliderEspada.enabled = true;

        anim.SetTrigger("bash");

        if (poderCorteAr && prefabCorteAr != null && pontoDisparoCorte != null)
        {
            yield return new WaitForSecondsRealtime(0.2f);
            Instantiate(prefabCorteAr, pontoDisparoCorte.position, transform.rotation);
        }

        yield return new WaitForSecondsRealtime(0.5f);

        if (colliderEspada != null) colliderEspada.enabled = false;
        estaAAtacar = false;
    }

    System.Collections.IEnumerator RotinaQuebraBloqueio()
    {
        bloqueioQuebrado = true;
        posturaAtual = posturaMaxima; 

        if (mascaraDaBarra != null) mascaraDaBarra.fillAmount = 1f; 
        if (imagemDaBarra != null) imagemDaBarra.color = corBarraQuebrada; // Fica vermelha!

        Debug.Log("DEFESA QUEBRADA! Ficas atordoado por 5 segundos!");

        yield return new WaitForSecondsRealtime(5f);

        bloqueioQuebrado = false;
        posturaAtual = 0f;

        if (mascaraDaBarra != null) mascaraDaBarra.fillAmount = 0f; 
        if (imagemDaBarra != null) imagemDaBarra.color = corNormalBarra; // Volta a amarelo!

        Debug.Log("Recuperaste a postura. Já podes bloquear de novo!");
    }
}