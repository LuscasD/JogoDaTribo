using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float iFrameDuration = 0.8f;

    public UnityEvent<int, int> OnHealthChanged;
    public UnityEvent OnDeath;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;

    private float lastDamageTime = -999f;
    private PlayerMovment movement;

    private void Awake()
    {
        movement = GetComponent<PlayerMovment>();
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(int damage, Vector3 knockbackDir = default)
    {
        if (Time.time < lastDamageTime + iFrameDuration) return;

        lastDamageTime = Time.time;
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (knockbackDir != Vector3.zero)
            movement?.ApplyKnockback(knockbackDir);

        if (CurrentHealth <= 0)
            Die();
    }

    private void Die()
    {
        OnDeath?.Invoke();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
