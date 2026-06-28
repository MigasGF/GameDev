using UnityEngine;
using UnityEngine.UI;
using FMODUnity; // Obrigatório para o som FMOD funcionar
using UnityEngine.SceneManagement;

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

    // --- FEEDBACK DE DANO (TUTORIAL) ---
    [Header("Feedback de Dano")]
    public Image telaDeDano; 
    public Color corDoDano = new Color(1f, 0f, 0f, 0.4f); 
    public float velocidadeRecuperacao = 4f; 
    private bool levouPancada = false;
    public bool estaInvencivel = false;

    // --- PODERES (TUTORIAL) ---
    [Header("Poderes Mágicos")]
    public bool poderCorteAr = false;
    public GameObject prefabCorteAr;
    public Transform pontoDisparoCorte;

    // --- SISTEMA DE POSTURA / ESCUDO (TUTORIAL) ---
    [Header("Sistema de Postura / Escudo")]
    public GameObject objetoBarraBloqueio; 
    public Image mascaraDaBarra;  
    public RawImage imagemDaBarra; 
    
    public float posturaAtual = 0f;
    public float posturaMaxima = 100f;
    public float custoPosturaPorAtaque = 25f; 
    public float velocidadeRegeneracao = 15f; 
    private bool bloqueioQuebrado = false;

    [Header("Cores da Barra")]
    public Color corNormalBarra = Color.yellow;
    public Color corBarraQuebrada = Color.red;

    // --- VARIÁVEIS DO FMOD (SOUNDS) ---
    [Header("Sons FMOD - Armadura")]
    [field: SerializeField] private EventReference armorRattle;
    public float runIntensity = 1f;        // Medium
    public float jumpIntensity = 2f;       // Heavy
    public float attackIntensity = 1f;     // Medium
    public float blockIntensity = 2f;      // Heavy

    [Header("Sons FMOD - Espada")]
    [field: SerializeField] private EventReference swordSwing;
    [field: SerializeField] private EventReference swordImpact;
    public float swingDelay = 0f;

    [Header("Sons FMOD - Escudo")]
    [field: SerializeField] private EventReference shieldRaise;
    [field: SerializeField] private EventReference shieldBlock;

    [Header("Sons FMOD - Dano")]
    [field: SerializeField] private EventReference playerHit;

    [Header("Sons FMOD - Água")]
    [field: SerializeField] private EventReference splashIn;
    [field: SerializeField] private EventReference splashOut;
    [field: SerializeField] private EventReference swim;

    // --- WATER SOUND STATE ---
    private bool wasInWater = false;
    private FMOD.Studio.EventInstance swimInstance;
    private bool swimPlaying = false;

    [Header("Sistema de Morte")]
    public GameObject mensagemMorteUI; 
    

    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main.transform;
        anim.applyRootMotion = false;
        if (mensagemMorteUI != null) mensagemMorteUI.SetActive(false);

        // --- SISTEMA DE CHECKPOINTS ---
        if (PlayerPrefs.GetInt("TieneCheckpoint", 0) == 1)
        {
            float posX = PlayerPrefs.GetFloat("CheckpointX");
            float posY = PlayerPrefs.GetFloat("CheckpointY");
            float posZ = PlayerPrefs.GetFloat("CheckpointZ");
            
            // Apagamos el controller un milisegundo para poder teletransportar al jugador sin problemas físicos
            controller.enabled = false;
            transform.position = new Vector3(posX, posY, posZ);
            controller.enabled = true;
        }

        if (barraVidaPlayer != null)
        {
            barraVidaPlayer.maxValue = vidaMaxima;
            barraVidaPlayer.value = vidaAtual;
        }

        if (objetoBarraBloqueio != null) objetoBarraBloqueio.SetActive(false);
        if (mascaraDaBarra != null) mascaraDaBarra.fillAmount = 0f;
        if (imagemDaBarra != null) imagemDaBarra.color = corNormalBarra;

        anim.updateMode = AnimatorUpdateMode.UnscaledTime;

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

        // --- SHIELD RAISE SOUND (FMOD) ---
        // Só toca o som do escudo a levantar se a postura não estiver quebrada
        if (Input.GetMouseButtonDown(1) && !bloqueioQuebrado)
        {
            if (!shieldRaise.IsNull)
            {
                RuntimeManager.PlayOneShot(shieldRaise, transform.position);
            }
            PlayArmorRattle(blockIntensity);
        }

        // --- 2. SISTEMA DE GRAVIDADE, SALTO E ÁGUA ---
        bool noFundoDoPoco = transform.position.y <= limiteInferiorY;

        // --- WATER SOUND DETECTION ---
        if (noFundoDoPoco && !wasInWater)
        {
            // Just entered water — splash in
            if (!splashIn.IsNull)
            {
                RuntimeManager.PlayOneShot(splashIn, transform.position);
            }
        }
        else if (!noFundoDoPoco && wasInWater)
        {
            // Just left water — splash out and stop swim
            if (!splashOut.IsNull)
            {
                RuntimeManager.PlayOneShot(splashOut, transform.position);
            }
            StopSwimSound();
        }

        // --- SWIM LOOP LOGIC ---
        if (noFundoDoPoco)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            bool isMovingInWater = (h != 0f || v != 0f);

            if (isMovingInWater && !swimPlaying)
            {
                StartSwimSound();
            }
            else if (!isMovingInWater && swimPlaying)
            {
                StopSwimSound();
            }

            // Update swim sound position to follow player
            if (swimPlaying)
            {
                swimInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
            }
        }

        wasInWater = noFundoDoPoco;

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

        if (noFundoDoPoco)
        {
            controller.enabled = false;
            transform.position = new Vector3(transform.position.x, limiteInferiorY, transform.position.z);
            controller.enabled = true;

            if (velocidadeVertical < 0) velocidadeVertical = 0f;
        }

        if (controller.isGrounded || noFundoDoPoco)
        {
            if (controller.isGrounded && !noFundoDoPoco)
            {
                velocidadeVertical = -2f; 
            }
            
            if (Input.GetKeyDown(KeyCode.Space)) 
            {
                if (noFundoDoPoco) velocidadeVertical = forcaSaltoAgua;
                else velocidadeVertical = forcaSalto;

                anim.SetTrigger("jump");
                
                if (boia != null) boia.SetActive(false);
                anim.SetBool("inWater", false);
            }
        }

        if (!noFundoDoPoco || velocidadeVertical > 0)
        {
            velocidadeVertical += gravidade * Time.unscaledDeltaTime;
        }

        // --- 3. SISTEMA DE MOVIMENTO ---
        float hMove = Input.GetAxisRaw("Horizontal");
        float vMove = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(hMove, 0f, vMove).normalized;

        Vector3 movimentoFinal = Vector3.zero;

        if (direction.magnitude >= 0.1f)
        {
            anim.SetBool("isRunning", true);

            float anguloMovimento = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCamera.eulerAngles.y;
            Vector3 direcaoDoMovimento = Quaternion.Euler(0f, anguloMovimento, 0f) * Vector3.forward;

            if (vMove < 0) 
            {
                float anguloOlhar = mainCamera.eulerAngles.y; 
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, anguloOlhar, ref rotationVelocity, 0.25f, Mathf.Infinity, Time.unscaledDeltaTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }
            else
            {
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, anguloMovimento, ref rotationVelocity, 0.25f, Mathf.Infinity, Time.unscaledDeltaTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }

            movimentoFinal = direcaoDoMovimento * speed;
        }
        else
        {
            anim.SetBool("isRunning", false);
        }

        // --- 4. APLICAR MOVIMENTO ---
        movimentoFinal.y = velocidadeVertical; 
        controller.Move(movimentoFinal * Time.unscaledDeltaTime); 

        // --- 5. LÓGICA DE UI E POSTURA (TUTORIAL) ---
        if (telaDeDano != null)
        {
            if (levouPancada)
            {
                telaDeDano.color = corDoDano;
                levouPancada = false;
            }
            else
            {
                telaDeDano.color = Color.Lerp(telaDeDano.color, Color.clear, velocidadeRecuperacao * Time.unscaledDeltaTime);
            }
        }

        if (!bloqueioQuebrado && posturaAtual > 0)
        {
            posturaAtual -= velocidadeRegeneracao * Time.unscaledDeltaTime;
            if (posturaAtual < 0) posturaAtual = 0;
        }

        if (mascaraDaBarra != null) mascaraDaBarra.fillAmount = posturaAtual / posturaMaxima;
        if (objetoBarraBloqueio != null) objetoBarraBloqueio.SetActive(posturaAtual > 0);
    }

    // =========================================================
    // MÉTODOS DO FMOD (Recuperados da branch de sounds)
    // =========================================================
    
    public void PlayArmorRattle(float intensity)
    {
        if (armorRattle.IsNull) return;
        FMOD.Studio.EventInstance instance = RuntimeManager.CreateInstance(armorRattle);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        instance.setParameterByName("Intensity", intensity);
        instance.start();
        instance.release();
    }

    public void PlaySwordImpact(float enemyType)
    {
        if (swordImpact.IsNull) return;
        FMOD.Studio.EventInstance instance = RuntimeManager.CreateInstance(swordImpact);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        instance.setParameterByName("Enemy", enemyType);
        instance.start();
        instance.release();
    }

    // --- SWIM SOUND HELPERS ---
    private void StartSwimSound()
    {
        if (swim.IsNull) return;
        swimInstance = RuntimeManager.CreateInstance(swim);
        swimInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        swimInstance.start();
        swimPlaying = true;
    }

    private void StopSwimSound()
    {
        if (!swimPlaying) return;
        swimInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        swimInstance.release();
        swimPlaying = false;
    }

    void OnDestroy()
    {
        // Clean up swim instance if player is destroyed while swimming
        StopSwimSound();
    }

    // =========================================================
    // SISTEMA DE COMBATE (Fundido: FMOD + Tutorial)
    // =========================================================

    void AtacarInimigos()
    {
        InteligenciaEsqueleto[] esqueletos = FindObjectsByType<InteligenciaEsqueleto>(FindObjectsSortMode.None);
        
        foreach (InteligenciaEsqueleto esqueleto in esqueletos)
        {
            float distancia = Vector3.Distance(transform.position, esqueleto.transform.position);
            if (distancia <= alcanceDoAtaque)
            {
                esqueleto.ReceberDano(danoDoAtaque);
                PlaySwordImpact(esqueleto.enemySoundType); // Som FMOD
            }
        }

        BossController boss = FindObjectOfType<BossController>();
        if (boss != null)
        {
            Vector3 posicaoPlayerNoChao = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 posicaoBossNoChao = new Vector3(boss.transform.position.x, 0, boss.transform.position.z);
            float distanciaBoss = Vector3.Distance(posicaoPlayerNoChao, posicaoBossNoChao);
            
            if (distanciaBoss <= (alcanceDoAtaque + 5.0f)) 
            {
                boss.ReceberDano(danoDoAtaque);
                PlaySwordImpact(0f); // Som FMOD Boss
            }
        }
        
        IntelligenceBoss bossGolem = FindObjectOfType<IntelligenceBoss>();
        if (bossGolem != null)
        {
            Vector3 posicaoPlayerNoChao = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 posicaoBossNoChao = new Vector3(bossGolem.transform.position.x, 0, bossGolem.transform.position.z);
            float distanciaBoss = Vector3.Distance(posicaoPlayerNoChao, posicaoBossNoChao);
            
            if (distanciaBoss <= (alcanceDoAtaque + 5.0f)) 
            {
                bossGolem.ReceberDano(danoDoAtaque);
                PlaySwordImpact(0f); // Som FMOD Golem
            }
        }
        // =======================================================
        // NUEVO: Buscar y atacar al Nature Boss (MutantBossController)
        MutantBossController bossNature = FindObjectOfType<MutantBossController>();
        if (bossNature != null)
        {
            Vector3 posicaoPlayerNoChao = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 posicaoBossNoChao = new Vector3(bossNature.transform.position.x, 0, bossNature.transform.position.z);
            float distanciaBoss = Vector3.Distance(posicaoPlayerNoChao, posicaoBossNoChao);
            
            if (distanciaBoss <= (alcanceDoAtaque + 5.0f)) 
            {
                bossNature.ReceberDano(danoDoAtaque);
                PlaySwordImpact(0f); // Som FMOD Boss
                Debug.Log("Le pegaste al Nature Boss!");
            }
        }
        // =======================================================
    }

    public void ReceberDano(float dano, Transform atacante)
    {
        if (estaMorto || estaInvencivel) return;

        // Sistema de escudo (Fundido)
        if (Input.GetMouseButton(1) && !bloqueioQuebrado)
        {
            Vector3 direcaoDoAtaque = (atacante.position - transform.position).normalized;
            direcaoDoAtaque.y = 0; 
            float angulo = Vector3.Angle(transform.forward, direcaoDoAtaque);

            if (angulo <= 70f)
            {
                Debug.Log("Bloqueaste o ataque com o escudo de frente!");

                // Som do Escudo FMOD
                if (!shieldBlock.IsNull) RuntimeManager.PlayOneShot(shieldBlock, transform.position);
                PlayArmorRattle(blockIntensity);

                // Sistema de Quebra de Postura (Tutorial)
                posturaAtual += custoPosturaPorAtaque;
                if (posturaAtual >= posturaMaxima)
                {
                    StartCoroutine(RotinaQuebraBloqueio());
                }

                return; // Anula o dano
            }
        }

        levouPancada = true;

        // Toca o som de dano do jogador
        if (!playerHit.IsNull)
        {
            RuntimeManager.PlayOneShot(playerHit, transform.position);
        }

        vidaAtual -= dano;

        if (barraVidaPlayer != null)
            barraVidaPlayer.value = vidaAtual;

        if (vidaAtual <= 0)
        {
            estaMorto = true;
            anim.SetTrigger("die"); 
            controller.enabled = false;
            if (mensagemMorteUI != null) mensagemMorteUI.SetActive(true);
            Debug.Log("Morreste!");
            StartCoroutine(RotinaRecarregarCena());
        }
    }

    System.Collections.IEnumerator RotinaDeAtaque()
    {
        estaAAtacar = true;
        
        if (scriptEspada != null) scriptEspada.PrepararNovoAtaque(); 
        if (colliderEspada != null) colliderEspada.enabled = true; 
        
        anim.SetTrigger("bash");

        // FMOD Sword Swing com Delay
        if (swingDelay > 0f) yield return new WaitForSecondsRealtime(swingDelay);
        if (!swordSwing.IsNull) RuntimeManager.PlayOneShot(swordSwing, transform.position);
        PlayArmorRattle(attackIntensity);

        // Disparo da magia do Tutorial (Se existir)
        if (poderCorteAr && prefabCorteAr != null && pontoDisparoCorte != null)
        {
            yield return new WaitForSecondsRealtime(0.2f);
            Instantiate(prefabCorteAr, pontoDisparoCorte.position, transform.rotation);
            yield return new WaitForSecondsRealtime(0.3f); // Espera o resto do tempo
        }
        else
        {
            yield return new WaitForSecondsRealtime(0.5f); 
        }

        // Danos calculados por script (FMOD version)
        AtacarInimigos(); 
        
        yield return new WaitForSecondsRealtime(0.5f); 
        
        if (colliderEspada != null) colliderEspada.enabled = false; 
        estaAAtacar = false;
    }

    System.Collections.IEnumerator RotinaQuebraBloqueio()
    {
        bloqueioQuebrado = true;
        posturaAtual = posturaMaxima; 

        if (mascaraDaBarra != null) mascaraDaBarra.fillAmount = 1f; 
        if (imagemDaBarra != null) imagemDaBarra.color = corBarraQuebrada; 

        Debug.Log("DEFESA QUEBRADA! Ficas atordoado por 5 segundos!");

        yield return new WaitForSecondsRealtime(5f);

        bloqueioQuebrado = false;
        posturaAtual = 0f;

        if (mascaraDaBarra != null) mascaraDaBarra.fillAmount = 0f; 
        if (imagemDaBarra != null) imagemDaBarra.color = corNormalBarra; 

        Debug.Log("Recuperaste a postura. Já podes bloquear de novo!");
    }
    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si el jugador chocó con un objeto que tiene la etiqueta "Checkpoint"
        if (other.CompareTag("Checkpoint"))
        {
            // Guardamos las coordenadas del cubo
            PlayerPrefs.SetFloat("CheckpointX", other.transform.position.x);
            PlayerPrefs.SetFloat("CheckpointY", other.transform.position.y);
            PlayerPrefs.SetFloat("CheckpointZ", other.transform.position.z);
            PlayerPrefs.SetInt("TieneCheckpoint", 1);
            PlayerPrefs.Save();
            
            Debug.Log("¡Checkpoing guardado exitosamente desde el Caballero!");
            
            // Apagamos el collider del cubo para no guardar infinitas veces
            other.enabled = false; 
        }
    }
    System.Collections.IEnumerator RotinaRecarregarCena()
    {
        // Esperamos 3 segundos reales para que la animación de muerte termine
        yield return new WaitForSecondsRealtime(5f);
        
        // Recargamos la escena en la que estamos actualmente
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
