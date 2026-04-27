using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Info Base")]
    [SerializeField] protected int life;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected int MaxLife = 3;
    [SerializeField] protected float speed;
    [SerializeField] protected float vision_radius;

    [Header("Referências (Opcional)")]
    [SerializeField] protected Animator animator;

    protected NavMeshAgent navAgent;

    private static readonly int AnimSpeed  = Animator.StringToHash("Speed");
    private static readonly int AnimAtacou = Animator.StringToHash("Atacou");

    protected virtual void Start()
    {
        life = MaxLife;

        navAgent = GetComponent<NavMeshAgent>();

        animator ??= GetComponentInChildren<Animator>();

        if (rb != null)
            rb.isKinematic = true;
    }

    protected virtual void Update()
    {
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        float currentSpeed = navAgent != null ? navAgent.velocity.magnitude : 0f;
        animator.SetFloat(AnimSpeed, currentSpeed);
    }

    protected void PlayAttackAnimation()
    {
        if (animator != null)
            animator.SetBool(AnimAtacou, true);
    }

    protected void StopAttackAnimation()
    {
        if (animator != null)
            animator.SetBool(AnimAtacou, false);
    }

    public virtual void TakeDamage(int damage, Vector3 knockbackDir = default)
    {
        life -= damage;
        if (life <= 0)
            Die();
    }

    protected virtual void Die()
    {
        OnEnemyDied?.Invoke();
        Destroy(gameObject);
    }

    public static event System.Action OnEnemyDied;
}
