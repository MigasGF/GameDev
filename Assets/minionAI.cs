using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI; 

public class InteligenciaEsqueleto : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player; 
    public Animator anim;
    public float distanciaParaAtacar = 2.0f;
    [Header("Configuración de Visión")]
    [Tooltip("Pon 0 para visión infinita (comportamiento original), o un número mayor para limitar la visión.")]
    public float distanciaDeVision = 0f;
    
    public float tempoEntreAtaques = 1.5f; 
    private float temporizador; 

    // --- SISTEMA DE VIDA ---
    public Slider barraVidaInimigo; 
    public float vidaAtual = 100f;
    public float danoDoAtaque = 15f; 
    private bool estaMorto = false;

    // --- CONFIGURAÇÕES DE SOM (FMOD) ---
    [Header("Sound Settings")]
    // Este valor é enviado para o parâmetro "Enemy" no FMOD ao levar com a espada
    // Configura no Inspector para cada tipo (ex: Ossos = 0, Pedra = 1, etc.)
    public float enemySoundType = 0f; 

    [Header("Drop System")]
    public GameObject[] comidasParaDropar; // Arrasta os prefabs de comida para aqui no Inspector
    [Range(0f, 1f)] public float chanceDeDrop = 0.5f; // 50% de chance de deixar cair algo

    // --- EFEITOS VISUAIS DE DANO (TUTORIAL) ---
    [Header("Efeitos Visuais de Dano")]
    public Renderer modelo3D; // Onde vais arrastar a malha 3D do esqueleto
    public Color corPiscar = Color.red; // A cor que ele vai ficar ao levar dano
    private Color[] coresOriginais;

    void Start()
    {
        if (barraVidaInimigo != null)
        {
            barraVidaInimigo.maxValue = 100f;
            barraVidaInimigo.value = vidaAtual;
        }

        if (modelo3D != null)
        {
            // Guarda as cores originais de todos os materiais do monstro
            coresOriginais = new Color[modelo3D.materials.Length];
            for (int i = 0; i < modelo3D.materials.Length; i++)
            {
                coresOriginais[i] = modelo3D.materials[i].color;
            }
        }
    }

    void Update()
    {
        if (estaMorto) return; 

        // Calculamos a qué distancia está el jugador
        float distancia = Vector3.Distance(transform.position, player.position);

        // EL TRUCO DE COMPATIBILIDAD:
        // Es verdadero SI la visión está en 0 (comportamiento original) O SI el jugador está en rango.
        bool puedeVerAlJugador = (distanciaDeVision <= 0f) || (distancia <= distanciaDeVision);

        if (puedeVerAlJugador)
        {
            // --- AQUÍ EMPIEZA EL COMPORTAMIENTO EXACTO DEL CÓDIGO ORIGINAL ---
            if (distancia > distanciaParaAtacar)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position); // Lo persigue
            }
            else 
            {
                agent.isStopped = true; // Se detiene para atacar
                
                Vector3 direcaoOlhar = new Vector3(player.position.x, transform.position.y, player.position.z);
                transform.LookAt(direcaoOlhar);
                
                if (Time.time >= temporizador)
                {
                    anim.SetTrigger("attack");
                    
                    PlayerMovement scriptPlayer = player.GetComponent<PlayerMovement>();
                    if (scriptPlayer != null)
                    {
                        scriptPlayer.ReceberDano(danoDoAtaque, transform);
                    }

                    temporizador = Time.time + tempoEntreAtaques; 
                }
            }
        }
        else
        {
            // --- NUEVA LÓGICA DE ESPERA ---
            // Solo entra aquí si en Unity pusiste distanciaDeVision mayor a 0 y el jugador está lejos
            agent.isStopped = true; 
        }

        // Actualiza la animación de caminar
        anim.SetFloat("Speed", agent.velocity.magnitude);
    }

    public void ReceberDano(float dano)
    {
        if (estaMorto) return;

        vidaAtual -= dano;
        if (barraVidaInimigo != null) barraVidaInimigo.value = vidaAtual;
        
        // Ativa o efeito de piscar (Merge do Tutorial)
        if (modelo3D != null) StartCoroutine(EfeitoPiscar());

        if (vidaAtual <= 0)
        {
            estaMorto = true;
            anim.SetTrigger("die"); 
            agent.isStopped = true;
            agent.enabled = false;
            Destroy(gameObject, 3f);
            
            if (Random.value <= chanceDeDrop && comidasParaDropar.Length > 0)
            {
                // Escolhe uma comida aleatória da lista
                int indexAleatorio = Random.Range(0, comidasParaDropar.Length);
                GameObject comidaEscolhida = comidasParaDropar[indexAleatorio];
            
                // Cria a comida ligeiramente acima do chão
                Vector3 posicaoDrop = transform.position + new Vector3(0, 1f, 0);
                Instantiate(comidaEscolhida, posicaoDrop, Quaternion.identity);
            } 
        }
    }

    System.Collections.IEnumerator EfeitoPiscar()
    {
        // 1. Pinta tudo com a cor de dano (Vermelho)
        for (int i = 0; i < modelo3D.materials.Length; i++)
        {
            modelo3D.materials[i].color = corPiscar;
        }

        // 2. Espera uma fração minúscula de segundo (o tempo do piscar)
        yield return new WaitForSeconds(0.15f); 

        // 3. Volta a pintar com as cores originais
        for (int i = 0; i < modelo3D.materials.Length; i++)
        {
            modelo3D.materials[i].color = coresOriginais[i];
        }
    }
}