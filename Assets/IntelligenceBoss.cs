using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class IntelligenceBoss : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public Animator anim;

    [Header("Distâncias")]
    public float distanciaParaMagia = 10f; 
    public float distanciaCorpoACorpo = 3f; 

    [Header("Ataques e Tempos")]
    // TEMPOS SEPARADOS:
    public float tempoEntreMagias = 6f; // Demora muito a carregar fogo
    private float temporizadorMagia;
    
    public float tempoEntreSocos = 1.5f; // Bate rápido se estiveres perto
    private float temporizadorSoco;
    
    public float danoCorpoACorpo = 25f;

    [Header("Magia (Bola de Fogo)")]
    public GameObject bolaDeFogoPrefab;
    public Transform pontoDeDisparo;

    [Header("Vida")]
    public Slider barraVidaBoss;
    public float vidaAtual = 300f;
    private bool estaMorto = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        if (barraVidaBoss != null)
        {
            barraVidaBoss.maxValue = vidaAtual;
            barraVidaBoss.value = vidaAtual;
        }
    }

    void Update()
    {
        if (estaMorto || player == null) return;

        float distancia = Vector3.Distance(transform.position, player.position);

        // =========================================================================
        // 1. CORPO A CORPO (Usa o temporizadorSoco)
        // =========================================================================
        if (distancia <= distanciaCorpoACorpo)
        {
            agent.isStopped = true;
            if (anim != null) anim.SetBool("isRunning", false);

            Vector3 direcaoOlhar = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.LookAt(direcaoOlhar);

            // Pergunta ao relógio dos socos se já pode bater
            if (Time.time >= temporizadorSoco)
            {
                if (anim != null) anim.SetTrigger("attackMelee"); 
                AtaqueFisico();
                temporizadorSoco = Time.time + tempoEntreSocos; // Reinicia SÓ o relógio do soco
            }
        }
        // =========================================================================
        // 2. MAGIA (Usa o temporizadorMagia)
        // =========================================================================
        else if (distancia <= distanciaParaMagia && Time.time >= temporizadorMagia)
        {
            agent.isStopped = true; 
            if (anim != null) anim.SetBool("isRunning", false);

            Vector3 direcaoOlhar = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.LookAt(direcaoOlhar);

            if (anim != null) anim.SetTrigger("attackRanged"); 
            Invoke("AtirarMagia", 0.5f); 

            temporizadorMagia = Time.time + tempoEntreMagias; // Reinicia SÓ o relógio da magia
        }
        // =========================================================================
        // 3. CORRER (Se a magia estiver em cooldown e não estiveres perto para soco)
        // =========================================================================
        else
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            if (anim != null) anim.SetBool("isRunning", true);
        }
    }

    void AtirarMagia()
    {
        if (bolaDeFogoPrefab != null && pontoDeDisparo != null)
        {
            Instantiate(bolaDeFogoPrefab, pontoDeDisparo.position, transform.rotation);
        }
    }

    void AtaqueFisico()
    {
       void AtaqueFisico()
    {
        PlayerMovement scriptPlayer = player.GetComponent<PlayerMovement>();
        
        if (scriptPlayer != null)
        {
            // Enviamos o dano do soco e o Transform do próprio Boss (transform) para o escudo funcionar!
            scriptPlayer.ReceberDano(danoCorpoACorpo, transform);
            Debug.Log("Boss desferiu um soco! Dano: " + danoCorpoACorpo);
        }
        else
        {
            Debug.LogWarning("AVISO: O objeto Player não tem o script PlayerMovement colado!");
        }
    }
    }

    public void ReceberDano(float dano)
    {
        if (estaMorto) return;

        vidaAtual -= dano;
        if (barraVidaBoss != null) barraVidaBoss.value = vidaAtual;

        if (vidaAtual <= 0)
        {
            estaMorto = true;
            if (anim != null) anim.SetTrigger("die");
            agent.isStopped = true;
            agent.enabled = false;
            Destroy(gameObject, 5f);
        }
    }
}