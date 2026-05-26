using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 720f;
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    private float rotationVelocity;
    private float verticalVelocity;

    private Animator anim;
    private CharacterController controller;

    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        anim.applyRootMotion = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            anim.SetTrigger("bash");
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = new Vector3(h, 0f, v).normalized;

        // Gravedad acumulada; reset al tocar el suelo
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f; // pequeño valor negativo para mantener contacto

            if (Input.GetButtonDown("Jump"))
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        if (moveDir.magnitude >= 0.1f)
        {
            anim.SetBool("isRunning", true);

            float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref rotationVelocity,
                0.05f
            );
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
        else
        {
            anim.SetBool("isRunning", false);
        }

        Vector3 motion = moveDir * speed;
        motion.y = verticalVelocity;
        controller.Move(motion * Time.deltaTime);
    }
}