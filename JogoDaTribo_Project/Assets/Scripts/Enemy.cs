using System.Collections;
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

    [Header("Knockback")]
    [SerializeField] private float knockbackDuration = 0.25f;

    [Header("Referências (Opcional)")]
    [SerializeField] protected Animator animator;

    protected NavMeshAgent navAgent;
    private bool isKnockedBack;

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
        {
            Die();
            return;
        }

        if (knockbackDir != Vector3.zero && rb != null && !isKnockedBack)
            StartCoroutine(ApplyKnockback(knockbackDir));
    }

    private IEnumerator ApplyKnockback(Vector3 direction)
    {
        isKnockedBack = true;

        if (navAgent != null) navAgent.enabled = false;
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.AddForce(direction, ForceMode.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        if (navAgent != null) navAgent.enabled = true;

        isKnockedBack = false;
    }

    protected virtual void Die()
    {
        GameManager.Instance.scrap += 3;
        OnEnemyDied?.Invoke();
        Destroy(gameObject);
    }

    public static event System.Action OnEnemyDied;
}
