using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI; // Obriga o script a perceber o que é um "Slider"

public class InteligenciaEsqueleto : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player; 
    public Animator anim;
    public float distanciaParaAtacar = 2.0f;
    
    public float tempoEntreAtaques = 1.5f; 
    private float temporizador; 

    // --- SISTEMA DE VIDA ---
    public Slider barraVidaInimigo; // Arrasta a barra de vida do esqueleto para aqui
    public float vidaAtual = 100f;
    public float danoDoAtaque = 15f; // Quanto de vida ele tira ao Cavaleiro
    private bool estaMorto = false;

    void Start()
    {
        // Configura a barra no início
        if (barraVidaInimigo != null)
        {
            barraVidaInimigo.maxValue = 100f;
            barraVidaInimigo.value = vidaAtual;
        }
    }

    void Update()
    {
        if (estaMorto) return; // Se estiver morto, ignora o resto do código!

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
                
                // Vai buscar o script do jogador e dá-lhe dano!
                PlayerMovement scriptPlayer = player.GetComponent<PlayerMovement>();
                if (scriptPlayer != null)
                {
                    scriptPlayer.ReceberDano(danoDoAtaque);
                }

                temporizador = Time.time + tempoEntreAtaques; 
            }
        }

        anim.SetFloat("Speed", agent.velocity.magnitude);
    }

    // O Cavaleiro vai chamar esta função para magoar o esqueleto
    public void ReceberDano(float dano)
    {
        if (estaMorto) return;

        vidaAtual -= dano;
        if (barraVidaInimigo != null) barraVidaInimigo.value = vidaAtual;

        if (vidaAtual <= 0)
        {
            estaMorto = true;
            anim.SetTrigger("die"); // Se tiveres animação de morrer, certifica-te que o parâmetro se chama "die"
            agent.isStopped = true;
            agent.enabled = false;
            Destroy(gameObject, 3f); // O corpo desaparece ao fim de 3 segundos
        }
    }
}