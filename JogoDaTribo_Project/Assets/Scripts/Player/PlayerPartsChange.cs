using System.Collections.Generic;
using UnityEngine;

public class PlayerPartsChange : MonoBehaviour
{
    [System.Serializable]
    public class ArmPartData
    {
        public string partName;
        public GameObject prefab;
        public int damageBonus;
        [HideInInspector] public bool isUnlocked;
    }

    [Header("Braço Esquerdo")]
    [SerializeField] private SkinnedMeshRenderer leftArmRenderer;
    public List<ArmPartData> leftArmParts;

    [Header("Braço Direito")]
    [SerializeField] private SkinnedMeshRenderer rightArmRenderer;
    public List<ArmPartData> rightArmParts;

    public int CurrentLeftArm  { get; private set; } = 0;
    public int CurrentRightArm { get; private set; } = 0;

    private PlayerAttack playerAttack;

    private void Awake()
    {
        playerAttack = GetComponent<PlayerAttack>();

        // índice 0 sempre começa desbloqueado (peça padrão)
        if (leftArmParts.Count  > 0) leftArmParts[0].isUnlocked  = true;
        if (rightArmParts.Count > 0) rightArmParts[0].isUnlocked = true;
    }

    private void Start()
    {
        if (leftArmParts.Count  > 0) ApplyMesh(leftArmRenderer,  leftArmParts[0].prefab);
        if (rightArmParts.Count > 0) ApplyMesh(rightArmRenderer, rightArmParts[0].prefab);
        UpdateDamage();
    }

    public void EquipLeftArm(int index)
    {
        if (!IsValidLeft(index) || !leftArmParts[index].isUnlocked) return;
        ApplyMesh(leftArmRenderer, leftArmParts[index].prefab);
        CurrentLeftArm = index;
        UpdateDamage();
    }

    public void EquipRightArm(int index)
    {
        if (!IsValidRight(index) || !rightArmParts[index].isUnlocked) return;
        ApplyMesh(rightArmRenderer, rightArmParts[index].prefab);
        CurrentRightArm = index;
        UpdateDamage();
    }

    public void UnlockLeftArm(int index)
    {
        if (IsValidLeft(index)) leftArmParts[index].isUnlocked = true;
    }

    public void UnlockRightArm(int index)
    {
        if (IsValidRight(index)) rightArmParts[index].isUnlocked = true;
    }

    private bool IsValidLeft(int i)  => i >= 0 && i < leftArmParts.Count;
    private bool IsValidRight(int i) => i >= 0 && i < rightArmParts.Count;

    [ContextMenu("DEBUG - Desbloquear Todos os Braços")]
    public void DebugUnlockAll()
    {
        foreach (var p in leftArmParts)  p.isUnlocked = true;
        foreach (var p in rightArmParts) p.isUnlocked = true;
        Debug.Log("[PlayerPartsChange] Todos os braços desbloqueados.");
    }

    private void ApplyMesh(SkinnedMeshRenderer target, GameObject prefab)
    {
        if (target == null || prefab == null) return;
        var source = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
        if (source == null) return;
        target.sharedMesh = source.sharedMesh;
        target.sharedMaterials = source.sharedMaterials;
    }

    private void UpdateDamage()
    {
        int bonus = 0;
        if (leftArmParts.Count  > CurrentLeftArm)  bonus += leftArmParts[CurrentLeftArm].damageBonus;
        if (rightArmParts.Count > CurrentRightArm) bonus += rightArmParts[CurrentRightArm].damageBonus;
        //playerAttack?.SetDamageBonus(bonus);
    }
}
