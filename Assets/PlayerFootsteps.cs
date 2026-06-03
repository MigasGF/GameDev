using UnityEngine;
using FMODUnity;
using FMOD.Studio;
 
public class PlayerFootsteps : MonoBehaviour
{
    [Header("FMOD Event")]
    public EventReference footstepEvent;
 
    [Header("Settings")]
    public float stepInterval = 0.4f;
 
    private CharacterController controller;
    private float stepTimer;
 
    void Start()
    {
        controller = GetComponent<CharacterController>();
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
	    RuntimeManager.AttachInstanceToGameObject(footstep, gameObject);
	    footstep.start();
	    footstep.release();
	}
}
