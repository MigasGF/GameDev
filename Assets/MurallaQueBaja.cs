using UnityEngine;

public class MurallaQueBaja : MonoBehaviour
{
    [Header("Configuración")]
    public float velocidad = 2f; // Qué tan rápido se hunde
    public float distanciaABajar = 5f; // Cuántos metros bajará

    private bool debeBajar = false;
    private Vector3 posicionDestino;

    void Start()
    {
        // Calculamos la posición final restándole la distancia al eje Y (abajo)
        posicionDestino = transform.position - new Vector3(0, distanciaABajar, 0);
    }

    void Update()
    {
        // Si activamos la bajada, movemos la pared suavemente cada frame
        if (debeBajar)
        {
            transform.position = Vector3.MoveTowards(transform.position, posicionDestino, velocidad * Time.deltaTime);
        }
    }

    // Esta es la función que el Boss llamará cuando muera
    public void ActivarBajada()
    {
        debeBajar = true;
    }
}