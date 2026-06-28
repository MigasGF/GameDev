using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

[System.Serializable]
public class SlotDeInventario
{
    public string nomeItem = "";
    public Sprite icone = null;
    public int quantidade = 0;
    public float vidaRestaurada = 0f;
}

public class Inventory : MonoBehaviour
{
    [Header("Os 5 Slots da Hotbar")]
    public SlotDeInventario[] slots = new SlotDeInventario[5];
    public int slotSelecionado = 0;

    [Header("Referências da UI")]
    public Image[] iconesUI;
    public Text[] textosQuantidade;
    public GameObject[] bordasSelecao;

    private PlayerMovement playerScript;

    [Header("Efeitos")]
    public GameObject auraDanoVermelha;
    public GameObject auraTempoAzul;
    public GameObject auraInvencivelRoxa;
    public GameObject particulasCura;

    [Header("FMOD - Sons ao adicionar ao inventário")]
    public EventReference somIceCream;
    public EventReference somCake;
    public EventReference somBurger;
    public EventReference somCrystal;

    void Start()
    {
        playerScript = GetComponent<PlayerMovement>();
        AtualizarUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { slotSelecionado = 0; AtualizarUI(); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { slotSelecionado = 1; AtualizarUI(); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { slotSelecionado = 2; AtualizarUI(); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { slotSelecionado = 3; AtualizarUI(); }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { slotSelecionado = 4; AtualizarUI(); }

        if (Input.GetKeyDown(KeyCode.E))
        {
            ConsumirItemSelecionado();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Food comidaNoChao = other.GetComponent<Food>();

        if (comidaNoChao != null)
        {
            if (AdicionarAoInventario(comidaNoChao))
            {
                Destroy(other.gameObject);
            }
        }
    }

    bool AdicionarAoInventario(Food item)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].nomeItem == item.nomeDaComida)
            {
                slots[i].quantidade += item.quantidade;
                AtualizarUI();
                TocarSomItemAdicionado(item.nomeDaComida);
                return true;
            }
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].quantidade == 0)
            {
                slots[i].nomeItem = item.nomeDaComida;
                slots[i].icone = item.iconeUI;
                slots[i].quantidade = item.quantidade;
                slots[i].vidaRestaurada = item.vidaRestaurada;

                AtualizarUI();
                TocarSomItemAdicionado(item.nomeDaComida);

                return true;
            }
        }

        Debug.Log("Inventário Cheio!");
        return false;
    }

    void TocarSomItemAdicionado(string nomeItem)
    {
        if (nomeItem == "Ice Cream")
        {
            RuntimeManager.PlayOneShot(somIceCream, transform.position);
        }
        else if (nomeItem == "Cake")
        {
            RuntimeManager.PlayOneShot(somCake, transform.position);
        }
        else if (nomeItem == "Burger")
        {
            RuntimeManager.PlayOneShot(somBurger, transform.position);
        }
        else if (
            nomeItem == "Cristal Vermelho" ||
            nomeItem == "Cristal Azul" ||
            nomeItem == "Cristal Roxo" ||
            nomeItem == "Crystal"
        )
        {
            RuntimeManager.PlayOneShot(somCrystal, transform.position);
        }
    }

    void ConsumirItemSelecionado()
    {
        SlotDeInventario slot = slots[slotSelecionado];

        if (slot.quantidade > 0)
        {
            if (slot.nomeItem == "Cristal Vermelho")
            {
                StartCoroutine(EfeitoCristalVermelho());
                GastarItem(slot);
            }
            else if (slot.nomeItem == "Cristal Azul")
            {
                StartCoroutine(EfeitoCristalAzul());
                GastarItem(slot);
            }
            else if (slot.nomeItem == "Cristal Roxo")
            {
                StartCoroutine(EfeitoCristalRoxo());
                GastarItem(slot);
            }
            else if (playerScript.vidaAtual < playerScript.vidaMaxima)
            {
                playerScript.vidaAtual += slot.vidaRestaurada;

                if (playerScript.vidaAtual > playerScript.vidaMaxima)
                    playerScript.vidaAtual = playerScript.vidaMaxima;

                if (playerScript.barraVidaPlayer != null)
                    playerScript.barraVidaPlayer.value = playerScript.vidaAtual;

                if (particulasCura != null)
                {
                    GameObject aura = Instantiate(particulasCura, transform.position, Quaternion.identity);
                    aura.transform.SetParent(this.transform);
                }

                GastarItem(slot);
            }
        }
    }

    System.Collections.IEnumerator EfeitoCristalVermelho()
    {
        Debug.Log("Poder Vermelho: +Dano!");

        if (auraDanoVermelha != null)
            auraDanoVermelha.SetActive(true);

        playerScript.danoDoAtaque = 50f;

        yield return new WaitForSecondsRealtime(30f);

        playerScript.danoDoAtaque = 35f;

        if (auraDanoVermelha != null)
            auraDanoVermelha.SetActive(false);
    }

    System.Collections.IEnumerator EfeitoCristalAzul()
    {
        Debug.Log("Poder Azul: Cortes pelo Ar Ativados!");

        if (auraTempoAzul != null)
            auraTempoAzul.SetActive(true);

        playerScript.poderCorteAr = true;

        yield return new WaitForSeconds(30f);

        playerScript.poderCorteAr = false;

        if (auraTempoAzul != null)
            auraTempoAzul.SetActive(false);

        Debug.Log("Poder Azul: A espada voltou ao normal.");
    }

    System.Collections.IEnumerator EfeitoCristalRoxo()
    {
        Debug.Log("Poder Roxo: Invencibilidade Ativada!");

        if (auraInvencivelRoxa != null)
            auraInvencivelRoxa.SetActive(true);

        playerScript.estaInvencivel = true;

        yield return new WaitForSecondsRealtime(15f);

        playerScript.estaInvencivel = false;

        if (auraInvencivelRoxa != null)
            auraInvencivelRoxa.SetActive(false);

        Debug.Log("Poder Roxo: A Invencibilidade acabou.");
    }

    void GastarItem(SlotDeInventario slot)
    {
        slot.quantidade--;

        if (slot.quantidade <= 0)
        {
            slot.nomeItem = "";
            slot.icone = null;
            slot.quantidade = 0;
            slot.vidaRestaurada = 0f;
        }

        AtualizarUI();
    }

    void AtualizarUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (bordasSelecao.Length > i && bordasSelecao[i] != null)
                bordasSelecao[i].SetActive(i == slotSelecionado);

            if (iconesUI.Length > i && iconesUI[i] != null)
            {
                if (slots[i].quantidade > 0)
                {
                    iconesUI[i].sprite = slots[i].icone;
                    iconesUI[i].color = Color.white;
                }
                else
                {
                    iconesUI[i].sprite = null;
                    iconesUI[i].color = new Color(1, 1, 1, 0);
                }
            }

            if (textosQuantidade.Length > i && textosQuantidade[i] != null)
            {
                textosQuantidade[i].text = slots[i].quantidade > 0 ? slots[i].quantidade.ToString() : "";
            }
        }
    }
}