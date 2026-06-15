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
    [SerializeField] private float stunDuration = 0.4f;
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float knockbackDuration = 0.25f; // tempo voando após knockback

    // 👇 NOVO: Referência para o Prefab do seu VFX de faísca
    [Header("Efeitos Visuais")]
    [SerializeField] private ParticleSystem hitVfxPrefab;
    [SerializeField] private float vfxHeightOffset = 1f; // Ajuste a altura da faísca no corpo do inimigo

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

        // Nada roda enquanto knockback físico está ativo
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

        // Gira até alinhar com o player
        yield return RotateUntilAligned(playerTransform, 1000f);
        yield return new WaitForSeconds(0.2f);

        PlayAttackAnimation();

        // Dash via nav.Move — fica no NavMesh, sem conflito com Rigidbody
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

    // ── TakeDamage com knockback corrigido e VFX ─────────────────────────
    public override void TakeDamage(int damage, Vector3 knockbackDir = default)
    {
        base.TakeDamage(damage, knockbackDir);

        // 👇 NOVO: Instancia e toca o efeito visual de impacto
        if (hitVfxPrefab != null)
        {
            // Posição: Pega a posição do inimigo e sobe um pouco para não ficar no pé
            Vector3 spawnPosition = transform.position + (Vector3.up * vfxHeightOffset);

            // Instancia o VFX
            ParticleSystem vfxInstance = Instantiate(hitVfxPrefab, spawnPosition, Quaternion.identity);

            // Toca a partícula
            vfxInstance.Play();

            // Destrói o GameObject da partícula após ela terminar de tocar para não pesar a memória
            float destroyDelay = vfxInstance.main.duration + vfxInstance.main.startLifetime.constantMax;
            Destroy(vfxInstance.gameObject, destroyDelay);
        }

        if (life <= 0) return;

        StopAllCoroutines();
        isAttacking = false;

        StartCoroutine(KnockbackRoutine(knockbackDir));
    }

    private IEnumerator KnockbackRoutine(Vector3 knockbackDir)
    {
        isKnockedBack = true;
        currentState = State.TakingDamage;

        // ── 1. Para o NavMeshAgent ──────────────────────────────────────
        if (nav != null && nav.enabled)
        {
            nav.isStopped = true;
            nav.ResetPath();
            nav.enabled = false;
        }

        // ── 2. Aplica impulso no Rigidbody ─────────────────────────────
        if (rb != null && knockbackDir != Vector3.zero)
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;

            // Força horizontal pura — evita que o inimigo voe para cima
            Vector3 force = knockbackDir;
            force.y = 0.15f; // leve arco, não zerar o y completamente
            rb.AddForce(force.normalized * knockbackForce, ForceMode.Impulse);
        }

        // ── 3. Aguarda o tempo de voo ───────────────────────────────────
        yield return new WaitForSeconds(knockbackDuration);

        // ── 4. Reativa o NavMeshAgent ───────────────────────────────────
        if (this == null || gameObject == null) yield break;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Aguarda 1 frame para física terminar
        yield return null;

        if (nav != null)
        {
            // Warp ressincroniza o agente à posição atual do Transform
            nav.enabled = true;
            nav.Warp(transform.position);
            nav.isStopped = false;
        }

        // ── 5. Stun: parado por stunDuration antes de retomar IA ────────
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