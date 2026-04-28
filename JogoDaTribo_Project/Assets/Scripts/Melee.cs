using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Melee : Enemy
{
    private NavMeshAgent nav;
    private Transform playerTransform;
    private PlayerHealth playerHealth;

    [Header("Combate")]
    [SerializeField] private float stopDistance = 1.5f;
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float stunDuration = 0.4f;
    [SerializeField] private float knockbackForce = 8f;


    [SerializeField] private State currentState;
    private bool isAttacking;

    private enum State { Idle, Chasing, Attacking, TakingDamage }

    protected override void Start()
    {
        base.Start();
        nav = GetComponent<NavMeshAgent>();
        nav.speed = speed;
        nav.stoppingDistance = stopDistance;
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
        switch (currentState)
        {
            case State.Idle:      HandleIdle();      break;
            case State.Chasing:   HandleChasing();   break;
            case State.Attacking: HandleAttacking(); break;
        }
    }

    private void HandleIdle()
    {
        if (playerTransform == null) return;
        if (Vector3.Distance(transform.position, playerTransform.position) <= vision_radius)
            currentState = State.Chasing;
    }

    private void HandleChasing()
    {
        if (playerTransform == null || !nav.isOnNavMesh) return;

        nav.SetDestination(playerTransform.position);

        if (!nav.pathPending && nav.remainingDistance <= stopDistance)
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

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        if (playerTransform == null || !nav.isOnNavMesh) { isAttacking = false; yield break; }

        yield return RotateUntilAligned(playerTransform, 640f);

        PlayAttackAnimation();

        // Dash usando nav.Move — fica na superfície do NavMesh, sem física
        nav.ResetPath();
        Vector3 dashDir = transform.forward;
        dashDir.y = 0;
        dashDir.Normalize();

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            // Para o dash antes de invadir o collider do player
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
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        currentState = dist > stopDistance * 1.5f ? State.Chasing : State.Attacking;
    }

    private void TryDamagePlayer()
    {
        if (playerHealth == null || playerTransform == null) return;
        if (Vector3.Distance(transform.position, playerTransform.position) <= stopDistance * 2f)
        {
            Vector3 knockback = (playerTransform.position - transform.position).normalized * knockbackForce;
            playerHealth.TakeDamage(attackDamage, knockback);
        }
    }

    public override void TakeDamage(int damage, Vector3 knockbackDir = default)
    {
        base.TakeDamage(damage, knockbackDir);
        if (life <= 0) return;

        StopAllCoroutines();
        isAttacking = false;
        if (rb != null) { rb.velocity = Vector3.zero; rb.isKinematic = true; }
        nav.enabled = true;
        nav.ResetPath();
        StartCoroutine(StunRoutine(knockbackDir));
    }

    private IEnumerator StunRoutine(Vector3 knockbackDir)
    {
        currentState = State.TakingDamage;

        float elapsed = 0f;
        while (elapsed < stunDuration)
        {
            if (knockbackDir != Vector3.zero && nav.isOnNavMesh)
            {
                float decel = 1f - (elapsed / stunDuration);
                nav.Move(knockbackDir * decel * Time.deltaTime);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (this == null) yield break;
        currentState = playerTransform != null ? State.Chasing : State.Idle;
    }

    private IEnumerator RotateUntilAligned(Transform target, float rotSpeed, float tolerance = 1f)
    {
        while (target != null && !IsAligned(target, tolerance))
        {
            Vector3 dir = (target.position - transform.position).normalized;
            dir.y = 0;
            if (dir != Vector3.zero)
            {
                Quaternion look = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, look, rotSpeed * Time.deltaTime);
            }
            yield return null;
        }
    }

    private bool IsAligned(Transform target, float tolerance)
    {
        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0;
        if (dir == Vector3.zero) return true;
        return Quaternion.Angle(transform.rotation, Quaternion.LookRotation(dir)) < tolerance;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, vision_radius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}
