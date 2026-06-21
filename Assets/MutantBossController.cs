using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class MutantBossController : MonoBehaviour
{   
    [Header("Sistema de Save")]
    public string idDoBoss = "Boss3";

    [Header("Referências Principais")]
    public NavMeshAgent agent;
    public Transform player;
    public PlayerMovement scriptPlayer; 
    public Animator anim;
    
    [Header("Mecânica do Nível")]
    public MurallaQueBaja murallaDelNivel; // Referencia para bajar la muralla al morir

    [Header("Distâncias")]
    public float distanciaDespertar = 15f; 
    public float distanciaParaMagia = 10f; 
    public float distanciaCorpoACorpo = 4f; 

    [Header("Ataques e Dano")]
    public float danoCorpoACorpo = 25f;
    public float cooldownSoco = 1.5f;
    public float cooldownMagia = 6f;
    private float tempoProximaMagia = 0f;

    [Header("Magia (Bola de Fogo)")]
    public GameObject bolaDeFogoPrefab;
    public Transform pontoDeDisparo;

    [Header("Status de Vida")]
    public Slider barraVidaBoss;
    public float vidaTotal = 300f;
    private float vidaAtual;
    private bool estaMorto = false;
    
    // VARIÁVEIS CINEMÁTICAS
    private bool estaAtacar = false; 
    private bool bossAtivo = false; 

    [Header("Sistema de Loot")]
    public GameObject[] cristaisParaDropar;

    void Start()
    {   
        if (PlayerPrefs.GetInt(idDoBoss, 0) == 1)
        {
            if (murallaDelNivel != null) murallaDelNivel.ActivarBajada(); 
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

        // 1. O DESPERTAR DO BOSS
        if (!bossAtivo)
        {
            if (distancia <= distanciaDespertar)
            {
                StartCoroutine(RotinaDespertar());
            }
            return; 
        }

        if (estaAtacar) return;

        OlharParaPlayer();

        // 2. CORPO A CORPO (Aquí el jefe ataca si estás cerca)
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
        // 4. PERSEGUIR O JOGADOR
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
        bossAtivo = true; 
        estaAtacar = true; 

        agent.isStopped = true;
        if (anim != null) anim.SetBool("isRunning", false);
        
        if (barraVidaBoss != null) barraVidaBoss.gameObject.SetActive(true);
        if (anim != null) anim.SetTrigger("wakeUp"); 

        yield return new WaitForSeconds(3f); 
        
        estaAtacar = false; 
    }

    IEnumerator RotinaSoco()
    {
        estaAtacar = true; 
        agent.isStopped = true;
        if (anim != null) anim.SetBool("isRunning", false);
        if (anim != null) anim.SetTrigger("attackMelee"); 

        // Espera un segundo para que la animación del golpe conecte visualmente
        yield return new WaitForSeconds(1f);

        // APLICACIÓN DE DAÑO AL JUGADOR
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

        // LA MUERTE DEL JEFE
        if (vidaAtual <= 0)
        {
            estaMorto = true;

            PlayerPrefs.SetInt(idDoBoss, 1);
            PlayerPrefs.Save();
            
            // 1. Activar la muralla
            if (murallaDelNivel != null)
            {
                murallaDelNivel.ActivarBajada();
            }

            // 2. Reproducir animación y apagar barra
            if (anim != null) anim.SetTrigger("die");
            if (barraVidaBoss != null) barraVidaBoss.gameObject.SetActive(false);

            // 3. Detener al jefe
            agent.isStopped = true;
            agent.enabled = false;
            
            // 4. Soltar Loot y destruir objeto
            DroparCristal();
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