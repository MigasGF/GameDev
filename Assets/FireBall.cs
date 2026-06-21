using UnityEngine;

public class FireBall : MonoBehaviour
{
    public float velocidade = 12f;
    public float dano = 20f;
    public float tempoDeVida = 5f; 

    void Start()
    {
        // Garante que a bola não viaja infinitamente pelo cenário
        Destroy(gameObject, tempoDeVida);
    }

    void Update()
    {
        // Voa sempre em linha reta na direção para onde foi disparada
        transform.Translate(Vector3.forward * velocidade * Time.deltaTime);
    }

private void OnTriggerEnter(Collider outro)
    {
        // ESTA LINHA VAI DENUNCIAR O CULPADO NA CONSOLA:
        Debug.Log("A bola de fogo explodiu porque bateu em: " + outro.name + " (Tag: " + outro.tag + ")");

        // 1. Ignora o Boss e Inimigos
        if (outro.CompareTag("Boss") || outro.CompareTag("Inimigo"))
        {
            return; 
        }

        // 2. Dá dano ao Player
        if (outro.CompareTag("Player")) 
        {
            PlayerMovement scriptPlayer = outro.GetComponent<PlayerMovement>();
            if (scriptPlayer != null)
            {
                scriptPlayer.ReceberDano(dano, transform);
            }
        }
        
        // 3. Destrói a bola
        Destroy(gameObject);
    }
}