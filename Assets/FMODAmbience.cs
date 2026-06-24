using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODAmbienceBlendController : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("FMOD Event")]
    public EventReference ambienceEvent;

    [Header("Z Mapping")]
    public float minZ = 0f;
    public float maxZ = 100f;

    [Header("FMOD Parameter Range")]
    public float minParameter = 0f;
    public float maxParameter = 1f;

    private EventInstance ambienceInstance;

    void Start()
    {
        ambienceInstance = RuntimeManager.CreateInstance(ambienceEvent);
        ambienceInstance.start();
    }

    void Update()
    {
        if (player == null)
            return;

        float normalizedZ = Mathf.InverseLerp(minZ, maxZ, player.position.z);

        float parameterValue = Mathf.Lerp(
            minParameter,
            maxParameter,
            normalizedZ
        );

        ambienceInstance.setParameterByName("AmbienceBlend", parameterValue);
    }

    void OnDestroy()
    {
        ambienceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        ambienceInstance.release();
    }
}