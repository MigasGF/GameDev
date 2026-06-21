using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class IntelligenceBoss : MonoBehaviour
{   
    [Header("Sistema de Save")]
    public string idDoBoss = "Boss2";

    [Header("Referências")]
    public NavMeshAgent agent;
    public Transform player;
    public PlayerMovement scriptPlayer; 
    public Animator anim;
    public MurallaQueBaja murallaDelNivel;

    // ==========================================
    // NOVO: Referência para a parede da Arena!
    [Header("Eventos da Arena")]
    public MurallaQueBaja paredeDaArena; 
    // ==========================================

    [Header("Distâncias")]
    public float distanciaDespertar = 15f; // Distância para ele acordar e gritar!
    public float distanciaParaMagia = 10f; 
    public float distanciaCorpoACorpo = 4f; 

    [Header("Ataques")]
    public float danoCorpoACorpo = 25f;
    public float cooldownSoco = 1.5f;
    public float cooldownMagia = 6f;
    private float tempoProximaMagia = 0f;

    [Header("Magia (Bola de Fogo)")]
    public GameObject bolaDeFogoPrefab;
    public Transform pontoDeDisparo;

    [Header("Vida")]
    public Slider barraVidaBoss;
    public float vidaTotal = 300f;
    private float vidaAtual;
    private bool estaMorto = false;
    
    // VARIÁVEIS CINEMÁTICAS
    private bool estaAtacar = false; 
    private bool bossAtivo = false; // Começa a dormir (parado)

    [Header("Sistema de Loot")]
    // Estes parênteses retos [] significam que é uma "lista" onde podes pôr vários objetos
    public GameObject[] cristaisParaDropar;

    void Start()
    {   
        if (PlayerPrefs.GetInt(idDoBoss, 0) == 1)
        {
            if (murallaDelNivel != null) murallaDelNivel.ActivarBajada();
            if (paredeDaArena != null) paredeDaArena.ActivarBajada(); // Este tem duas paredes!
            Destroy(gameObject); 
            return;
        }

        agent = GetComponent<NavMeshAgent>();
        vidaAtual = vidaTotal;
        
        if (barraVidaBoss != null)
        {
            barraVidaBoss.maxValue = vidaTotal;
            barraVidaBoss.value = vidaAtual;
            barraVidaBoss.gameObject.SetActive(false); 
        }
    }

    void Update()
    {
        if (estaMorto || player == null) return;

        float distancia = Vector3.Distance(transform.position, player.position);

        // ==========================================================
        // 1. O DESPERTAR DO BOSS (O INÍCIO ÉPICO)
        // ==========================================================
        if (!bossAtivo)
        {
            if (distancia <= distanciaDespertar)
            {
                StartCoroutine(RotinaDespertar());
            }
            return; // Impede que o resto do código corra enquanto ele dorme/grita
        }

        // ==========================================================

        if (estaAtacar) return;

        OlharParaPlayer();

        // 2. CORPO A CORPO
        if (distancia <= distanciaCorpoACorpo)
        {
            StartCoroutine(RotinaSoco());
        }
        // 3. MAGIA (BOLA DE FOGO)
        else if (distancia <= distanciaParaMagia && Time.time >= tempoProximaMagia)
        {
            tempoProximaMagia = Time.time + cooldownMagia;
            StartCoroutine(RotinaMagia());
        }
        // 4. CORRER ATRÁS DO JOGADOR
        else
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            if (anim != null) anim.SetBool("isRunning", true);
        }
    }

    private void OlharParaPlayer()
    {
        Vector3 direcao = (player.position - transform.position).normalized;
        direcao.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direcao), Time.deltaTime * 5f);
    }

    IEnumerator RotinaDespertar()
    {
        bossAtivo = true; // Já foi acordado!
        estaAtacar = true; // Tranca o cérebro para não se mexer enquanto grita

        agent.isStopped = true;
        if (anim != null) anim.SetBool("isRunning", false);
        
        // Liga a barra de vida e ativa a animação de medo!
        if (barraVidaBoss != null) barraVidaBoss.gameObject.SetActive(true);
        if (anim != null) anim.SetTrigger("wakeUp"); 

        // IMPORTANTE: Espera que a animação inicial acabe (Ajusta este número aos segundos que a animação dura!)
        yield return new WaitForSeconds(3f); 
        
        estaAtacar = false; // Destranca o cérebro, agora vem a porrada!
    }

    IEnumerator RotinaSoco()
    {
        estaAtacar = true; 
        agent.isStopped = true;
        if (anim != null) anim.SetBool("isRunning", false);
        if (anim != null) anim.SetTrigger("attackMelee"); 

        yield return new WaitForSeconds(1f);

        if (Vector3.Distance(transform.position, player.position) <= distanciaCorpoACorpo + 1f)
        {
            if (scriptPlayer != null)
            {
                scriptPlayer.ReceberDano(danoCorpoACorpo, transform);
            }
        }

        yield return new WaitForSeconds(cooldownSoco);
        estaAtacar = false; 
    }

    IEnumerator RotinaMagia()
    {
        estaAtacar = true; 
        agent.isStopped = true;
        if (anim != null) anim.SetBool("isRunning", false);
        if (anim != null) anim.SetTrigger("attackRanged"); 

        yield return new WaitForSeconds(0.5f);

        if (bolaDeFogoPrefab != null && pontoDeDisparo != null)
        {
            Instantiate(bolaDeFogoPrefab, pontoDeDisparo.position, transform.rotation);
        }

        yield return new WaitForSeconds(1.5f);
        estaAtacar = false; 
    }

    public void ReceberDano(float dano)
    {
        if (estaMorto) return;

        vidaAtual -= dano;
        if (barraVidaBoss != null) barraVidaBoss.value = vidaAtual;

        if (vidaAtual <= 0)
        {   
            PlayerPrefs.SetInt(idDoBoss, 1);
            PlayerPrefs.Save();

            if (murallaDelNivel != null)
            {
                murallaDelNivel.ActivarBajada();
            }
            
            estaMorto = true;
            if (anim != null) anim.SetTrigger("die");
            if (barraVidaBoss != null) barraVidaBoss.gameObject.SetActive(false);

            agent.isStopped = true;
            agent.enabled = false;
            DroparCristal();

            // ==========================================================
            // NOVO: Manda a parede descer se ela estiver configurada!
            if (paredeDaArena != null)
            {
                paredeDaArena.ActivarBajada();
            }
            // ==========================================================

            Destroy(gameObject, 5f);
        }
    }

    void DroparCristal()
    {
        if (cristaisParaDropar != null && cristaisParaDropar.Length > 0)
        {
            int indiceSorteado = Random.Range(0, cristaisParaDropar.Length);
            
            GameObject cristalEscolhido = cristaisParaDropar[indiceSorteado];

            if (cristalEscolhido != null)
            {
                Instantiate(cristalEscolhido, transform.position + Vector3.up * 1.5f, Quaternion.identity);
                Debug.Log("O Boss dropou o item: " + cristalEscolhido.name);
            }
        }
    }
}