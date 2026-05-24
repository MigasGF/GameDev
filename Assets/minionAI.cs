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

    void Start()
    {
        if (barraVidaInimigo != null)
        {
            barraVidaInimigo.maxValue = 100f;
            barraVidaInimigo.value = vidaAtual;
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

        if (vidaAtual <= 0)
        {
            estaMorto = true;
            anim.SetTrigger("die"); 
            agent.isStopped = true;
            agent.enabled = false;
            Destroy(gameObject, 3f); 
        }
    }
}