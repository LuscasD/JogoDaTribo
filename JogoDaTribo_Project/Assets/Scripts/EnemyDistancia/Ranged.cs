using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Inimigo de ataque à distância. Persegue até a distância de tiro, encara o
/// player e dispara projéteis no cooldown. Se o player chega perto demais, ele
/// recua (kiting). Reage a dano com o mesmo knockback/faísca do Melee.
/// </summary>
public class Ranged : Enemy
{
    private NavMeshAgent nav;
    private Transform playerTransform;

    [Header("Combate à Distância")]
    [Tooltip("Distância em que ele para e começa a atirar.")]
    [SerializeField] private float attackDistance = 10f;
    [Tooltip("Se o player chegar mais perto que isso, ele recua (kiting).")]
    [SerializeField] private float tooCloseDistance = 5f;
    [Tooltip("Velocidade de giro para mirar (graus/s).")]
    [SerializeField] private float faceSpeed = 360f;
    [Tooltip("Tempo entre tiros.")]
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Projétil")]
    [SerializeField] private EnemyProjectile projectilePrefab;
    [Tooltip("De onde o tiro sai (ponta do cano). Se vazio, usa o próprio inimigo.")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private float projectileSpeed = 18f;
    [SerializeField] private int projectileDamage = 1;
    [SerializeField] private float projectileKnockback = 4f;
    [SerializeField] private float projectileLife = 4f;
    [Tooltip("O que o tiro atinge: Player + paredes/obstáculos.")]
    [SerializeField] private LayerMask projectileHitMask;

    [Header("Linha de Tiro")]
    [SerializeField] private bool requireLineOfSight = true;
    [Tooltip("Paredes/obstáculos que bloqueiam o tiro.")]
    [SerializeField] private LayerMask obstacleMask;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float knockbackMultiplier = 2.5f;
    [SerializeField] private float knockbackDuration = 0.3f;
    [SerializeField] private float upwardBias = 0.15f;
    [SerializeField] private float stunDuration = 0.4f;

    [Header("Efeitos Visuais")]
    [SerializeField] private ParticleSystem hitVfxPrefab;
    [SerializeField] private float vfxHeightOffset = 1f;

    private State currentState;
    private bool isKnockedBack;
    private float _nextShot;

    private enum State { Idle, Chasing, Attacking, TakingDamage }

    // ─────────────────────────────────────────────────────────────────────
    protected override void Start()
    {
        base.Start();
        nav = GetComponent<NavMeshAgent>();
        nav.speed = speed;
        nav.updateRotation = false; // a mira é manual (sempre encara o player)
        currentState = State.Idle;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    protected override void Update()
    {
        base.Update();
        if (isKnockedBack) return;

        switch (currentState)
        {
            case State.Idle: HandleIdle(); break;
            case State.Chasing: HandleChasing(); break;
            case State.Attacking: HandleAttacking(); break;
        }
    }

    // ── Estados ──────────────────────────────────────────────────────────
    private void HandleIdle()
    {
        if (playerTransform == null) return;
        if (Vector3.Distance(transform.position, playerTransform.position) <= vision_radius)
            currentState = State.Chasing;
    }

    private void HandleChasing()
    {
        if (playerTransform == null || !nav.isOnNavMesh) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (dist > desaggroDistance)
        {
            nav.ResetPath();
            nav.velocity = Vector3.zero;
            currentState = State.Idle;
            return;
        }

        FaceTarget(playerTransform);

        if (dist <= attackDistance)
        {
            nav.ResetPath();
            nav.velocity = Vector3.zero;
            currentState = State.Attacking;
            return;
        }

        nav.SetDestination(playerTransform.position);
    }

    private void HandleAttacking()
    {
        if (playerTransform == null) { currentState = State.Idle; return; }

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (dist > desaggroDistance) { currentState = State.Idle; return; }
        if (dist > attackDistance * 1.15f) { currentState = State.Chasing; return; }

        FaceTarget(playerTransform);

        // Kiting: recua se o player estiver perto demais; senão, para.
        if (nav.isOnNavMesh)
        {
            if (dist < tooCloseDistance)
            {
                Vector3 away = (transform.position - playerTransform.position).normalized;
                nav.SetDestination(transform.position + away * 3f);
            }
            else
            {
                nav.ResetPath();
                nav.velocity = Vector3.zero;
            }
        }

        // Atira no cooldown, se tiver linha de tiro
        if (Time.time >= _nextShot && TemLinhaDeTiro())
        {
            _nextShot = Time.time + attackCooldown;
            Atirar();
        }
    }

    // ── Mira e tiro ──────────────────────────────────────────────────────
    private void FaceTarget(Transform t)
    {
        Vector3 dir = t.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        Quaternion look = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, look, faceSpeed * Time.deltaTime);
    }

    private bool TemLinhaDeTiro()
    {
        if (!requireLineOfSight || playerTransform == null) return true;
        Vector3 origin = (muzzle != null ? muzzle : transform).position;
        Vector3 to = (playerTransform.position + Vector3.up * 0.5f) - origin;
        // se o raycast bate em algo do obstacleMask antes do player, está bloqueado
        return !Physics.Raycast(origin, to.normalized, to.magnitude, obstacleMask, QueryTriggerInteraction.Ignore);
    }

    private void Atirar()
    {
        if (projectilePrefab == null || playerTransform == null) return;

        Transform m = muzzle != null ? muzzle : transform;
        Vector3 origin = m.position;

        Vector3 dir = playerTransform.position - origin;
        dir.y = 0f; // tiro horizontal
        if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;
        dir.Normalize();

        EnemyProjectile p = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(dir, Vector3.up));
        p.Launch(dir, projectileSpeed, projectileDamage, projectileKnockback, projectileHitMask, projectileLife, transform);

        PlayAttackAnimation();
    }

    // ── TakeDamage: faísca SEMPRE + knockback (igual ao Melee) ───────────
    public override void TakeDamage(int damage, Vector3 knockbackDir = default)
    {
        base.TakeDamage(damage, knockbackDir);

        SpawnHitVfx();

        if (life <= 0) return;

        StopAllCoroutines();
        StartCoroutine(KnockbackRoutine(knockbackDir));
    }

    private void SpawnHitVfx()
    {
        if (hitVfxPrefab == null) return;

        Vector3 pos = transform.position + Vector3.up * vfxHeightOffset;
        ParticleSystem vfx = Instantiate(hitVfxPrefab, pos, Quaternion.identity);
        vfx.Play();

        var main = vfx.main;
        float vida = main.duration + main.startLifetime.constantMax;
        if (vida <= 0.05f) vida = 1f;
        Destroy(vfx.gameObject, vida);
    }

    private IEnumerator KnockbackRoutine(Vector3 incoming)
    {
        isKnockedBack = true;
        currentState = State.TakingDamage;

        if (nav != null && nav.enabled)
        {
            nav.isStopped = true;
            nav.ResetPath();
            nav.enabled = false;
        }

        if (rb != null)
        {
            Vector3 dir = incoming;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) dir = -transform.forward;
            dir.Normalize();

            float strength = Mathf.Max(incoming.magnitude * knockbackMultiplier, knockbackForce);

            rb.isKinematic = false;
            Vector3 vel = dir * strength;
            vel.y = strength * upwardBias;
            rb.velocity = vel;
        }

        yield return new WaitForSeconds(knockbackDuration);
        if (this == null || gameObject == null) yield break;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }

        yield return null;

        if (nav != null)
        {
            nav.enabled = true;
            nav.updateRotation = false;
            nav.Warp(transform.position);
            nav.isStopped = false;
        }

        yield return new WaitForSeconds(stunDuration);
        if (this == null || gameObject == null) yield break;

        isKnockedBack = false;
        currentState = playerTransform != null ? State.Chasing : State.Idle;
    }

    // ── Gizmos ────────────────────────────────────────────────────────────
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, tooCloseDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, vision_radius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, desaggroDistance);
    }
}
