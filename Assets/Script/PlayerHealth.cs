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
    [SerializeField] private int damageFromEnemy = 10; // Damage nhận từ enemy
    
    private Animator animator;
    private float lastDamageTime = -999f;
    private float damageCooldown = 1.0f; // Cooldown giữa các lần nhận damage

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0;

    private void Awake()
    {
        if (currentHealth <= 0)
            currentHealth = maxHealth;
        
        // Tìm Animator trên GameObject này, GameObject cha hoặc GameObject con
        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInParent<Animator>(); // Tìm ở parent (Player)
        if (animator == null)
            animator = GetComponentInChildren<Animator>(); // Tìm ở children
        
        if (animator != null)
            Debug.Log("Found Animator successfully!");
        else
            Debug.LogError("ANIMATOR NOT FOUND! Player GameObject cần có Animator component!");
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
        Debug.Log("=== DIE() CALLED ===");
        
        currentHealth = 0;
        if (healthBar != null)
            healthBar.SetHealth(0);

        // Trigger animation chết giống Enemy
        if (animator != null)
        {
            Debug.Log("Triggering Die animation...");
            animator.SetTrigger("Die");
            Debug.Log("Die trigger set successfully!");
        }
        else
        {
            Debug.LogError("Cannot trigger Die animation - Animator is NULL!");
        }

        // Disable movement
        PlayerMovement2D movement = GetComponent<PlayerMovement2D>();
        if (movement != null)
            movement.enabled = false;

        // Disable collider để không va chạm nữa
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        // TODO: load scene end game sau 2 giây (làm sau)
        // Invoke("LoadGameOverScene", 2f);
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Khi chạm vào Enemy
        if (collision.CompareTag("Enemy"))
        {
            // Kiểm tra cooldown
            if (Time.time - lastDamageTime < damageCooldown)
                return;
            
            lastDamageTime = Time.time;
            
            // Nhận damage
            TakeDamage(damageFromEnemy);
            Debug.Log("Player took damage from Enemy!");
        }
    }
}
