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
<<<<<<< Updated upstream
    
=======
    private PlayerHealth playerHealth;

>>>>>>> Stashed changes
    private Vector2 moveInput;
    private bool inputEnabled = true;

    // ─── Lifecycle ────────────────────────────────────────────────────────────
    private void Awake()
    {
<<<<<<< Updated upstream
        // Lấy component trên chính object này hoặc trên parent (không dùng transform.parent để tránh null khi parent chưa có)
        rb = GetComponentInParent<Rigidbody2D>();
        animator = GetComponentInParent<Animator>();
        spriteRenderer = GetComponentInParent<SpriteRenderer>();
=======
        rb              = GetComponentInParent<Rigidbody2D>();
        animator        = GetComponentInParent<Animator>();
        spriteRenderer  = GetComponentInParent<SpriteRenderer>();

        // Find PlayerHealth on self, parent, or sibling
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
            playerHealth = GetComponentInParent<PlayerHealth>();
        if (playerHealth == null && transform.parent != null)
            playerHealth = transform.parent.GetComponentInChildren<PlayerHealth>();

        if (playerHealth == null)
            Debug.LogWarning("[PlayerMovement2D] PlayerHealth not found! Movement won't stop when dead.");
>>>>>>> Stashed changes
    }

    private void Update()
    {
<<<<<<< Updated upstream
        // Đọc input từ bàn phím
=======
        // Stop completely when dead or input locked
        if (!inputEnabled || (playerHealth != null && playerHealth.IsDead))
            return;

>>>>>>> Stashed changes
        GetInput();
        UpdateAnimation();
        FlipSprite();
    }

    private void FixedUpdate()
    {
<<<<<<< Updated upstream
        // Di chuyển nhân vật
=======
        if (!inputEnabled || (playerHealth != null && playerHealth.IsDead))
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

>>>>>>> Stashed changes
        Move();
    }

    // ─── Input ────────────────────────────────────────────────────────────────
    private void GetInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(h, v).normalized;
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
        if (moveInput.x > 0)       spriteRenderer.flipX = false;
        else if (moveInput.x < 0)  spriteRenderer.flipX = true;
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
