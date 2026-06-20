using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Estação de troca de UM lado (esquerdo OU direito). Mantém a lista de braços
/// possíveis daquele lado (GameObjects que já existem na cena, ex.: Arm.L e
/// ArmGun.L) e equipar = ativar o escolhido e desativar os outros.
/// </summary>
public class PartSwapStation : MonoBehaviour
{
    [System.Serializable]
    public class ArmEntry
    {
        public string nome = "Braço";
        [Tooltip("GameObject do braço a ativar (ex.: Arm.L ou ArmGun.L).")]
        public GameObject objetoBraco;
        public bool desbloqueado = true;
    }

    [Header("Configuração")]
    [Tooltip("Texto só de exibição (ex.: Esquerdo / Direito).")]
    [SerializeField] private string nomeLado = "Esquerdo";
    [SerializeField] private float interactRange = 3f;

    [Header("Braços deste lado")]
    [SerializeField] private List<ArmEntry> bracos = new();

    [Header("UI - Prompt de Interação")]
    [SerializeField] private GameObject interactTextObject;
    [SerializeField] private TextMeshProUGUI interactText;

    [Header("UI - Painel de Seleção")]
    [SerializeField] private GameObject painelSelecao;
    [SerializeField] private TextMeshProUGUI tituloPainel;
    [SerializeField] private Transform containerBotoes;
    [SerializeField] private GameObject botaoTemplate;

    private static readonly Color CorEquipado = new Color(0.20f, 0.75f, 0.20f, 1f);
    private static readonly Color CorSelecionado = new Color(0.95f, 0.75f, 0.10f, 1f);
    private static readonly Color CorDisponivel = new Color(0.90f, 0.90f, 0.90f, 1f);
    private static readonly Color CorBloqueado = new Color(0.35f, 0.35f, 0.35f, 1f);

    private PlayerMovment player;

    private bool painelAberto;
    private int indexEquipado;
    private int indexSelecionado;
    private readonly List<GameObject> botoesInstanciados = new();

    private void Start()
    {
        player = FindObjectOfType<PlayerMovment>();

        if (bracos.Count > 0) bracos[0].desbloqueado = true;

        painelSelecao.SetActive(false);
        interactTextObject.SetActive(false);

        Equipar(0); // ativa o primeiro braço, desativa o resto
    }

    private void Update()
    {
        if (player == null) return;

        bool noRange = Vector3.Distance(transform.position, player.transform.position) <= interactRange;

        if (painelAberto)
        {
            if (!noRange) { FecharPainel(); return; }

            HandleNavegacao();

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.F))
                FecharPainel();
        }
        else
        {
            interactTextObject.SetActive(noRange);
            if (interactText != null)
                interactText.text = $"[F] Customizar Braço {nomeLado}";

            if (noRange && Input.GetKeyDown(KeyCode.F))
                AbrirPainel();
        }
    }

    private void OnDisable()
    {
        if (painelAberto)
        {
            painelAberto = false;
            GunArm.BloqueiosTiro = Mathf.Max(0, GunArm.BloqueiosTiro - 1);
        }
    }

    // -------------------------------------------------------

    private void AbrirPainel()
    {
        painelAberto = true;
        GunArm.BloqueiosTiro++;          // trava o tiro enquanto customiza
        interactTextObject.SetActive(false);
        painelSelecao.SetActive(true);
        tituloPainel.text = $"Braço {nomeLado}";

        indexSelecionado = indexEquipado;
        PopularBotoes();
    }

    private void FecharPainel()
    {
        painelAberto = false;
        GunArm.BloqueiosTiro = Mathf.Max(0, GunArm.BloqueiosTiro - 1);
        painelSelecao.SetActive(false);
        LimparBotoes();
    }

    private void HandleNavegacao()
    {
        if (bracos.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            SetSelecionado((indexSelecionado + 1) % bracos.Count);
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            SetSelecionado((indexSelecionado - 1 + bracos.Count) % bracos.Count);
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            EquiparSelecionado();
    }

    private void SetSelecionado(int index)
    {
        indexSelecionado = index;
        AtualizarCoresBotoes();
    }

    private void EquiparSelecionado()
    {
        if (!bracos[indexSelecionado].desbloqueado) return;
        Equipar(indexSelecionado);
        AtualizarCoresBotoes();
    }

    private void Equipar(int index)
    {
        if (index < 0 || index >= bracos.Count) return;
        if (!bracos[index].desbloqueado) return;

        indexEquipado = index;
        for (int i = 0; i < bracos.Count; i++)
            if (bracos[i].objetoBraco != null)
                bracos[i].objetoBraco.SetActive(i == index);
    }

    // -------------------------------------------------------

    private string Descricao(ArmEntry e)
    {
        if (e.objetoBraco == null) return "";
        if (e.objetoBraco.GetComponentInChildren<SawArm>(true) != null) return "Serra";
        if (e.objetoBraco.GetComponentInChildren<GunArm>(true) != null) return "Arma";
        return "";
    }

    private void PopularBotoes()
    {
        LimparBotoes();

        for (int i = 0; i < bracos.Count; i++)
        {
            var braco = bracos[i];
            int idx = i;

            GameObject obj = Instantiate(botaoTemplate, containerBotoes);
            obj.SetActive(true);

            var textos = obj.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (textos.Length > 0)
                textos[0].text = braco.desbloqueado ? braco.nome : "???";
            if (textos.Length > 1)
                textos[1].text = braco.desbloqueado ? Descricao(braco) : "<color=#888>Bloqueado</color>";

            var tagEquipado = obj.transform.Find("TagEquipado");
            if (tagEquipado != null) tagEquipado.gameObject.SetActive(false);

            var button = obj.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = braco.desbloqueado;
                button.onClick.AddListener(() => { indexSelecionado = idx; EquiparSelecionado(); });
            }

            botoesInstanciados.Add(obj);
        }

        AtualizarCoresBotoes();
    }

    private void AtualizarCoresBotoes()
    {
        for (int i = 0; i < botoesInstanciados.Count; i++)
        {
            var obj = botoesInstanciados[i];
            var img = obj.GetComponent<Image>();
            var braco = bracos[i];

            if (img != null)
                img.color = i == indexSelecionado ? CorSelecionado
                          : i == indexEquipado ? CorEquipado
                          : braco.desbloqueado ? CorDisponivel
                          : CorBloqueado;

            var tagEquipado = obj.transform.Find("TagEquipado");
            if (tagEquipado != null) tagEquipado.gameObject.SetActive(i == indexEquipado);
        }
    }

    private void LimparBotoes()
    {
        foreach (var b in botoesInstanciados) Destroy(b);
        botoesInstanciados.Clear();
    }
}