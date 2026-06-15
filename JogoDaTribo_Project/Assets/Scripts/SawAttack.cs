using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Detecta quando as pontas das serras entram em contato com inimigos
/// e aplica dano proporcional à velocidade angular do torso.
/// O knockback é delegado ao Enemy.TakeDamage — que já cuida de
/// desligar o NavMeshAgent e aplicar o impulso via Rigidbody.
/// </summary>
public class SawAttack : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform body;
    [SerializeField] private Transform armL;
    [SerializeField] private Transform armR;

    [Header("Dano da Serra")]
    [SerializeField] private float minAngularSpeed = 30f;
    [SerializeField] private float maxAngularSpeed = 360f;
    [SerializeField] private int minDamage = 1;
    [SerializeField] private int maxDamage = 5;
    [SerializeField] private float hitCooldownPerEnemy = 0.4f;
    [SerializeField] private float knockbackForce = 7f;

    [Header("Hitbox da Serra")]
    [SerializeField] private float sawRadius = 0.35f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;

    // ── Estado interno ─────────────────────────────────────────────
    private Quaternion _lastBodyRot;
    private float _currentAngularSpeed;
    private Dictionary<int, float> _hitTimestamps = new Dictionary<int, float>();

    // Pontas das serras
    private Vector3 TipL => armL != null ? armL.position + armL.right * -1.5f : Vector3.zero;
    private Vector3 TipR => armR != null ? armR.position + armR.right * 1.5f : Vector3.zero;

    private PlayerAttack _playerAttack;

    // ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        _playerAttack = GetComponent<PlayerAttack>();
    }

    private void Start()
    {
        if (body != null) _lastBodyRot = body.rotation;
    }

    private void Update() => MeasureAngularSpeed();
    private void FixedUpdate()
    {
        if (armL != null) CheckSawTip(TipL);
        if (armR != null) CheckSawTip(TipR);
    }

    // ─────────────────────────────────────────────────────────────
    private void MeasureAngularSpeed()
    {
        if (body == null) return;
        float angle = Quaternion.Angle(_lastBodyRot, body.rotation);
        _currentAngularSpeed = angle / Time.deltaTime;
        _lastBodyRot = body.rotation;
    }

    private void CheckSawTip(Vector3 tipPos)
    {
        if (_currentAngularSpeed < minAngularSpeed) return;

        Collider[] hits = Physics.OverlapSphere(tipPos, sawRadius, enemyLayer);

        foreach (var col in hits)
        {
            int id = col.gameObject.GetInstanceID();

            if (_hitTimestamps.TryGetValue(id, out float lastHit) &&
                Time.time - lastHit < hitCooldownPerEnemy)
                continue;

            _hitTimestamps[id] = Time.time;

            // Dano escalado pela velocidade angular
            float t = Mathf.InverseLerp(minAngularSpeed, maxAngularSpeed, _currentAngularSpeed);
            int damage = Mathf.RoundToInt(Mathf.Lerp(minDamage, maxDamage, t));

            // Direção do knockback: robô → inimigo no plano horizontal
            Vector3 kbDir = col.transform.position - transform.position;
            kbDir.y = 0f;
            kbDir = kbDir.normalized;

            // O Enemy.TakeDamage já cuida do knockback interno (NavMeshAgent + Rigidbody)
            var enemy = col.GetComponent<Enemy>() ?? col.GetComponentInParent<Enemy>();
            enemy?.TakeDamage(damage, kbDir * knockbackForce);

            // Congela rotação do torso ao acertar
            _playerAttack?.OnSawHit();
        }
    }

    // ─────────────────────────────────────────────────────────────
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        float t = Mathf.InverseLerp(minAngularSpeed, maxAngularSpeed, _currentAngularSpeed);
        Gizmos.color = Color.Lerp(Color.green, Color.red, t);

        if (armL != null) Gizmos.DrawWireSphere(TipL, sawRadius);
        if (armR != null) Gizmos.DrawWireSphere(TipR, sawRadius);
    }

    public float AngularSpeed => _currentAngularSpeed;
}