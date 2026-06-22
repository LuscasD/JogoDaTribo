using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Melee : Enemy
{
    private NavMeshAgent nav;
    private Transform playerTransform;
    private PlayerHealth playerHealth;

    [Header("Combate")]
    [SerializeField] private float attackDistance = 6f;
    [SerializeField] private float hitArea = 3f;
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 2f;

    [Header("Knockback")]
    [Tooltip("Velocidade mínima de recuo (m/s), usada como piso.")]
    [SerializeField] private float knockbackForce = 10f;
    [Tooltip("Multiplica a força de knockback que vem da arma. Aumente para empurrar mais longe.")]
    [SerializeField] private float knockbackMultiplier = 2.5f;
    [Tooltip("Tempo voando após o impacto.")]
    [SerializeField] private float knockbackFlyTime = 0.3f;
    [Tooltip("Leve arco pra cima (0 = puramente horizontal).")]
    [SerializeField] private float upwardBias = 0.15f;
    [SerializeField] private float stunDuration = 0.4f;

    [Header("Efeitos Visuais")]
    [SerializeField] private ParticleSystem hitVfxPrefab;
    [SerializeField] private float vfxHeightOffset = 1f;

    private State currentState;
    private bool isAttacking;
    private bool isKnockedBack;

    private enum State { Idle, Chasing, Attacking, TakingDamage }

    // ─────────────────────────────────────────────────────────────────────
    protected override void Start()
    {
        base.Start();
        nav = GetComponent<NavMeshAgent>();
        nav.speed = speed;
        currentState = State.Idle;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerHealth = player.GetComponent<PlayerHealth>();
        }
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

        nav.SetDestination(playerTransform.position);

        if (!nav.pathPending && nav.remainingDistance <= attackDistance)
        {
            nav.ResetPath();
            currentState = State.Attacking;
        }
    }

    private void HandleAttacking()
    {
        if (isAttacking) return;
        StartCoroutine(AttackRoutine());
    }

    // ── Rotina de ataque ─────────────────────────────────────────────────
    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        if (playerTransform == null || !nav.isOnNavMesh)
        {
            isAttacking = false;
            yield break;
        }

        yield return RotateUntilAligned(playerTransform, 1000f);
        yield return new WaitForSeconds(0.2f);

        PlayAttackAnimation();

        nav.ResetPath();
        Vector3 dashDir = transform.forward;
        dashDir.y = 0f;
        dashDir.Normalize();

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            if (playerTransform != null &&
                Vector3.Distance(transform.position, playerTransform.position) <= 0.9f)
                break;

            nav.Move(dashDir * dashSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        TryDamagePlayer();
        StopAttackAnimation();

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;

        if (playerTransform == null) { currentState = State.Idle; yield break; }
        float d = Vector3.Distance(transform.position, playerTransform.position);
        currentState = d > attackDistance ? State.Chasing : State.Attacking;
    }

    private void TryDamagePlayer()
    {
        if (playerHealth == null || playerTransform == null) return;

        Vector3 hitOrigin = transform.position + transform.forward * 1.5f;
        Collider[] hits = Physics.OverlapSphere(hitOrigin, hitArea, LayerMask.GetMask("Player"));

        foreach (var hit in hits)
        {
            var ph = hit.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                Vector3 knockback = (playerTransform.position - transform.position).normalized * knockbackForce;
                ph.TakeDamage(attackDamage, knockback);
                break;
            }
        }
    }

    // ── TakeDamage: faísca SEMPRE + knockback forte ──────────────────────
    public override void TakeDamage(int damage, Vector3 knockbackDir = default)
    {
        base.TakeDamage(damage, knockbackDir);

        SpawnHitVfx(); // toca a faísca em TODO hit, vivo ou morto

        if (life <= 0) return; // morreu: não roda knockback/stun

        StopAllCoroutines();
        isAttacking = false;
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
        if (vida <= 0.05f) vida = 1f; // fallback caso a duração venha 0
        Destroy(vfx.gameObject, vida);
    }

    private IEnumerator KnockbackRoutine(Vector3 incoming)
    {
        isKnockedBack = true;
        currentState = State.TakingDamage;

        // 1. Desliga o NavMeshAgent
        if (nav != null && nav.enabled)
        {
            nav.isStopped = true;
            nav.ResetPath();
            nav.enabled = false;
        }

        // 2. Lança via VELOCIDADE direta (independe da massa)
        if (rb != null)
        {
            Vector3 dir = incoming;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) dir = -transform.forward; // fallback
            dir.Normalize();

            // Respeita a força que veio da arma, amplificada; com piso de knockbackForce
            float strength = Mathf.Max(incoming.magnitude * knockbackMultiplier, knockbackForce);

            rb.isKinematic = false;
            Vector3 vel = dir * strength;
            vel.y = strength * upwardBias; // leve arco
            rb.velocity = vel;
        }

        // 3. Tempo de voo
        yield return new WaitForSeconds(knockbackFlyTime);
        if (this == null || gameObject == null) yield break;

        // 4. Para e volta a ser kinematic
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }

        yield return null; // 1 frame para a física assentar

        // 5. Reativa o NavMeshAgent na posição atual
        if (nav != null)
        {
            nav.enabled = true;
            nav.Warp(transform.position);
            nav.isStopped = false;
        }

        // 6. Stun
        yield return new WaitForSeconds(stunDuration);
        if (this == null || gameObject == null) yield break;

        isKnockedBack = false;
        currentState = playerTransform != null ? State.Chasing : State.Idle;
    }

    // ── Utilitários ──────────────────────────────────────────────────────
    private IEnumerator RotateUntilAligned(Transform target, float rotSpeed, float tolerance = 1f)
    {
        while (target != null && !IsAligned(target, tolerance))
        {
            Vector3 dir = (target.position - transform.position).normalized;
            dir.y = 0f;
            if (dir != Vector3.zero)
            {
                Quaternion look = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, look, rotSpeed * Time.deltaTime);
            }
            yield return null;
        }
    }

    private bool IsAligned(Transform target, float tolerance)
    {
        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0f;
        if (dir == Vector3.zero) return true;
        return Quaternion.Angle(transform.rotation, Quaternion.LookRotation(dir)) < tolerance;
    }

    // ── Gizmos ────────────────────────────────────────────────────────────
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 1.5f, hitArea);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, vision_radius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, desaggroDistance);
    }
}