using UnityEngine;
using System.Collections.Generic; // Precisamos disto para criar Listas

public class SwordDamage : MonoBehaviour
{
    public PlayerMovement scriptPlayer;
    
    // Lista que guarda quem já levou dano nesta espadada
    private List<Collider> inimigosAcertados = new List<Collider>();

    // Função para limpar a memória da espada a cada novo ataque
    public void PrepararNovoAtaque()
    {
        inimigosAcertados.Clear();
    }

    // Mudámos de Enter para Stay! Assim acerta sempre, mesmo estando colado.
    private void OnTriggerStay(Collider outro)
    {
        if (!scriptPlayer.estaAAtacar) return;

        // Se este inimigo já está na lista de acertados deste golpe, ignoramos!
        if (inimigosAcertados.Contains(outro)) return;

        if (outro.CompareTag("Inimigo"))
        {
            InteligenciaEsqueleto esqueleto = outro.GetComponent<InteligenciaEsqueleto>();
            if (esqueleto != null) 
            {
                esqueleto.ReceberDano(scriptPlayer.danoDoAtaque);
                inimigosAcertados.Add(outro); // Regista que este esqueleto já levou tau-tau
            }
        }
        
        if (outro.CompareTag("Boss"))
        {
            BossController boss = outro.GetComponentInParent<BossController>();
            if (boss != null) 
            {
                boss.ReceberDano(scriptPlayer.danoDoAtaque);
                inimigosAcertados.Add(outro); // Regista que o Boss já sofreu o dano
            }
        }
    }
}