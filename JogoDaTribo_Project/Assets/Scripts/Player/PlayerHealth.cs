using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

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

    public TextMeshProUGUI  lifeText;

    private void Awake()
    {
        movement = GetComponent<PlayerMovment>();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
            maxHealth = GameManager.Instance.playerHealth;

        CurrentHealth = maxHealth;
        lifeText?.SetText("Vida: " + CurrentHealth);
    }

    public void TakeDamage(int damage, Vector3 knockbackDir = default)
    {
        if (Time.time < lastDamageTime + iFrameDuration) return;

        lastDamageTime = Time.time;
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        lifeText.text = "Vida: "+ CurrentHealth;

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
