using UnityEngine;

public class CamaraSeguimiento : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 4, -6);
    public float smoothTime = 0.2f;

    private Vector3 _velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 posicionObjetivo = target.position + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            posicionObjetivo,
            ref _velocity,
            smoothTime
        );

        transform.LookAt(target.position);
    }
}
