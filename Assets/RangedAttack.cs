using UnityEngine;

public class RangedAttack : MonoBehaviour
{
    [Header("Configurações do Tiro")]
    public float velocidade = 15f;
    public float tempoDeVida = 2f; // 2 segundos a voar = Alcance Médio. (Aumenta para ir mais longe)
    public float dano = 35f;

    void Start()
    {
        // O SEGREDO DO ALCANCE: Destrói o objeto ao fim do tempo definido
        Destroy(gameObject, tempoDeVida); 
    }

    void Update()
    {
        // Empurra a bola de fogo sempre para a frente
        transform.Translate(Vector3.forward * velocidade * Time.deltaTime);
    }

    void OnTriggerEnter(Collider outro)
    {
        // Se bater num esqueleto
        if (outro.CompareTag("Inimigo"))
        {
            Debug.Log("Bola de Fogo bateu num Inimigo: " + outro.name);

            // Tenta encontrar o script do esqueleto de 3 maneiras diferentes para não falhar
            InteligenciaEsqueleto esqueleto = outro.GetComponent<InteligenciaEsqueleto>();
            if (esqueleto == null) esqueleto = outro.GetComponentInParent<InteligenciaEsqueleto>();
            if (esqueleto == null) esqueleto = outro.GetComponentInChildren<InteligenciaEsqueleto>();

            if (esqueleto != null)
            {
                esqueleto.ReceberDano(dano);
                Debug.Log("Dano enviado ao esqueleto!");
            }
            else
            {
                Debug.LogError("Bateu no Inimigo, mas não encontrou o script InteligenciaEsqueleto!");
            }

            Destroy(gameObject); // A bola desaparece
        }
        // Se bater no Boss
        else if (outro.CompareTag("Boss"))
        {
            Debug.Log("Bola de Fogo bateu no Boss!");

            BossController boss = outro.GetComponent<BossController>();
            if (boss == null) boss = outro.GetComponentInParent<BossController>();
            if (boss == null) boss = outro.GetComponentInChildren<BossController>();

            if (boss != null)
            {
                boss.ReceberDano(dano);
                Debug.Log("Dano enviado ao Boss!");
            }
            else
            {
                Debug.LogError("Bateu no Boss, mas não encontrou o script BossController!");
            }

            Destroy(gameObject); // A bola desaparece
        }
    }
}