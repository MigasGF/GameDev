using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BossController : MonoBehaviour
{   
    [Header("Sistema de Save")]
    public string idDoBoss = "Boss1";

    [Header("Referências")]
    public Transform player;
    public PlayerMovement scriptPlayer; // Referência ao teu script
    public Transform magicPoint; // O Empty Object na mão do boss
    public LineRenderer magicBeam; // O Line Renderer do feixe
    public MurallaQueBaja murallaDelNivel;

    private Animator anim;
    private NavMeshAgent agent;

    [Header("Atributos do Boss")]
    public float vidaTotal = 500f;
    private float vidaAtual;
    private bool estaMorto = false;
    private bool bossAtivo = false; // Fica true quando o player entra na arena
    private bool estaAtacar = false;

    [Header("Distâncias de Combate")]
    public float distanciaDespertar = 15f; // Distância para o boss gritar e acordar
    public float distanciaSoco = 3f; // Distância para ataque Melee
    public float distanciaMagia = 10f; // Distância mínima para começar a afastar e usar magia

    [Header("Dano")]
    public float danoSoco = 40f;
    public float danoMagiaPorSegundo = 15f;

    [Header("UI do Boss")]
    public Slider barraVidaBoss;

    [Header("Desvio")]
    public float desvio = -60f;

    [Header("Espera")]
    public float espera = 0f;

    [Header("Cooldown")]
    public float cooldown = 6f;

    [Header("Duracao")]

    public float duracaoFeixe = 1f;

    [Header("Recarga")]
    public float tempoProximaMagia = 0f; // Controla o tempo de recarga do laser

    [Header("Efeitos Visuais")]
    public GameObject charging;

    [Header("Sistema de Loot")]
    // Estes parênteses retos [] significam que é uma "lista" onde podes pôr vários objetos
    public GameObject[] cristaisParaDropar;

    void Start()
    {   
        if (PlayerPrefs.GetInt(idDoBoss, 0) == 1)
        {
            if (murallaDelNivel != null) murallaDelNivel.ActivarBajada(); // Abre logo a porta!
            Destroy(gameObject); // O Boss não chega a nascer
            return;
        }

        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        
        vidaAtual = vidaTotal;
        
        if (magicBeam != null)
            magicBeam.enabled = false;

        if (barraVidaBoss != null)
        {
            barraVidaBoss.maxValue = vidaTotal;
            barraVidaBoss.value = vidaAtual;
            barraVidaBoss.gameObject.SetActive(false); // Esconde a barra no início
        }
    }

void Update()
    {
        if (estaMorto || player == null) return;

        // Esto obliga al Boss a morir si le pones un 0 en el Inspector
        if (vidaAtual <= 0)
        {
            if (murallaDelNivel != null)
            {
                murallaDelNivel.ActivarBajada();
            }
            Morrer();
            return; 
        }

        float distanciaParaPlayer = Vector3.Distance(transform.position, player.position);

        // 1. Lógica de Despertar
        if (!bossAtivo)
        {
            if (distanciaParaPlayer <= distanciaDespertar)
            {
                StartCoroutine(RotinaDespertar());
            }
            return;
        }

        if (estaAtacar) return; 

        // 2. Tomada de Decisão (Movimento e Ataques)
        OlharParaPlayer();

        if (distanciaParaPlayer <= distanciaSoco)
        {
            StartCoroutine(RotinaSoco());
        }
        // Só usa magia se estiver longe E o tempo de recarga já tiver passado!
        else if (distanciaParaPlayer >= distanciaMagia && Time.time >= tempoProximaMagia)
        {
             // 6 segundos de cooldown antes de poder usar o laser outra vez
            tempoProximaMagia = Time.time + cooldown;
            StartCoroutine(RotinaMagia());
        }
        else
        {
            // Perseguir o Player (Walk)
            agent.isStopped = false;
            agent.SetDestination(player.position);
            anim.SetBool("isWalking", true);
        }
    }

    private void OlharParaPlayer()
    {
        Vector3 direcao = (player.position - transform.position).normalized;
        direcao.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direcao), Time.deltaTime * 5f);
    }

    // --- ROTINAS DE ESTADO ---

    IEnumerator RotinaDespertar()
    {
        bossAtivo = true;
        estaAtacar = true;

        if (barraVidaBoss != null) barraVidaBoss.gameObject.SetActive(true);
        
        anim.SetTrigger("scream"); // Ativa Zombie Scream
        // Espera o tempo da animação acabar (Ajusta este valor para o tempo exato do Zombie Scream)
        yield return new WaitForSeconds(3f); 
        
        estaAtacar = false;
    }

    IEnumerator RotinaSoco()
    {
        estaAtacar = true;
        PararMovimento();
        
        anim.SetTrigger("punch");
        
        // Espera pelo "hit" da animação
        yield return new WaitForSeconds(1f); 
        
        // Verifica se o player ainda está perto para levar o hit
        if (Vector3.Distance(transform.position, player.position) <= distanciaSoco + 1f)
        {
            scriptPlayer.ReceberDano(danoSoco, transform);
        }

        // Tempo de cooldown do soco
        yield return new WaitForSeconds(1.5f); 
        estaAtacar = false;
    }

IEnumerator RotinaMagia()
    {
        estaAtacar = true;
        PararMovimento();
        
        anim.SetTrigger("magic"); 
        
        // --- 1. COMEÇA A ACUMULAR ENERGIA ---
        GameObject efeitoEnergia = null; // Guardamos a referência aqui
        if (charging != null)
        {
            // Cria a energia exatamente na posição e rotação da mão
            efeitoEnergia = Instantiate(charging, magicPoint.position, magicPoint.rotation);
            // Ao fazer SetParent, o VFX cola-se à mão e acompanha o movimento da animação!
            efeitoEnergia.transform.SetParent(magicPoint);
        }

        // Espera que as mãos cheguem à posição de disparo (enquanto o VFX brilha)
        yield return new WaitForSeconds(espera); 
        
        // --- 2. ACABOU O TEMPO DE CARREGAR ---
        if (efeitoEnergia != null)
        {
            Destroy(efeitoEnergia); // Apagamos a bola de energia...
        }

        // --- 3. DISPARA O LASER MORTAL ---
        if (magicBeam != null) magicBeam.enabled = true; // ...e ligamos o Laser!

        float tempoAtivo = 0f;
         // Tempo que o laser fica ligado

        while (tempoAtivo < duracaoFeixe)
        {
            tempoAtivo += Time.deltaTime;

            // A ROTAÇÃO COMPENSADA
            Vector3 direcaoOlhar = (player.position - transform.position).normalized;
            direcaoOlhar.y = 0; 
            
            // Aplicamos o "desvio" para obrigar o corpo a ficar de lado, mas as mãos viradas para ti
            Quaternion rotacaoAlvo = Quaternion.LookRotation(direcaoOlhar) * Quaternion.Euler(0, desvio, 0);
            
            // Aumentei a velocidade de rotação para 5f para ele mirar mais rápido
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacaoAlvo, Time.deltaTime * 5f); 

            // O LASER DIRETO
            // Aponta diretamente para o peito do jogador (+ Vector3.up * 1f)
            Vector3 direcaoFeixe = (player.position + Vector3.up * 1f) - magicPoint.position;

            magicBeam.SetPosition(0, magicPoint.position);

            if (Physics.Raycast(magicPoint.position, direcaoFeixe, out RaycastHit hit, 50f))
            {
                magicBeam.SetPosition(1, hit.point); 
                
                if (hit.collider.CompareTag("Player"))
                {
                    scriptPlayer.ReceberDano(danoMagiaPorSegundo * Time.deltaTime, transform);
                }
            }
            else
            {
                magicBeam.SetPosition(1, magicPoint.position + direcaoFeixe.normalized * 50f);
            }

            yield return null;
        }

        if (magicBeam != null) magicBeam.enabled = false;
        
        yield return new WaitForSeconds(2f);
        estaAtacar = false;
    }

    private void PararMovimento()
    {
        agent.isStopped = true;
        anim.SetBool("isWalking", false);
        agent.velocity = Vector3.zero;
    }

    // Função pública para o Boss receber dano das tuas espadas/magias
    public void ReceberDano(float dano)
    {
        if (estaMorto) return;

        vidaAtual -= dano;
        
        if (barraVidaBoss != null) barraVidaBoss.value = vidaAtual;

        if (vidaAtual <= 0)
        {
            if (murallaDelNivel != null)
            {
                murallaDelNivel.ActivarBajada();
            }
            Morrer();
        }
    }

    private void Morrer()
    {
        estaMorto = true;
        PararMovimento();

        PlayerPrefs.SetInt(idDoBoss, 1);
        PlayerPrefs.Save();
        
        if (magicBeam != null) magicBeam.enabled = false;

        if (barraVidaBoss != null) barraVidaBoss.gameObject.SetActive(false);
        
        anim.SetTrigger("die"); // Mutant Dying
        
        // Desativa a colisão e agente de navegação para o Boss não ser mais um obstáculo
        GetComponent<Collider>().enabled = false;
        agent.enabled = false;
        
        DroparCristal();
        
        Destroy(gameObject, 5f); // Destrói o corpo do boss após 5 segundos (opcional)
    }

    void DroparCristal()
    {
        // Verifica se tens cristais na lista
        if (cristaisParaDropar != null && cristaisParaDropar.Length > 0)
        {
            // O Unity escolhe um número à sorte entre 0 e o número de cristais que lá puseste
            int indiceSorteado = Random.Range(0, cristaisParaDropar.Length);
            
            GameObject cristalEscolhido = cristaisParaDropar[indiceSorteado];

            if (cristalEscolhido != null)
            {
                // Cria o cristal na posição do Boss, mas um bocadinho mais acima (Vector3.up) para não ficar enterrado no chão
                Instantiate(cristalEscolhido, transform.position + Vector3.up * 1.5f, Quaternion.identity);
                Debug.Log("O Boss dropou o item: " + cristalEscolhido.name);
            }
        }
    }
}