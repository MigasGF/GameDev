using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryManager : MonoBehaviour
{
    [Header("UI Elementos")]
    public GameObject panelVictoria;

    // Esta función se llamará cuando el jefe muera
    public void MostrarVictoria()
    {
        panelVictoria.SetActive(true); // Enciende la interfaz
        Time.timeScale = 0f;           // Congela el tiempo en el juego
    }

    // Esta función se conectará al botón de tu panel
    public void VolverAlMenu()
    {
        Time.timeScale = 1f; // ¡VITAL! Restaurar el tiempo antes de cambiar de escena
        
        // Escribe aquí el nombre exacto de tu escena del menú principal
        SceneManager.LoadScene("MainMenu"); 
    }
}