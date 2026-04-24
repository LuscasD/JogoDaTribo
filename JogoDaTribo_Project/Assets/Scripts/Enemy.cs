using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Info Base")]
    [SerializeField] protected int life;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected int MaxLife;
    [SerializeField] protected float speed;
    [SerializeField] protected float vision_radius;

    protected virtual void Start()
    {
        life = MaxLife;
    }

    public virtual void TakeDamage(int damage, Vector3 knockbackDir = default)
    {
        life -= damage;
        if (life <= 0)
            Die();
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}
