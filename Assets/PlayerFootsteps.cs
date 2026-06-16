using UnityEngine;
using FMODUnity;
using FMOD.Studio;
public class PlayerFootsteps : MonoBehaviour
{
    [Header("FMOD Event")]
    public EventReference footstepEvent;
    [Header("Settings")]
    public float stepInterval = 0.47f;
    [Header("Surface Parameter")]
    public string surfaceParameterName = "Surface";
    private CharacterController controller;
    private PlayerMovement playerMovement;
    private float stepTimer;
    private float currentSurface = 0f; // 0 = Sand, 1 = Stone
    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
    }
    void Update()
    {
        bool isMoving = controller.velocity.magnitude > 0.1f;
        bool isGrounded = controller.isGrounded;
        if (isMoving && isGrounded)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }
    void PlayFootstep()
    {
        EventInstance footstep = RuntimeManager.CreateInstance(footstepEvent);
        FMOD.RESULT result = footstep.setParameterByName(surfaceParameterName, currentSurface);
        Debug.Log("Surface: " + currentSurface + " | Result: " + result);
        RuntimeManager.AttachInstanceToGameObject(footstep, gameObject);
        footstep.start();
        footstep.release();

        // Play armor rattle (Medium intensity) with each footstep
        if (playerMovement != null)
        {
            playerMovement.PlayArmorRattle(playerMovement.runIntensity);
        }
    }
    // When player enters a stone zone
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Stone"))
        {
            currentSurface = 1f; // Stone
        }
    }
    // When player leaves a stone zone, go back to sand
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Stone"))
        {
            currentSurface = 0f; // Sand
        }
    }
    
}
