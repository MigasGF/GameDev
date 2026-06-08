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

    // Deteta quando colide com o jogador ou com o cenário
    private void OnTriggerEnter(Collider outro)
    {
        // Se tocar no Cavaleiro (que tem a Tag "Player")
        if (outro.CompareTag("Player")) 
        {
            PlayerMovement scriptPlayer = outro.GetComponent<PlayerMovement>();
            if (scriptPlayer != null)
            {
                // Envia o dano e a própria posição da bola para o cálculo do escudo direcional!
                scriptPlayer.ReceberDano(dano, transform);
            }
        }
        
        // Explode/Desaparece mal toque em qualquer obstáculo (chão, paredes ou jogador)
        Destroy(gameObject);
    }
}