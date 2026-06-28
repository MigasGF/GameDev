using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using FMODUnity;

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

    [Header("Sistema de Vida")]
    public Slider barraVidaInimigo;
    public float vidaAtual = 100f;
    public float danoDoAtaque = 15f;
    private bool estaMorto = false;

    [Header("Sound Settings")]
    public float enemySoundType = 0f;

    [Header("FMOD - Áudio 3D")]
    [field: SerializeField] private EventReference skeletonDeath;
    [field: SerializeField] private EventReference skeletonFootsteps;

    [Header("Footsteps")]
    [Tooltip("Valor enviado para o parâmetro Speed do evento FMOD dos passos.")]
    [Range(0.5f, 3f)]
    public float footstepsSpeed = 1f;

    private FMOD.Studio.EventInstance footstepsInstance;
    private bool footstepsPlaying = false;

    [Header("Drop System")]
    public GameObject[] comidasParaDropar;
    [Range(0f, 1f)] public float chanceDeDrop = 0.5f;

    [Header("Efeitos Visuais de Dano")]
    public Renderer modelo3D;
    public Color corPiscar = Color.red;
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
            coresOriginais = new Color[modelo3D.materials.Length];

            for (int i = 0; i < modelo3D.materials.Length; i++)
            {
                coresOriginais[i] = modelo3D.materials[i].color;
            }
        }

        if (!skeletonFootsteps.IsNull)
        {
            footstepsInstance = RuntimeManager.CreateInstance(skeletonFootsteps);

            // Áudio 3D: o som fica associado à posição do esqueleto no mundo
            footstepsInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));

            // Parâmetro FMOD para controlar a rapidez dos passos
            footstepsInstance.setParameterByName("Speed", footstepsSpeed);
        }
    }

    void Update()
    {
        if (estaMorto) return;

        float distancia = Vector3.Distance(transform.position, player.position);

        bool puedeVerAlJugador = (distanciaDeVision <= 0f) || (distancia <= distanciaDeVision);

        if (puedeVerAlJugador)
        {
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
        }
        else
        {
            agent.isStopped = true;
        }

        float velocidade = agent.velocity.magnitude;

        anim.SetFloat("Speed", velocidade);

        // ---------- FMOD FOOTSTEPS 3D ----------
        if (!skeletonFootsteps.IsNull && footstepsInstance.isValid())
        {
            // Atualiza continuamente a posição 3D do som
            footstepsInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));

            // Atualiza continuamente o parâmetro Speed no FMOD
            footstepsInstance.setParameterByName("Speed", footstepsSpeed);

            if (velocidade > 0.1f)
            {
                if (!footstepsPlaying)
                {
                    footstepsInstance.start();
                    footstepsPlaying = true;
                }
            }
            else
            {
                if (footstepsPlaying)
                {
                    footstepsInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                    footstepsPlaying = false;
                }
            }
        }
    }

    public void ReceberDano(float dano)
    {
        if (estaMorto) return;

        vidaAtual -= dano;

        if (barraVidaInimigo != null)
            barraVidaInimigo.value = vidaAtual;

        if (modelo3D != null)
            StartCoroutine(EfeitoPiscar());

        if (vidaAtual <= 0)
        {
            estaMorto = true;

            // ---------- FMOD DEATH 3D ----------
            if (!skeletonDeath.IsNull)
            {
                RuntimeManager.PlayOneShot(skeletonDeath, transform.position);
            }

            // ---------- PARAR FOOTSTEPS ----------
            if (footstepsInstance.isValid())
            {
                footstepsInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                footstepsInstance.release();
                footstepsPlaying = false;
            }

            anim.SetTrigger("die");

            agent.isStopped = true;
            agent.enabled = false;

            Destroy(gameObject, 3f);

            if (Random.value <= chanceDeDrop && comidasParaDropar.Length > 0)
            {
                int indexAleatorio = Random.Range(0, comidasParaDropar.Length);
                GameObject comidaEscolhida = comidasParaDropar[indexAleatorio];

                Vector3 posicaoDrop = transform.position + new Vector3(0, 1f, 0);
                Instantiate(comidaEscolhida, posicaoDrop, Quaternion.identity);
            }
        }
    }

    System.Collections.IEnumerator EfeitoPiscar()
    {
        for (int i = 0; i < modelo3D.materials.Length; i++)
        {
            modelo3D.materials[i].color = corPiscar;
        }

        yield return new WaitForSeconds(0.15f);

        for (int i = 0; i < modelo3D.materials.Length; i++)
        {
            modelo3D.materials[i].color = coresOriginais[i];
        }
    }

    private void OnDestroy()
    {
        if (footstepsInstance.isValid())
        {
            footstepsInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            footstepsInstance.release();
        }
    }
}