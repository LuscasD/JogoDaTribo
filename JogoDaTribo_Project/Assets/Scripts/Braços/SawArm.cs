using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Vai DIRETO no GameObject do braço de serra (Arm.L, Arm.R).
/// Causa dano por contato enquanto o torso gira, lendo a velocidade angular
/// do PlayerAttack. Quando o braço é desativado pelo swap, este script para.
/// </summary>
[DisallowMultipleComponent]
public class SawArm : MonoBehaviour
{
    [Header("Dano")]
    [SerializeField] private float minAngularSpeed = 30f;   // graus/s mínimo para causar dano
    [SerializeField] private float maxAngularSpeed = 360f;
    [SerializeField] private int minDamage = 1;
    [SerializeField] private int maxDamage = 5;
    [SerializeField] private float hitCooldownPerEnemy = 0.4f;
    [SerializeField] private float knockbackForce = 7f;

    [Header("Hitbox")]
    [Tooltip("Onde detecta o inimigo. Se vazio, usa este objeto. Ex.: arraste Weapon.L (a lâmina).")]
    [SerializeField] private Transform tip;
    [SerializeField] private float sawRadius = 0.5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;

    private readonly Dictionary<int, float> _hits = new();
    private PlayerAttack _attack;

    private Transform Tip => tip != null ? tip : transform;

    private void Awake() => _attack = GetComponentInParent<PlayerAttack>();
    private void OnEnable() => _hits.Clear();

    private void FixedUpdate()
    {
        float speed = _attack != null ? _attack.AngularSpeed : 0f;
        if (speed < minAngularSpeed) return;

        Collider[] cols = Physics.OverlapSphere(Tip.position, sawRadius, enemyLayer);
        foreach (var col in cols)
        {
            int id = col.gameObject.GetInstanceID();
            if (_hits.TryGetValue(id, out float last) && Time.time - last < hitCooldownPerEnemy)
                continue;
            _hits[id] = Time.time;

            float t = Mathf.InverseLerp(minAngularSpeed, maxAngularSpeed, speed);
            int damage = Mathf.RoundToInt(Mathf.Lerp(minDamage, maxDamage, t));

            Vector3 dir = col.transform.position - transform.root.position;
            dir.y = 0f; dir.Normalize();

            var enemy = col.GetComponent<Enemy>() ?? col.GetComponentInParent<Enemy>();
            enemy?.TakeDamage(damage, dir * knockbackForce);

            _attack?.OnSawHit();
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        float speed = (Application.isPlaying && _attack != null) ? _attack.AngularSpeed : 0f;
        float t = Mathf.InverseLerp(minAngularSpeed, maxAngularSpeed, speed);
        Gizmos.color = Color.Lerp(Color.green, Color.red, t);
        Gizmos.DrawWireSphere(Tip.position, sawRadius);
    }
}