using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MutantBossController : MonoBehaviour
{
    [Header("Referencias Principales")]
    public Transform player;
    public PlayerMovement scriptPlayer; // El script de tu jugador
    public MurallaQueBaja murallaDelNivel; // El script de la muralla que hicimos antes

    private Animator anim;
    private NavMeshAgent agent;

    [Header("Atributos del Boss Mutante")]
    public float vidaTotal = 500f;
    private float vidaAtual;
    private bool estaMorto = false;
    private bool estaAtacar = false;

    [Header("Combate")]
    public float distanciaAtaque = 3.5f; // A qué distancia tira el golpe
    public float danoAtaque = 40f;
    public float cooldownAtaque = 2f; // Tiempo de descanso entre golpes

    [Header("Sonidos (¡Tus Enchufes!)")]
    public AudioSource audioSource;
    public AudioClip sonidoRugidoInicial;
    public AudioClip sonidoAtaque;
    public AudioClip sonidoMuerte;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        vidaAtual = vidaTotal;

        // Si le pones un sonido de rugido, sonará apenas aparezca/despierte
        if (audioSource != null && sonidoRugidoInicial != null)
        {
            audioSource.PlayOneShot(sonidoRugidoInicial);
        }
    }

    void Update()
    {
        // Si está muerto, no hay jugador, o está en medio de un ataque, no hace nada
        if (estaMorto || player == null || estaAtacar) return;

        float distanciaParaPlayer = Vector3.Distance(transform.position, player.position);

        if (distanciaParaPlayer <= distanciaAtaque)
        {
            StartCoroutine(RutinaAtaque());
        }
        else
        {
            PerseguirPlayer();
        }
    }

    private void PerseguirPlayer()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
        anim.SetBool("isWalking", true); // Activa la animación de movimiento
    }

    private void OlharParaPlayer()
    {
        Vector3 direcao = (player.position - transform.position).normalized;
        direcao.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direcao), Time.deltaTime * 5f);
    }

    IEnumerator RutinaAtaque()
    {
        estaAtacar = true;
        
        // Se detiene para pegar
        agent.isStopped = true;
        anim.SetBool("isWalking", false);
        OlharParaPlayer();

        // Llama a la animación "attack1" (o la que elijas)
        anim.SetTrigger("Attack");

        // Reproduce el sonido del golpe al instante
        if (audioSource != null && sonidoAtaque != null)
        {
            audioSource.PlayOneShot(sonidoAtaque);
        }

        // IMPORTANTE: Espera 1 segundo para que la animación llegue al momento del impacto real
        yield return new WaitForSeconds(1f);

        // Verifica si el jugador no se escapó rodando y sigue en rango
        if (Vector3.Distance(transform.position, player.position) <= distanciaAtaque + 1f)
        {
            if (scriptPlayer != null)
            {
                scriptPlayer.ReceberDano(danoAtaque, transform);
            }
        }

        // Tiempo de recuperación antes de volver a caminar/pegar
        yield return new WaitForSeconds(cooldownAtaque);
        estaAtacar = false;
    }

    // Esta es la función CLAVE que se comunica con las armas de tu equipo
    public void ReceberDano(float dano)
    {
        if (estaMorto) return;

        vidaAtual -= dano;

        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    private void Morrer()
    {
        estaMorto = true;
        
        // Detener todo movimiento
        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.enabled = false;
        }
        
        anim.SetBool("isWalking", false);
        anim.SetTrigger("Die"); // Llama a la animación "death1"

        // Grito de muerte
        if (audioSource != null && sonidoMuerte != null)
        {
            audioSource.PlayOneShot(sonidoMuerte);
        }

        // Apagar colisiones para que el jugador pueda pasar por encima
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // ¡MAGIA! Activamos la bajada de la muralla
        if (murallaDelNivel != null)
        {
            murallaDelNivel.ActivarBajada();
            Debug.Log("Jefe derrotado. Bajando la muralla...");
        }

        // Desaparecer el cuerpo después de 5 segundos
        Destroy(gameObject, 5f);
    }
}