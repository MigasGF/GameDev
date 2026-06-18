using UnityEngine;
using System.Collections.Generic; // Precisamos disto para criar Listas

public class SwordDamage : MonoBehaviour
{
    public PlayerMovement scriptPlayer;

    public GameObject particles;
    
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
                inimigosAcertados.Add(outro); 
                GerarEfeito(outro);
            }
        }
        
        if (outro.CompareTag("Boss"))
        {
            BossController boss = outro.GetComponentInParent<BossController>();
            if (boss != null) 
            {
                boss.ReceberDano(scriptPlayer.danoDoAtaque);
                inimigosAcertados.Add(outro); // Regista que o Boss já sofreu o dano
                GerarEfeito(outro);
            }
        }
    }

    private void GerarEfeito(Collider inimigoHitbox)
    {
        if (particles != null)
        {
            // PRO TIP: ClosestPoint descobre a coordenada 3D exata onde a tua espada bateu no monstro!
            Vector3 pontoDeImpacto = inimigoHitbox.ClosestPoint(transform.position);
            pontoDeImpacto.y += 0.5f; 
            // Cria o clone do prefab naquele exato ponto
            Instantiate(particles, pontoDeImpacto, Quaternion.identity);
        }
    }
}