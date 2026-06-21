using UnityEngine;
using UnityEngine.SceneManagement; // Vital para cambiar de nivel

public class MainMenuManager : MonoBehaviour
{
    // Asegúrate de poner el nombre EXACTO de tu escena de juego
    public string nombreDelNivel = "SampleScene"; 

    public void IniciarJuego()
    {
        // Esto carga tu nivel principal
        SceneManager.LoadScene(nombreDelNivel); 
    }

    public void SalirJuego()
    {
        Application.Quit();
        Debug.Log("El juego se ha cerrado"); // Esto solo se ve en el editor
    }
}