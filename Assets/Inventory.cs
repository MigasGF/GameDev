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

        // Se houver comida neste slot e o jogador não tiver a vida no máximo
        if (slot.quantidade > 0 && playerScript.vidaAtual < 100f)
        {
            // Aumenta a vida (garantindo que não passa dos 100)
            playerScript.vidaAtual += slot.vidaRestaurada;
            if (playerScript.vidaAtual > 100f) playerScript.vidaAtual = 100f;

            // Atualiza a barra de vida do PlayerMovement
            if (playerScript.barraVidaPlayer != null) 
            {
                playerScript.barraVidaPlayer.value = playerScript.vidaAtual;
            }

            // Gasta 1 item
            slot.quantidade--;

            // Se a quantidade chegar a zero, limpa o slot
            if (slot.quantidade <= 0)
            {
                slot.nomeItem = "";
                slot.icone = null;
                slot.quantidade = 0;
                slot.vidaRestaurada = 0f;
            }

            AtualizarUI();
            Debug.Log("Comida consumida! Vida curada.");
        }
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