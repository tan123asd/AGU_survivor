using Photon.Pun;
using UnityEngine;

/// <summary>
/// Root component for a Player GameObject.
/// - Holds references to PlayerHealth and PlayerMovement2D.
/// - Registers / unregisters with PlayerController (singleton).
/// - Supports a PlayerIndex for multiplayer input routing.
/// - Exposes RootTransform: always the top-level GameObject transform.
/// </summary>
public class PlayerEntity : MonoBehaviour
{
    [Header("Player Components")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] public PlayerMovement2D playerMovement;

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
    /// </summary>
    public Transform RootTransform { get; private set; }

    // ─── Lifecycle ────────────────────────────────────────────────────────────
    private void Awake()
    {
        RootTransform = transform;
        _photonView = GetComponentInParent<PhotonView>();

        if (playerHealth == null)
            playerHealth = GetComponentInChildren<PlayerHealth>() ?? GetComponent<PlayerHealth>();

        if (playerMovement == null)
            playerMovement = GetComponentInChildren<PlayerMovement2D>() ?? GetComponent<PlayerMovement2D>();

        if (PlayerController.Instance != null)
            PlayerController.Instance.RegisterPlayer(this);
        else
            Debug.LogWarning("[PlayerEntity] PlayerController.Instance is null.");
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance != null)
            PlayerController.Instance.UnregisterPlayer(this);
    }

    // ─── Public API ───────────────────────────────────────────────────────────
    public void SetRootTransform(Transform root)
    {
        RootTransform = root;
    }

    public void SetPlayerIndex(int index)
    {
        PlayerIndex = index;
    }

    // ─── XP Orb Collection ────────────────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool isExp = false;
        int expAmount = 0;

        // Cách 1: Kiểm tra bằng tag
        if (collision.CompareTag("Exp1"))
        {
            isExp = true;
            expAmount = 100;
        }
        else if (collision.CompareTag("Exp2"))
        {
            isExp = true;
            expAmount = 200;
        }
        // Cách 2: Kiểm tra bằng tên GameObject (dự phòng)
        else if (collision.gameObject.name.Contains("Exp"))
        {
            isExp = true;
            if (collision.gameObject.name.Contains("Blue"))
                expAmount = 100;
            else if (collision.gameObject.name.Contains("RED"))
                expAmount = 200;
            else
                expAmount = 100;
        }

        if (!isExp) return;

        // ────────────────────────────────────────────────────────────────
        // PHẦN QUAN TRỌNG: XỬ LÝ THEO VAI TRÒ MASTER / CLIENT
        // ────────────────────────────────────────────────────────────────

        // Bước 1: Mọi client (local player) đều có thể thu thập EXP
        // (chỉ cần chạm là nhận EXP, không cần chờ destroy)
        ExperienceManager.ShareExp(expAmount);

        // Bước 2: Chỉ Master Client mới destroy orb (để đồng bộ và tránh lỗi ownership)
        if (!PhotonNetwork.IsMasterClient)
        {
            // Client thường chỉ thu thập EXP, không destroy
            // Orb sẽ được Master destroy → mọi người thấy biến mất
            return;
        }

        // Từ đây chỉ chạy trên Master Client

        PhotonView orbView = collision.GetComponent<PhotonView>();

        if (orbView != null)
        {
            // Master luôn có quyền destroy mọi network object
            PhotonNetwork.Destroy(collision.gameObject);
            Debug.Log($"[Master] Destroyed EXP orb (ViewID: {orbView.ViewID})");
        }
        else
        {
            // Trường hợp orb là object local (không network) – hiếm gặp
            Destroy(collision.gameObject);
            Debug.Log("[Master] Local Destroy EXP orb (no PhotonView)");
        }
    }
}