using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODZMusicController : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("FMOD Event")]
    public EventReference soundtrackEvent;

    [Header("Z Mapping")]
    public float minZ = 0f;
    public float maxZ = 100f;

    [Header("FMOD Parameter Range")]
    public float minParameter = 0f;
    public float maxParameter = 1f;

    private EventInstance soundtrackInstance;

    void Start()
    {
        soundtrackInstance = RuntimeManager.CreateInstance(soundtrackEvent);
        soundtrackInstance.start();
    }

    void Update()
    {
        if (player == null)
            return;

        float normalizedZ = Mathf.InverseLerp(minZ, maxZ, player.position.z);

        float parameterValue = Mathf.Lerp(minParameter, maxParameter, normalizedZ);

        soundtrackInstance.setParameterByName("SoundTrack (continuous)", parameterValue);
    }

    void OnDestroy()
    {
        soundtrackInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        soundtrackInstance.release();
    }
}
