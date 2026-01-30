using UnityEngine;

/// <summary>
/// Quản lý máu và nhận damage của Player, implement IDamageable.
/// </summary>
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("References")]
    [SerializeField] private HealthBar healthBar;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0;

    private void Awake()
    {
        if (currentHealth <= 0)
            currentHealth = maxHealth;
    }

    private void Start()
    {
        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (healthBar != null)
            healthBar.SetHealth(currentHealth);

        if (IsDead)
            Die();
    }

    public void Die()
    {
        currentHealth = 0;
        if (healthBar != null)
            healthBar.SetHealth(0);

        // Vô hiệu hóa điều khiển, có thể gọi event hoặc reload scene
        var movement = GetComponent<PlayerMovement2D>();
        if (movement != null)
            movement.enabled = false;

        // TODO: trigger death animation, game over UI, respawn...
        Debug.Log("Player died.");
    }

    /// <summary>
    /// Hồi máu (optional).
    /// </summary>
    public void Heal(int amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        if (healthBar != null)
            healthBar.SetHealth(currentHealth);
    }
}
