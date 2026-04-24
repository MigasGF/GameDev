using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float rotationVelocity;

    public float speed = 5f;
    public float rotationSpeed = 720f;
    
    private Animator anim;
    private CharacterController controller;

    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        // Evita conflito entre deslocacao por animacao e por script
        anim.applyRootMotion = false;
    }

void Update()
{
    if (Input.GetMouseButtonDown(0)) // clique esquerdo
{
    anim.SetTrigger("bash");
}

    float h = Input.GetAxisRaw("Horizontal");
    float v = Input.GetAxisRaw("Vertical");

    Vector3 direction = new Vector3(h, 0f, v).normalized;

    if (direction.magnitude >= 0.1f)
    {
        anim.SetBool("isRunning", true);

        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        float angle = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            targetAngle,
            ref rotationVelocity,
            0.05f // mais rápido = menos efeito S
        );

        transform.rotation = Quaternion.Euler(0f, angle, 0f);

        // 🔥 AQUI ESTÁ A CORREÇÃO PRINCIPAL
        controller.Move(direction * speed * Time.deltaTime);
    }
    else
    {
        anim.SetBool("isRunning", false);
    }
}

}