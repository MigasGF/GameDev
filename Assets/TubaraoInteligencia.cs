using UnityEngine;

public class InteligenciaTubarao : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public Transform player;
    public float velocidade = 4f;
    public float distanciaDeDeteccao = 25f; 
    public float compensacaoDeRotacaoY = -90f; 
    
    // --- NOVO: Configurações de Ataque ---
    [Header("Configurações de Ataque")]
    public float dano = 25f;
    public float tempoEntreAtaques = 3f; // Os 3 segundos de espera
    private float tempoProximoAtaque = 0f; // Guarda o relógio interno do tubarão
    
    private Vector3 posicaoInicial;
    private Animator playerAnim; 
    private PlayerMovement playerScript; // Para podermos tirar vida ao jogador

    void Start()
    {
        posicaoInicial = transform.position;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player != null)
        {
            playerAnim = player.GetComponent<Animator>();
            // Vamos buscar o teu script de movimento para aceder à vida
            playerScript = player.GetComponent<PlayerMovement>(); 
        }
    }

    void Update()
    {
        if (player == null || playerAnim == null) return;

        bool jogadorNaAgua = playerAnim.GetBool("inWater");
        float distanciaParaOJogador = Vector3.Distance(transform.position, player.position);

        if (jogadorNaAgua && distanciaParaOJogador <= distanciaDeDeteccao)
        {
            Perseguir(player.position);
        }
        else
        {
            if (Vector3.Distance(transform.position, posicaoInicial) > 0.5f)
            {
                Perseguir(posicaoInicial);
            }
        }
    }

    private void Perseguir(Vector3 destino)
    {
        Vector3 direcao = (destino - transform.position).normalized;
        direcao.y = 0; 

        if (direcao != Vector3.zero)
        {
            Quaternion rotacaoBase = Quaternion.LookRotation(direcao);
            Quaternion rotacaoCorrigida = rotacaoBase * Quaternion.Euler(0, compensacaoDeRotacaoY, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacaoCorrigida, Time.deltaTime * 5f);
        }

        transform.position += direcao * velocidade * Time.deltaTime;
    }

    // --- NOVO: Usamos Stay para o tubarão conseguir dar dano contínuo (com pausas) ---
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Verifica se o tempo atual do jogo já ultrapassou a "barreira" do próximo ataque
            if (Time.time >= tempoProximoAtaque)
            {
                Debug.Log("Tubarão atacou! -25 vida");
                
                // Manda o dano para a função que já existe no teu PlayerMovement
                if (playerScript != null)
                {
                    playerScript.ReceberDano(dano, transform);
                }

                // Define que o próximo ataque só pode acontecer daqui a 3 segundos
                tempoProximoAtaque = Time.time + tempoEntreAtaques;
            }
        }
    }
}