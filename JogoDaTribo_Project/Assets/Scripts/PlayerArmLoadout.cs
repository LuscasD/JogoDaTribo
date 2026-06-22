using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fica no jogador (Robot 1). É o DONO dos braços: liga/desliga os GameObjects
/// de cada lado e LEMBRA qual está equipado entre as cenas (campos static).
/// Assim o loadout escolhido no hub continua valendo quando entra na fase.
///
/// Coloque este componente no prefab do jogador, para que TODAS as cenas
/// tenham as mesmas listas (os índices precisam bater entre cenas).
/// </summary>
public class PlayerArmLoadout : MonoBehaviour
{
    [System.Serializable]
    public class Braco
    {
        public string nome = "Braço";
        [Tooltip("GameObject do braço a ativar (ex.: Arm.L ou ArmGun.L).")]
        public GameObject objeto;
        public bool desbloqueado = true;
    }

    [Header("Braço Esquerdo (ex.: Arm.L, ArmGun.L)")]
    public List<Braco> esquerdo = new();

    [Header("Braço Direito (ex.: Arm.R, ArmGun.R)")]
    public List<Braco> direito = new();

    // Memória que sobrevive à troca de cena (na mesma sessão de jogo)
    private static int _savedEsq = 0;
    private static int _savedDir = 0;

    public int IndexEsquerdo { get; private set; }
    public int IndexDireito  { get; private set; }

    private void Start()
    {
        // Reaplica o loadout salvo (ou o padrão 0) assim que o jogador nasce
        AplicarEsquerdo(Clamp(esquerdo, _savedEsq));
        AplicarDireito(Clamp(direito, _savedDir));
    }

    // Chamado pela estação de troca: equipa, valida e SALVA
    public void EquiparEsquerdo(int index)
    {
        if (!Valido(esquerdo, index) || !esquerdo[index].desbloqueado) return;
        AplicarEsquerdo(index);
        _savedEsq = index;
    }

    public void EquiparDireito(int index)
    {
        if (!Valido(direito, index) || !direito[index].desbloqueado) return;
        AplicarDireito(index);
        _savedDir = index;
    }

    /// Zera o loadout — chame ao começar uma RUN nova, se quiser.
    public static void Resetar()
    {
        _savedEsq = 0;
        _savedDir = 0;
    }

    // -------------------------------------------------------

    private void AplicarEsquerdo(int index) { IndexEsquerdo = index; Ativar(esquerdo, index); }
    private void AplicarDireito(int index)  { IndexDireito  = index; Ativar(direito,  index); }

    private void Ativar(List<Braco> lista, int index)
    {
        for (int i = 0; i < lista.Count; i++)
            if (lista[i] != null && lista[i].objeto != null)
                lista[i].objeto.SetActive(i == index);
    }

    private bool Valido(List<Braco> lista, int i) => i >= 0 && i < lista.Count && lista[i] != null;

    private int Clamp(List<Braco> lista, int i) => lista.Count == 0 ? 0 : Mathf.Clamp(i, 0, lista.Count - 1);
}
