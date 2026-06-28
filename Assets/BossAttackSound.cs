using UnityEngine;
using FMODUnity;

public class AttackFMODState : StateMachineBehaviour
{
    [Header("FMOD")]
    public EventReference attackEvent;

    [Header("Delay (segundos)")]
    public float delay = 0.2f;

    private bool played;
    private float timer;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        played = false;
        timer = 0f;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (played)
            return;

        timer += Time.deltaTime;

        if (timer >= delay)
        {
            RuntimeManager.PlayOneShot(attackEvent, animator.transform.position);
            played = true;
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        played = false;
        timer = 0f;
    }
}