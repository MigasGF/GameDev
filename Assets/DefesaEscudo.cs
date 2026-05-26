using UnityEngine;

public class DefesaEscudo : MonoBehaviour
{
    private Animator anim;

    [Header("Escudos")]
    public GameObject escudoMaoIdle;    
    public GameObject escudoMaoDefesa;  
    public GameObject escudoDasCostas;  

    void Start()
    {
        anim = GetComponent<Animator>(); 

        AtivarEscudo(escudoMaoIdle);
    }

    void Update()
    {
        bool estaAMoverSe = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;

        bool estaNaAgua = anim.GetBool("inWater");

        if (estaNaAgua)
        {
            anim.SetBool("A_Defender", false); // Garante que a animação de defesa desliga
            AtivarEscudo(escudoDasCostas);
            return; // O "return" faz o código parar aqui, ignorando o resto da lógica abaixo
        }

        if (Input.GetMouseButton(1)) 
        {
            anim.SetBool("A_Defender", true); 
            AtivarEscudo(escudoMaoDefesa);   
        }
        else 
        {
            anim.SetBool("A_Defender", false); 

            if (estaAMoverSe)
            {
                AtivarEscudo(escudoDasCostas);
            }
            else
            {
                AtivarEscudo(escudoMaoIdle);   
            }
        }
    }

   
    private void AtivarEscudo(GameObject escudoParaLigar)
    {
        escudoMaoIdle.SetActive(false);
        escudoMaoDefesa.SetActive(false);
        escudoDasCostas.SetActive(false);

        escudoParaLigar.SetActive(true);
    }
}