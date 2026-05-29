using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    public PlayerMovement scriptPlayer;

    private void OnTriggerEnter(Collider outro)
    {
        // Se o player não estiver na animação de ataque, a espada não faz dano
        if (!scriptPlayer.estaAAtacar) return;

        // Se bater num inimigo comum
        if (outro.CompareTag("Inimigo"))
        {
            InteligenciaEsqueleto esqueleto = outro.GetComponent<InteligenciaEsqueleto>();
            if (esqueleto != null) esqueleto.ReceberDano(scriptPlayer.danoDoAtaque);
        }
        
        // Se bater no Boss
        if (outro.CompareTag("Boss"))
        {
            BossController boss = outro.GetComponentInParent<BossController>();
            if (boss != null) boss.ReceberDano(scriptPlayer.danoDoAtaque);
        }
    }
}