using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Movimento WASD
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        
        Vector3 direction = new Vector3(h, 0, v).normalized;

        if (direction.magnitude > 0.1f)
        {
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
            transform.forward = -direction;
        }

        // Atualizar Animação
        if (anim != null)
        {
            anim.SetFloat("Speed", direction.magnitude);
        }

        // Ataque
        if (Input.GetMouseButtonDown(0)) 
        {
            if (anim != null)
            {
                anim.SetTrigger("Attack");
            }
        }
    }
}