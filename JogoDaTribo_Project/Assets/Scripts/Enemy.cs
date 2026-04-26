using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Info Base")]
    [SerializeField] protected int life;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected int MaxLife = 3;
    [SerializeField] protected float speed;
    [SerializeField] protected float vision_radius;

    protected virtual void Start()
    {
        life = MaxLife;
        // NavMeshAgent controla o movimento — Rigidbody kinematic evita briga entre os dois
        if (rb != null)
            rb.isKinematic = true;
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
