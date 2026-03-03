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
<<<<<<< Updated upstream

=======
    [SerializeField] private int damageFromEnemy = 10;

    private Animator animator;
    private float lastDamageTime = -999f;
    private float damageCooldown = 1.0f;
>>>>>>> Stashed changes

    // ─── Properties ───────────────────────────────────────────────────────────
    public int  MaxHealth     => maxHealth;
    public int  CurrentHealth => currentHealth;
    public bool IsDead        => currentHealth <= 0;

    // ─── Lifecycle ────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (currentHealth <= 0)
            currentHealth = maxHealth;
<<<<<<< Updated upstream
=======

        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInParent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (animator != null)
            Debug.Log("[PlayerHealth] Animator found.");
        else
            Debug.LogError("[PlayerHealth] Animator NOT found! Player needs an Animator component.");
>>>>>>> Stashed changes
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
    /// Must be called BEFORE Start() runs, e.g. immediately after Instantiate().
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
<<<<<<< Updated upstream
        currentHealth = 0;
=======
        Debug.Log("[PlayerHealth] Player died.");
        currentHealth = 0;

>>>>>>> Stashed changes
        if (healthBar != null)
            healthBar.SetHealth(0);       

<<<<<<< Updated upstream
        // Destroy player ngay khi chết
        GameObject.FindObjectOfType<Player>().enabled = false;
        // TODO: load scene end game (làm sau) — có thể gọi trước Destroy với delay, hoặc dùng SceneManager.LoadScene trong callback
=======
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

        // Notify PlayerController so OnAllPlayersDied can fire
        Player player = GetComponent<Player>();
        if (player == null) player = GetComponentInParent<Player>();
        if (player != null && PlayerController.Instance != null)
            PlayerController.Instance.NotifyPlayerDied(player);
>>>>>>> Stashed changes
    }

    // ─── Heal ─────────────────────────────────────────────────────────────────
    public void Heal(int amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        if (healthBar != null)
            healthBar.SetHealth(currentHealth);
    }
<<<<<<< Updated upstream
=======

    // ─── Collision ────────────────────────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;
        if (Time.time - lastDamageTime < damageCooldown) return;

        lastDamageTime = Time.time;
        TakeDamage(damageFromEnemy);
        Debug.Log("[PlayerHealth] Took damage from Enemy.");
    }
>>>>>>> Stashed changes
}
