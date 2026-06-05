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
        // Se bater num inimigo, dá dano e desaparece imediatamente
        if (outro.CompareTag("Inimigo"))
        {
            InteligenciaEsqueleto esqueleto = outro.GetComponentInParent<InteligenciaEsqueleto>();
            if (esqueleto != null) esqueleto.ReceberDano(dano);
            Destroy(gameObject); // A bola "explode" ao bater
        }
        else if (outro.CompareTag("Boss"))
        {
            BossController boss = outro.GetComponentInParent<BossController>();
            if (boss != null) boss.ReceberDano(dano);
            Destroy(gameObject); // A bola "explode" ao bater
        }
    }
}