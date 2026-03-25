using Photon.Pun;
using UnityEngine;

/// <summary>
/// Handles 2D movement for one player.
/// Reads WASD / Arrow Keys (Player 0).
/// SetInputEnabled() allows PlayerController to globally lock movement (e.g. game-over).
/// </summary>
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Components")]
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerHealth playerHealth;

    private Vector2 moveInput;
    private bool inputEnabled = true;

    // ─── Photon ───────────────────────────────────────────────────────────────
    // Null when offline (single-player). Non-null in multiplayer.
    private PhotonView _photonView;

    [Header("Visual Options")]
    [Tooltip("Invert the flip logic for this prefab. Use when animation faces opposite direction on default.")]
    [SerializeField] private bool invertFlip = false;

    // Horizontal raw input (un-normalized) used for stable flip decisions
    private float lastRawHorizontal = 0f;
    // Last non-zero horizontal direction: 1 = facing right, -1 = facing left
    private int lastFacingSign = 1;
    // Deadzone to ignore small input noise
    private const float horizontalDeadzone = 0.15f;

    // ─── Lifecycle ────────────────────────────────────────────────────────────
    private void Awake()
    {
        // Lấy component trên chính object này hoặc trên parent
        // (không dùng transform.parent để tránh null khi parent chưa có)
        rb             = GetComponentInParent<Rigidbody2D>();
        animator       = GetComponentInParent<Animator>();
        spriteRenderer = GetComponentInParent<SpriteRenderer>();

        // Find PlayerHealth on self, parent, or sibling
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
            playerHealth = GetComponentInParent<PlayerHealth>();
        if (playerHealth == null && transform.parent != null)
            playerHealth = transform.parent.GetComponentInChildren<PlayerHealth>();

        if (playerHealth == null)
            Debug.LogWarning("[PlayerMovement2D] PlayerHealth not found! Movement won't stop when dead.");

        // PhotonView lives on the root of the player prefab.
        // GetComponentInParent also checks this object itself.
        _photonView = GetComponentInParent<PhotonView>();
    }

    private void Update()
    {
        // In multiplayer: only the owning client drives input.
        // Remote players are moved by PhotonTransformView automatically.
        if (_photonView != null && !_photonView.IsMine) return;

        // Stop completely when dead or input locked
        if (!inputEnabled || (playerHealth != null && playerHealth.IsDead))
            return;

        // Đọc input từ bàn phím
        GetInput();

        // Cập nhật animation dựa trên trạng thái di chuyển
        UpdateAnimation();

        // Lật sprite theo hướng di chuyển
        FlipSprite();
    }

    private void FixedUpdate()
    {
        // Remote player: physics driven by PhotonTransformView, do nothing here.
        if (_photonView != null && !_photonView.IsMine) return;

        if (!inputEnabled || (playerHealth != null && playerHealth.IsDead))
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        // Di chuyển nhân vật
        Move();
    }

    // ─── Input ────────────────────────────────────────────────────────────────
    private void GetInput()
    {
        // Đọc input từ WASD hoặc phím mũi tên
        float h = Input.GetAxisRaw("Horizontal"); // A/D hoặc Left/Right
        float v = Input.GetAxisRaw("Vertical");   // W/S hoặc Up/Down
        moveInput = new Vector2(h, v).normalized;

        // Store raw horizontal for flip decision and remember last non-zero direction
        lastRawHorizontal = h;
        if (Mathf.Abs(h) > horizontalDeadzone)
            lastFacingSign = h > 0f ? 1 : -1;
    }

    // ─── Movement ─────────────────────────────────────────────────────────────
    private void Move()
    {
        if (rb == null) return;
        rb.linearVelocity = moveInput * moveSpeed;
    }

    // ─── Animation & Visuals ──────────────────────────────────────────────────
    private void UpdateAnimation()
    {
        if (animator == null) return;
        animator.SetBool("isRuning", moveInput.magnitude > 0.1f);
    }

    private void FlipSprite()
    {
        if (spriteRenderer == null) return;

        // Use raw horizontal input to decide flip; ignore small noise via deadzone
        if (Mathf.Abs(lastRawHorizontal) > horizontalDeadzone)
        {
            bool flip = lastRawHorizontal < 0f;
            if (invertFlip) flip = !flip;
            spriteRenderer.flipX = flip;
        }
        else
        {
            // No strong horizontal input — keep last facing direction
            bool flip = lastFacingSign < 0;
            if (invertFlip) flip = !flip;
            spriteRenderer.flipX = flip;
        }
    }

    // ─── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Enable or disable player input (called by PlayerController).
    /// Use this to freeze movement during cutscenes or game-over.
    /// </summary>
    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
        if (!enabled && rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    /// <summary>Current move speed (read-only).</summary>
    public float MoveSpeed => moveSpeed;
}
