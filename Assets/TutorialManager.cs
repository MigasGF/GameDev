using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("UI do Tutorial")]
    [Tooltip("Arrasta para aqui todos os painéis por ordem (Página 1, Página 2, etc)")]
    public GameObject[] paineisTutorial; 
    
    private int paginaAtual = 0;

    void Start()
    {
        if (paineisTutorial.Length > 0)
        {
            // Primeiro, garante que todos os painéis estão desligados
            foreach (GameObject painel in paineisTutorial)
            {
                painel.SetActive(false);
            }

            // Liga apenas o primeiro (Página 0)
            paineisTutorial[0].SetActive(true);
            
            // Pausa o jogo
            Time.timeScale = 0f; 
        }
    }

    void Update()
    {
        // Se ainda houver páginas para mostrar e o jogador carregar no Enter
        if (paginaAtual < paineisTutorial.Length && Input.GetKeyDown(KeyCode.Return))
        {
            AvancarPagina();
        }
    }

    public void AvancarPagina()
    {
        // Desliga a página que estava a ser mostrada
        paineisTutorial[paginaAtual].SetActive(false);

        // Avança o número da página
        paginaAtual++;

        // Verifica se a nova página ainda existe na lista
        if (paginaAtual < paineisTutorial.Length)
        {
            // Se existir, liga-a
            paineisTutorial[paginaAtual].SetActive(true);
        }
        else
        {
            // Se já não existirem mais páginas, fecha o tutorial e começa o jogo
            FecharTutorial();
        }
    }

    public void FecharTutorial()
    {
        // Retoma o tempo do jogo para o normal
        Time.timeScale = 1f; 
    }
}