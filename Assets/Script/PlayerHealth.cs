using UnityEngine;

/// <summary>
/// Manages health and damage for a Player.
/// The HealthBar reference is resolved at Start():
///   1. Uses the [SerializeField] if assigned in the Prefab Inspector.
///   2. Otherwise searches the scene for a HealthBar component (works when
///      the Canvas lives on the Camera, not on the Player prefab).
/// </summary>
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("References")]
    [Tooltip("Drag the HealthBar here in the Prefab. Leave empty to auto-find in scene.")]
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private int damageFromEnemy = 10;

    private Animator animator;
    private float lastDamageTime = -999f;
    private float damageCooldown = 1.0f;

    // ─── Properties ───────────────────────────────────────────────────────────
    public int  MaxHealth     => maxHealth;
    public int  CurrentHealth => currentHealth;
    public bool IsDead        => currentHealth <= 0;

    // ─── Lifecycle ────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (currentHealth <= 0)
            currentHealth = maxHealth;

        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInParent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (animator != null)
            Debug.Log("[PlayerHealth] Animator found.");
        else
            Debug.LogError("[PlayerHealth] Animator NOT found! Player needs an Animator component.");
    }

    private void Start()
    {
        // ── Auto-find HealthBar if not assigned externally ─────────────────
        // Fallback: search scene (works if canvas was placed manually in editor)
        if (healthBar == null)
        {
            healthBar = FindFirstObjectByType<HealthBar>();
            if (healthBar != null)
                Debug.Log("[PlayerHealth] Auto-found HealthBar in scene.");
            else
                Debug.LogWarning("[PlayerHealth] No HealthBar found. Call SetHealthBar() from PlayerSpawner.");
        }

        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);
    }

    /// <summary>
    /// Called by PlayerSpawner to inject the runtime-instantiated HealthBar.
    /// </summary>
    public void SetHealthBar(HealthBar hb)
    {
        healthBar = hb;
        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);
    }

    // ─── IDamageable ──────────────────────────────────────────────────────────
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
        Debug.Log("[PlayerHealth] Player died.");
        currentHealth = 0;

        if (healthBar != null)
            healthBar.SetHealth(0);

        if (animator != null)
            animator.SetTrigger("Die");
        else
            Debug.LogError("[PlayerHealth] Cannot trigger Die — Animator is null.");

        // Disable movement
        PlayerMovement2D movement = GetComponent<PlayerMovement2D>();
        if (movement == null) movement = GetComponentInParent<PlayerMovement2D>();
        if (movement != null) movement.SetInputEnabled(false);

        // Disable collider
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) col = GetComponentInParent<Collider2D>();
        if (col != null) col.enabled = false;

        // Disable player component (your original approach, done safely)
        Player player = GetComponent<Player>();
        if (player == null) player = GetComponentInParent<Player>();
        if (player != null)
        {
            player.enabled = false;
            // Notify PlayerController so OnAllPlayersDied can fire
            if (PlayerController.Instance != null)
                PlayerController.Instance.NotifyPlayerDied(player);
        }

        // TODO: load scene end game sau 2 giây (làm sau)
        // Invoke("LoadGameOverScene", 2f);
    }

    // ─── Heal ─────────────────────────────────────────────────────────────────
    public void Heal(int amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        if (healthBar != null)
            healthBar.SetHealth(currentHealth);
    }

    // ─── Collision ────────────────────────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;
        if (Time.time - lastDamageTime < damageCooldown) return;

        lastDamageTime = Time.time;
        TakeDamage(damageFromEnemy);
        Debug.Log("[PlayerHealth] Took damage from Enemy.");
    }
}
