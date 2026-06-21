using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject painelLoading;
    public GameObject botaoPlay;
    public Slider barraDeProgresso;

    [Range(0.1f, 2f)] public float velocidadeDaBarra = 0.8f; 

    public void Jogar()
    {
        StartCoroutine(CarregarCenaAssincrona("MainGame"));
    }

    IEnumerator CarregarCenaAssincrona(string nomeDaCena)
    {
        botaoPlay.SetActive(false);
        painelLoading.SetActive(true);

        yield return new WaitForSeconds(0.1f); 

        AsyncOperation operacao = SceneManager.LoadSceneAsync(nomeDaCena);
        
        operacao.allowSceneActivation = false;

        float progressoAlvo = 0f;

        while (!operacao.isDone)
        {
            if (operacao.progress >= 0.9f)
            {
                progressoAlvo = 1f; 
            }
            else
            {
                progressoAlvo = operacao.progress / 0.9f;
            }

            barraDeProgresso.value = Mathf.MoveTowards(barraDeProgresso.value, progressoAlvo, Time.deltaTime * velocidadeDaBarra);

            if (Mathf.Approximately(barraDeProgresso.value, 1f))
            {
                yield return new WaitForSeconds(0.2f);
                operacao.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}