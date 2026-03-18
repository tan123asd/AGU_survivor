using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Root component for a Player GameObject.
/// - Holds references to PlayerHealth and PlayerMovement2D.
/// - Registers / unregisters with PlayerController (singleton).
/// - Supports a PlayerIndex for multiplayer input routing.
/// - Exposes RootTransform: always the top-level GameObject transform,
///   regardless of where this script lives in the prefab hierarchy.
/// </summary>
public class PlayerEntity : MonoBehaviour
{
    [Header("Player Components")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] public PlayerMovement2D playerMovement;
    // ExperienceManager is now a scene-level singleton (ExperienceManager.Instance).
    // EXP collection is shared across all players via ExperienceManager.ShareExp().

    // ─── Photon ───────────────────────────────────────────────────────────────
    private PhotonView _photonView;

    // ─── Properties ───────────────────────────────────────────────────────────
    public PlayerHealth PlayerHealth => playerHealth;
    public PlayerMovement2D PlayerMovement => playerMovement;

    /// <summary>
    /// True for the player owned by THIS client.
    /// In single-player (no PhotonView), always true.
    /// In multiplayer, true only on the owning client.
    /// </summary>
    public bool IsLocalPlayer => _photonView == null || _photonView.IsMine;

    /// <summary>
    /// Index assigned by PlayerSpawner / NetworkPlayerSpawner.
    /// 0 = local player, 1+ = additional local/remote players.
    /// </summary>
    public int PlayerIndex { get; private set; } = 0;

    /// <summary>
    /// The root-most Transform of this player's GameObject hierarchy.
    /// Use this instead of transform when you need the actual world position
    /// of the player (in case PlayerEntity lives on a child object).
    /// Set by PlayerSpawner immediately after Instantiate().
    /// </summary>
    public Transform RootTransform { get; private set; }

    // ─── Lifecycle ────────────────────────────────────────────────────────────
    private void Awake()
    {
        // Default RootTransform to this object; PlayerSpawner will override if needed
        RootTransform = transform;

        // PhotonView on the root of the network player prefab
        _photonView = GetComponentInParent<PhotonView>();

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
            Debug.LogWarning("[PlayerEntity] PlayerController.Instance is null. Make sure PlayerController exists in the scene.");
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance != null)
            PlayerController.Instance.UnregisterPlayer(this);
    }

    // ─── Public API ───────────────────────────────────────────────────────────

    /// <summary>Called by PlayerSpawner immediately after Instantiate() to set the root transform.</summary>
    public void SetRootTransform(Transform root)
    {
        RootTransform = root;
    }

    /// <summary>Called by PlayerSpawner to assign which player slot this is.</summary>
    public void SetPlayerIndex(int index)
    {
        PlayerIndex = index;
    }

    // ─── XP Orb Collection ────────────────────────────────────────────────────
    // EXP is SHARED: ShareExp() broadcasts to all clients via RPC.
    // Only the player who touches the orb destroys it and triggers the EXP gain.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Exp1"))
        {
            // Destroy orb on all clients if online, locally if offline
            PhotonView orbView = collision.GetComponent<PhotonView>();
            if (orbView != null) PhotonNetwork.Destroy(collision.gameObject);
            else Destroy(collision.gameObject);

            ExperienceManager.ShareExp(100); // broadcasts to ALL clients
        }
        else if (collision.CompareTag("Exp2"))
        {
            PhotonView orbView = collision.GetComponent<PhotonView>();
            if (orbView != null) PhotonNetwork.Destroy(collision.gameObject);
            else Destroy(collision.gameObject);

            ExperienceManager.ShareExp(200);
        }
    }
}
