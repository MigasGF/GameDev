using UnityEngine;
using UnityEngine.AI;

public class InteligenciaEsqueleto : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player; 
    public Animator anim;
    public float distanciaParaAtacar = 2.0f;
    
    // Novas variáveis para controlar o tempo dos ataques
    public float tempoEntreAtaques = 1.5f; // Esqueleto ataca a cada 1.5 segundos
    private float temporizador; // Guarda o tempo que passou

    void Update()
    {
        // Medir a distância entre o esqueleto e o jogador
        float distancia = Vector3.Distance(transform.position, player.position);

        if (distancia > distanciaParaAtacar)
        {
            // Fora de alcance: Seguir o jogador
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            // Perto o suficiente: Parar
            agent.isStopped = true;
            
            // Fazer o esqueleto olhar para o jogador
            Vector3 direcaoOlhar = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.LookAt(direcaoOlhar);
            
            // Lógica de Ataque com Cooldown (Tempo de Recarga)
            if (Time.time >= temporizador)
            {
                Debug.Log("O script está a mandar atacar!");
                anim.SetTrigger("attack");
                // Define que o próximo ataque só pode acontecer daqui a 1.5 segundos
                temporizador = Time.time + tempoEntreAtaques; 
            }
        }

        // Atualizar a animação de correr/andar baseada na velocidade real
        anim.SetFloat("Speed", agent.velocity.magnitude);
    }
}