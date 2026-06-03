using UnityEngine;
using UnityEngine.SceneManagement; // ESTA LÍNEA ES VITAL para cambiar de niveles

public class MenuPrincipal : MonoBehaviour
{
    // Esta es la función que activará el botón
    public void EmpezarJuego()
    {
        // El nombre entre comillas DEBE ser exactamente igual al de tu escena
        SceneManager.LoadScene("SampleScene"); 
    }
}