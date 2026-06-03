using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI; 

public class InteligenciaEsqueleto : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player; 
    public Animator anim;
    public float distanciaParaAtacar = 2.0f;
    
    public float tempoEntreAtaques = 1.5f; 
    private float temporizador; 

    // --- SISTEMA DE VIDA ---
    public Slider barraVidaInimigo; 
    public float vidaAtual = 100f;
    public float danoDoAtaque = 15f; 
    private bool estaMorto = false;

    [Header("Drop System")]
    public GameObject[] comidasParaDropar; // Arrasta os prefabs de comida para aqui no Inspector
    [Range(0f, 1f)] public float chanceDeDrop = 0.5f; // 50% de chance de deixar cair algo

    [Header("Efeitos Visuais de Dano")]
    public Renderer modelo3D; // Onde vais arrastar a malha 3D do esqueleto
    public Color corPiscar = Color.red; // A cor que ele vai ficar (podes mudar no Unity)
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

        float distancia = Vector3.Distance(transform.position, player.position);

        if (distancia > distanciaParaAtacar)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            agent.isStopped = true;
            
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

        anim.SetFloat("Speed", agent.velocity.magnitude);
    }

    public void ReceberDano(float dano)
    {
        if (estaMorto) return;

        vidaAtual -= dano;
        if (barraVidaInimigo != null) barraVidaInimigo.value = vidaAtual;
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
            
                // Cria a comida ligeiramente acima do chão para não ficar presa na malha 3D
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