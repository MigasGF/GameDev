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