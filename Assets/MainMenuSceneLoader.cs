using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using FMODUnity;

public class MainMenuSceneLoader : MonoBehaviour
{
    [Header("FMOD Event Emitter da música")]
    public StudioEventEmitter menuMusicEmitter;

    [Header("Scene")]
    public string sceneToLoad = "Main Game";

    [Header("Tempo para esperar pelo fadeout")]
    public float delayBeforeLoad = 2f;

    private bool isLoading = false;

    public void PlayButton()
    {
        if (isLoading)
            return;

        StartCoroutine(StopMusicAndLoadScene());
    }

    private IEnumerator StopMusicAndLoadScene()
    {
        isLoading = true;

        if (menuMusicEmitter != null)
        {
            menuMusicEmitter.Stop();
        }

        yield return new WaitForSeconds(delayBeforeLoad);

        SceneManager.LoadScene(sceneToLoad);
    }
}