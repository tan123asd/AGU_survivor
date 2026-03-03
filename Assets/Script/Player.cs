using UnityEngine;

/// <summary>
/// Root component for a Player GameObject.
/// - Holds references to PlayerHealth and PlayerMovement2D.
/// - Registers / unregisters with PlayerController (singleton).
/// - Supports a PlayerIndex for multiplayer input routing.
/// </summary>
public class Player : MonoBehaviour, IDamageable
{
    [Header("Player Components")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] public PlayerMovement2D playerMovement;

    // ─── Properties ───────────────────────────────────────────────────────────
    public PlayerHealth PlayerHealth => playerHealth;
    public PlayerMovement2D PlayerMovement => playerMovement;

    /// <summary>
    /// Index assigned by PlayerSpawner.
    /// 0 = local player, 1+ = additional local/remote players.
    /// </summary>
    public int PlayerIndex { get; private set; } = 0;

    // ─── Lifecycle ────────────────────────────────────────────────────────────
    private void Awake()
    {
        // Auto-find components if not assigned in Inspector
        if (playerHealth == null)
            playerHealth = GetComponentInChildren<PlayerHealth>();
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        if (playerMovement == null)
            playerMovement = GetComponentInChildren<PlayerMovement2D>();
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement2D>();

        // Register with the central manager
        if (PlayerController.Instance != null)
            PlayerController.Instance.RegisterPlayer(this);
        else
            Debug.LogWarning("[Player] PlayerController.Instance is null. Make sure PlayerController exists in the scene.");
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance != null)
            PlayerController.Instance.UnregisterPlayer(this);
    }

    // ─── Public API ───────────────────────────────────────────────────────────

    /// <summary>Called by PlayerSpawner to assign which player slot this is.</summary>
    public void SetPlayerIndex(int index)
    {
        PlayerIndex = index;
    }

    public void TakeDamage(int damage)
    {
        if (playerHealth != null)
            playerHealth.TakeDamage(damage);
    }

    public void Die()
    {
        if (playerHealth != null)
            playerHealth.Die();

        // Notify the central manager so it can fire events
        if (PlayerController.Instance != null)
            PlayerController.Instance.NotifyPlayerDied(this);
    }
}
