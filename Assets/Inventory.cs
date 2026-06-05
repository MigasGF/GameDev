using UnityEngine;
using UnityEngine.UI;

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

    [Header("Referências da UI (Arrastar do Canvas)")]
    public Image[] iconesUI;       // As imagens de cada slot
    public Text[] textosQuantidade; // Os textos dos números (podes usar TextMeshPro se preferires)
    public GameObject[] bordasSelecao; // Um contorno para mostrar qual está selecionado

    private PlayerMovement playerScript;


    [Header("Efeitos")]
    public GameObject auraDanoVermelha; // Cristal Vermelho
    public GameObject auraTempoAzul;    // Cristal Azul
    public GameObject auraInvencivelRoxa;   // Cristal Roxo
    public GameObject particulasCura;   // Comida 


    void Start()
    {
        playerScript = GetComponent<PlayerMovement>();
        AtualizarUI();
    }

    void Update()
    {
        // Escolher o slot com os números do teclado (1 a 5)
        if (Input.GetKeyDown(KeyCode.Alpha1)) { slotSelecionado = 0; AtualizarUI(); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { slotSelecionado = 1; AtualizarUI(); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { slotSelecionado = 2; AtualizarUI(); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { slotSelecionado = 3; AtualizarUI(); }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { slotSelecionado = 4; AtualizarUI(); }

        // Consumir item com a tecla E (ou podes deixar a UpArrow se preferires)
        if (Input.GetKeyDown(KeyCode.E))
        {
            ConsumirItemSelecionado();
        }
    }


    // Apanhar os itens do chão
    void OnTriggerEnter(Collider other)
    {
        Food comidaNoChao = other.GetComponent<Food>();

        if (comidaNoChao != null)
        {
            if (AdicionarAoInventario(comidaNoChao))
            {
                Destroy(other.gameObject); // Destrói o item do chão se conseguiu apanhar
            }
        }
    }

    bool AdicionarAoInventario(Food item)
    {
        // 1. Tenta encontrar um slot que já tenha esta comida para acumular
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].nomeItem == item.nomeDaComida)
            {
                slots[i].quantidade += item.quantidade;
                AtualizarUI();
                return true;
            }
        }

        // 2. Se não encontrou, procura o primeiro slot vazio
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].quantidade == 0) // Slot vazio
            {
                slots[i].nomeItem = item.nomeDaComida;
                slots[i].icone = item.iconeUI;
                slots[i].quantidade = item.quantidade;
                slots[i].vidaRestaurada = item.vidaRestaurada;
                AtualizarUI();
                return true;
            }
        }

        Debug.Log("Inventário Cheio!");
        return false; // Não conseguiu apanhar
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
            // --- COMIDA NORMAL ---
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

    // ==========================================
    // PODERES DOS CRISTAIS
    // ==========================================

    System.Collections.IEnumerator EfeitoCristalVermelho()
    {
        Debug.Log("Poder Vermelho: +Dano!");
        if (auraDanoVermelha != null) auraDanoVermelha.SetActive(true);

        playerScript.danoDoAtaque = 50f; // Boost

        // Espera 30 segundos usando tempo real
        yield return new WaitForSecondsRealtime(30f);

        playerScript.danoDoAtaque = 35f; // Volta ao normal
        if (auraDanoVermelha != null) auraDanoVermelha.SetActive(false);
    }

    System.Collections.IEnumerator EfeitoCristalAzul()
    {
        Debug.Log("Poder Azul: Cortes pelo Ar Ativados!");
        if (auraTempoAzul != null) auraTempoAzul.SetActive(true);

        playerScript.poderCorteAr = true; // ATIVA O PODER

        yield return new WaitForSeconds(30f); // DURA 30 SEGUNDOS

        playerScript.poderCorteAr = false; // DESATIVA O PODER
        if (auraTempoAzul != null) auraTempoAzul.SetActive(false);
        Debug.Log("Poder Azul: A espada voltou ao normal.");
    }

    System.Collections.IEnumerator EfeitoCristalRoxo()
    {
        Debug.Log("Poder Roxo: Invencibilidade Ativada!");
        if (auraInvencivelRoxa != null) auraInvencivelRoxa.SetActive(true);

        playerScript.estaInvencivel = true; // LIGA O GOD MODE

        // Coloquei 15 segundos, mas podes mudar este valor para o que achares justo!
        yield return new WaitForSecondsRealtime(15f);

        playerScript.estaInvencivel = false; // DESLIGA O GOD MODE
        if (auraInvencivelRoxa != null) auraInvencivelRoxa.SetActive(false);
        Debug.Log("Poder Roxo: A Invencibilidade acabou.");
    }

    // Função auxiliar para não repetirmos código
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
            // Liga/Desliga o contorno de seleção
            if (bordasSelecao.Length > i && bordasSelecao[i] != null)
                bordasSelecao[i].SetActive(i == slotSelecionado);

            // Atualiza Ícones
            if (iconesUI.Length > i && iconesUI[i] != null)
            {
                if (slots[i].quantidade > 0)
                {
                    iconesUI[i].sprite = slots[i].icone;
                    iconesUI[i].color = Color.white; // Mostra a cor normal
                }
                else
                {
                    iconesUI[i].sprite = null;
                    iconesUI[i].color = new Color(1, 1, 1, 0); // Fica transparente se vazio
                }
            }

            // Atualiza Textos de Quantidade
            if (textosQuantidade.Length > i && textosQuantidade[i] != null)
            {
                textosQuantidade[i].text = slots[i].quantidade > 0 ? slots[i].quantidade.ToString() : "";
            }
        }
    }
}