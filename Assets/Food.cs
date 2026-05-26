using UnityEngine;

public class Food : MonoBehaviour
{
    [Header("Configurações do Item")]
    public string nomeDaComida = "Maçã";
    public Sprite iconeUI; // A imagem que vai aparecer no inventário
    public float vidaRestaurada = 20f;
    
    // Quando o Esqueleto criar isto, pode vir com quantidades aleatórias (ex: 1 a 3)
    public int quantidade = 1; 

    [Header("Animação")]
    public float velocidadeRotacao = 50f;

    void Start()
    {
        // Define uma quantidade aleatória entre 1 e 3 ao nascer, se quiseres
        quantidade = Random.Range(1, 4);
    }

    void Update()
    {
        // Roda o objeto no eixo Y (para os lados) ao longo do tempo
        transform.Rotate(0f, velocidadeRotacao * Time.deltaTime, 0f);
    }
}