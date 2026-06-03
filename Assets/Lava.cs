using UnityEngine;

public class Lava : MonoBehaviour
{
    [Header("Configurações da Lava")]
    public float danoPorSegundo = 20f; // Tira 20 de vida
    private float tempoProximoDano = 0f; // O cronómetro interno da lava

    // OnTriggerStay corre constantemente enquanto o jogador estiver "dentro" da lava
    void OnTriggerStay(Collider other)
    {
        // Verifica se quem está a tocar na lava é o jogador
        if (other.CompareTag("Player"))
        {
            // Verifica se já passou 1 segundo desde o último dano
            if (Time.time >= tempoProximoDano)
            {
                // Vai buscar o script do jogador para lhe tirar vida
                PlayerMovement playerScript = other.GetComponent<PlayerMovement>();
                
                if (playerScript != null)
                {
                    Debug.Log("Lava a queimar! -20 de vida.");
                    playerScript.ReceberDano(danoPorSegundo, transform);
                }

                // Reinicia o cronómetro para dar dano outra vez daqui a 1 segundo
                tempoProximoDano = Time.time + 1f; 
            }
        }
    }
}