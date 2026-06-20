using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Vai DIRETO no GameObject do braço de arma (ArmGun.L, ArmGun.R).
/// Atira na direção da mira (PlayerAttack.AimForward) ao clicar. Quando o braço
/// é desativado pelo swap, este script para.
/// </summary>
[DisallowMultipleComponent]
public class GunArm : MonoBehaviour
{
    // Trava global de disparo (a estação de troca liga/desliga ao abrir o painel)
    public static int BloqueiosTiro;

    [Header("Disparo")]
    [SerializeField] private Projectile projetil;
    [Tooltip("De onde a bala sai (ponta do cano). Se vazio, usa este objeto.")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private int damage = 3;
    [SerializeField] private float bulletSpeed = 30f;
    [SerializeField] private float knockbackForce = 4f;
    [SerializeField] private float fireRate = 6f;        // tiros por segundo (limite)
    [SerializeField] private bool fullAuto = false;      // segurar para atirar (senão, 1 por clique)
    [SerializeField] private float projectileLife = 3f;
    [SerializeField] private int mouseButton = 0;        // 0 = botão esquerdo
    [Tooltip("O que a bala atinge: inimigos + paredes/obstáculos")]
    [SerializeField] private LayerMask hitMask;

    [Header("Mira")]
    [Tooltip("Se ligado, atira na direção do cano (muzzle.forward) em vez da mira do PlayerAttack.")]
    [SerializeField] private bool mirarPeloCano = false;

    private float _nextFire;
    private PlayerAttack _attack;
    private Transform Muzzle => muzzle != null ? muzzle : transform;

    private void Awake() => _attack = GetComponentInParent<PlayerAttack>();

    private void Update()
    {
        if (BloqueiosTiro > 0) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        bool quer = fullAuto ? Input.GetMouseButton(mouseButton) : Input.GetMouseButtonDown(mouseButton);
        if (!quer || Time.time < _nextFire) return;

        _nextFire = Time.time + (fireRate > 0f ? 1f / fireRate : 0f);
        Atirar();
    }

    private void Atirar()
    {
        if (projetil == null) return;

        Vector3 dir = (!mirarPeloCano && _attack != null) ? _attack.AimForward : Muzzle.forward;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = Muzzle.forward;
        dir.Normalize();

        Projectile p = Instantiate(projetil, Muzzle.position, Quaternion.LookRotation(dir, Vector3.up));
        p.Launch(dir, bulletSpeed, damage, knockbackForce, hitMask, projectileLife, transform.root);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 d = (!mirarPeloCano && _attack != null) ? _attack.AimForward : Muzzle.forward;
        if (d.sqrMagnitude > 0.0001f)
            Gizmos.DrawLine(Muzzle.position, Muzzle.position + d.normalized * 2f);
    }
}