using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private static readonly Color CorEquipado   = new Color(0.20f, 0.75f, 0.20f, 1f);
    private static readonly Color CorSelecionado = new Color(0.95f, 0.75f, 0.10f, 1f);
    private static readonly Color CorDisponivel  = new Color(0.90f, 0.90f, 0.90f, 1f);
    private static readonly Color CorBloqueado   = new Color(0.35f, 0.35f, 0.35f, 1f);

    private PlayerMovment player;
    private PlayerPartsChange playerParts;

    private bool painelAberto;
    private int indexSelecionado;
    private readonly List<GameObject> botoesInstanciados = new();

    private void Start()
    {
        player      = FindObjectOfType<PlayerMovment>();
        playerParts = player?.GetComponent<PlayerPartsChange>();

        painelSelecao.SetActive(false);
        interactTextObject.SetActive(false);
    }

    private void Update()
    {
        if (player == null) return;

        bool noRange = Vector3.Distance(transform.position, player.transform.position) <= interactRange;

        if (painelAberto)
        {
            if (!noRange)
            {
                FecharPainel();
                return;
            }

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

    // -------------------------------------------------------

    private void AbrirPainel()
    {
        painelAberto = true;
        interactTextObject.SetActive(false);
        painelSelecao.SetActive(true);
        tituloPainel.text = $"Braço {lado}";

        indexSelecionado = lado == ArmSide.Esquerdo
            ? playerParts.CurrentLeftArm
            : playerParts.CurrentRightArm;

        PopularBotoes();
    }

    private void FecharPainel()
    {
        painelAberto = false;
        painelSelecao.SetActive(false);
        LimparBotoes();
    }

    private void HandleNavegacao()
    {
        var partes = lado == ArmSide.Esquerdo ? playerParts.leftArmParts : playerParts.rightArmParts;

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            int next = (indexSelecionado + 1) % partes.Count;
            SetSelecionado(next);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            int prev = (indexSelecionado - 1 + partes.Count) % partes.Count;
            SetSelecionado(prev);
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            EquiparSelecionado();
        }
    }

    private void SetSelecionado(int index)
    {
        indexSelecionado = index;
        AtualizarCoresBotoes();
    }

    private void EquiparSelecionado()
    {
        var partes = lado == ArmSide.Esquerdo ? playerParts.leftArmParts : playerParts.rightArmParts;
        if (!partes[indexSelecionado].isUnlocked) return;

        if (lado == ArmSide.Esquerdo) playerParts.EquipLeftArm(indexSelecionado);
        else                          playerParts.EquipRightArm(indexSelecionado);

        AtualizarCoresBotoes();
    }

    // -------------------------------------------------------

    private void PopularBotoes()
    {
        LimparBotoes();

        var partes = lado == ArmSide.Esquerdo ? playerParts.leftArmParts : playerParts.rightArmParts;

        for (int i = 0; i < partes.Count; i++)
        {
            var parte = partes[i];
            int idx   = i;

            GameObject obj = Instantiate(botaoTemplate, containerBotoes);
            obj.SetActive(true);

            var textos = obj.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (textos.Length > 0)
                textos[0].text = parte.isUnlocked ? parte.partName : "???";
            if (textos.Length > 1)
                textos[1].text = parte.isUnlocked ? $"+{parte.damageBonus} Dano" : "<color=#888>Bloqueado</color>";

            var tagEquipado = obj.transform.Find("TagEquipado");
            if (tagEquipado != null)
                tagEquipado.gameObject.SetActive(false);

            var button = obj.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = parte.isUnlocked;
                button.onClick.AddListener(() => {
                    indexSelecionado = idx;
                    EquiparSelecionado();
                });
            }

            botoesInstanciados.Add(obj);
        }

        AtualizarCoresBotoes();
    }

    private void AtualizarCoresBotoes()
    {
        var partes   = lado == ArmSide.Esquerdo ? playerParts.leftArmParts  : playerParts.rightArmParts;
        int equipado = lado == ArmSide.Esquerdo ? playerParts.CurrentLeftArm : playerParts.CurrentRightArm;

        for (int i = 0; i < botoesInstanciados.Count; i++)
        {
            var obj  = botoesInstanciados[i];
            var img  = obj.GetComponent<Image>();
            var part = partes[i];

            if (img != null)
                img.color = i == indexSelecionado ? CorSelecionado
                          : i == equipado         ? CorEquipado
                          : part.isUnlocked       ? CorDisponivel
                          :                         CorBloqueado;

            var tagEquipado = obj.transform.Find("TagEquipado");
            if (tagEquipado != null)
                tagEquipado.gameObject.SetActive(i == equipado);
        }
    }

    private void LimparBotoes()
    {
        foreach (var b in botoesInstanciados) Destroy(b);
        botoesInstanciados.Clear();
    }
}
