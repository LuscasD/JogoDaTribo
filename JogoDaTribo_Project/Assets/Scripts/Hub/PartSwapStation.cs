using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Estação de troca de UM lado (esquerdo OU direito). NÃO guarda mais os braços
/// nem o que está equipado — ela só mostra a lista do jogador (PlayerArmLoadout)
/// e manda equipar. A escolha é lembrada pelo PlayerArmLoadout entre as cenas.
/// </summary>
public class PartSwapStation : MonoBehaviour
{
    public enum ArmSide { Esquerdo, Direito }

    [Header("Configuração")]
    [SerializeField] private ArmSide lado = ArmSide.Esquerdo;
    [SerializeField] private float interactRange = 3f;

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
    private PlayerArmLoadout loadout;

    private bool painelAberto;
    private int indexSelecionado;
    private readonly List<GameObject> botoesInstanciados = new();

    private void Start()
    {
        player = FindObjectOfType<PlayerMovment>();
        loadout = player != null ? player.GetComponent<PlayerArmLoadout>() : FindObjectOfType<PlayerArmLoadout>();

        painelSelecao.SetActive(false);
        interactTextObject.SetActive(false);
    }

    private void Update()
    {
        if (player == null || loadout == null) return;

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
                interactText.text = $"[F] Customizar Braço {lado}";

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

    private List<PlayerArmLoadout.Braco> Lista =>
        lado == ArmSide.Esquerdo ? loadout.esquerdo : loadout.direito;

    private int IndexEquipado =>
        lado == ArmSide.Esquerdo ? loadout.IndexEsquerdo : loadout.IndexDireito;

    private void AbrirPainel()
    {
        painelAberto = true;
        GunArm.BloqueiosTiro++;
        interactTextObject.SetActive(false);
        painelSelecao.SetActive(true);
        tituloPainel.text = $"Braço {lado}";

        indexSelecionado = IndexEquipado;
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
        var lista = Lista;
        if (lista.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            SetSelecionado((indexSelecionado + 1) % lista.Count);
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            SetSelecionado((indexSelecionado - 1 + lista.Count) % lista.Count);
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
        if (!Lista[indexSelecionado].desbloqueado) return;

        if (lado == ArmSide.Esquerdo) loadout.EquiparEsquerdo(indexSelecionado);
        else loadout.EquiparDireito(indexSelecionado);

        AtualizarCoresBotoes();
    }

    // -------------------------------------------------------

    private string Descricao(PlayerArmLoadout.Braco b)
    {
        if (b.objeto == null) return "";
        if (b.objeto.GetComponentInChildren<SawArm>(true) != null) return "Serra";
        if (b.objeto.GetComponentInChildren<GunArm>(true) != null) return "Arma";
        return "";
    }

    private void PopularBotoes()
    {
        LimparBotoes();

        var lista = Lista;
        for (int i = 0; i < lista.Count; i++)
        {
            var braco = lista[i];
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
        var lista = Lista;
        int equipado = IndexEquipado;

        for (int i = 0; i < botoesInstanciados.Count; i++)
        {
            var obj = botoesInstanciados[i];
            var img = obj.GetComponent<Image>();
            var braco = lista[i];

            if (img != null)
                img.color = i == indexSelecionado ? CorSelecionado
                          : i == equipado ? CorEquipado
                          : braco.desbloqueado ? CorDisponivel
                          : CorBloqueado;

            var tagEquipado = obj.transform.Find("TagEquipado");
            if (tagEquipado != null) tagEquipado.gameObject.SetActive(i == equipado);
        }
    }

    private void LimparBotoes()
    {
        foreach (var b in botoesInstanciados) Destroy(b);
        botoesInstanciados.Clear();
    }
}