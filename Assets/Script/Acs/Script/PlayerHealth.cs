using Photon.Pun;
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

    // ─── Photon ───────────────────────────────────────────────────────────────
    private PhotonView _photonView;

    public int MaxHealth => maxHealth;

    /// <summary>
    /// Called at runtime by PlayerSpawner to wire the HealthBar UI after spawning.
    /// </summary>
    public void SetHealthBar(HealthBar bar)
    {
        healthBar = bar;
        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);
    }
    public int CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0;

    private void Awake()
    {
        if (currentHealth <= 0)
            currentHealth = maxHealth;
        
        // Tìm Animator trên GameObject này, GameObject cha hoặc GameObject con
        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInParent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        
        if (animator != null)
            Debug.Log("Found Animator successfully!");
        else
            Debug.LogError("ANIMATOR NOT FOUND! Player GameObject cần có Animator component!");

        // PhotonView is on the root of the player prefab.
        _photonView = GetComponentInParent<PhotonView>();
    }

    private void Start()
    {
        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);
    }

    /// <summary>
    /// Called locally when offline, or via RPC in multiplayer.
    /// In multiplayer, always call SendTakeDamageRPC() instead of this directly.
    /// </summary>
    [PunRPC]
    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (healthBar != null)
            healthBar.SetHealth(currentHealth);

        if (IsDead)
            Die();
    }

    /// <summary>
    /// Sends TakeDamage to all clients via RPC.
    /// Falls back to a direct call when offline (no PhotonView).
    /// Call this from enemy collision — never call TakeDamage() directly in multiplayer.
    /// </summary>
    public void SendTakeDamageRPC(int damage)
    {
        if (_photonView != null)
            _photonView.RPC(nameof(TakeDamage), RpcTarget.All, damage);
        else
            TakeDamage(damage); // offline fallback
    }

    public void Die()
    {
        Debug.Log("=== DIE() CALLED ===");
        
        currentHealth = 0;
        
        if (healthBar != null)
            healthBar.SetHealth(0);

        // Trigger death animation
        if (animator != null)
            animator.SetTrigger("Die");
        else
            Debug.LogError("Cannot trigger Die animation - Animator is NULL!");

        // Disable movement
        PlayerMovement2D movement = GetComponentInParent<PlayerMovement2D>();
        if (movement == null) movement = GetComponent<PlayerMovement2D>();
        if (movement != null)
            movement.enabled = false;

        // Disable collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        // Notify PlayerController so it can fire OnAllPlayersDied if needed
        PlayerEntity entity = GetComponentInParent<PlayerEntity>();
        if (entity != null && PlayerController.Instance != null)
            PlayerController.Instance.NotifyPlayerDied(entity);
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

            // Use RPC in multiplayer so all clients update HP simultaneously.
            // Falls back to direct call when offline.
            SendTakeDamageRPC(damageFromEnemy);
            Debug.Log("Player took damage from Enemy!");
        }
    }
}
