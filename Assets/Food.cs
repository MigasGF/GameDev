using UnityEngine;

public class Food : MonoBehaviour
{
    [Header("Configurações do Item")]
    public string nomeDaComida = "Maçã";
    public Sprite iconeUI; // A imagem que vai aparecer no inventário
    public float vidaRestaurada = 20f;
    public int quantidade = 1; 

    [Header("Comportamento")]
    public bool quantidadeAleatoria = true; // <--- ADICIONAMOS ESTA OPÇÃO!

    [Header("Animação")]
    public float velocidadeRotacao = 50f;

    void Start()
    {
        // Agora o código só inventa um número se a caixa estiver ativada!
        if (quantidadeAleatoria)
        {
            quantidade = Random.Range(1, 4);
        }
    }

    void Update()
    {
        // Roda o objeto no eixo Y (para os lados) ao longo do tempo
        transform.Rotate(0f, velocidadeRotacao * Time.deltaTime, 0f);
    }
}