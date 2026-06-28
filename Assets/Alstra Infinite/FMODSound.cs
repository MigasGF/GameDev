using UnityEngine;
using FMODUnity;

public class FMODSoundPlayer : MonoBehaviour
{
    [Header("Evento FMOD")]
    [SerializeField] private EventReference soundEvent;

    public void PlaySound()
    {
        if (!soundEvent.IsNull)
        {
            RuntimeManager.PlayOneShot(soundEvent, transform.position);
        }
    }
}