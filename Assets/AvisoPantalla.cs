using UnityEngine;

public class AvisoPantalla : MonoBehaviour
{
    void Start()
    {
        // Destruye el objeto (el texto) exactamente 2 segundos después de aparecer
        Destroy(gameObject, 2f);
    }
}
