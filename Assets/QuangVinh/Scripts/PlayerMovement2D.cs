using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement Settings")]
    private float moveSpeed;

    [Header("Components")]
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerHealth playerHealth;

    private Vector2 moveInput;
     
    private void Awake()
    {
        // Lấy component trên chính object này hoặc trên parent (không dùng transform.parent để tránh null khi parent chưa có)
        rb = GetComponentInParent<Rigidbody2D>();
        animator = GetComponentInParent<Animator>();
        spriteRenderer = GetComponentInParent<SpriteRenderer>();

        // Tìm PlayerHealth: trên chính object này, parent, hoặc sibling (anh em cùng parent)
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
            playerHealth = GetComponentInParent<PlayerHealth>();
        if (playerHealth == null && transform.parent != null)
            playerHealth = transform.parent.GetComponentInChildren<PlayerHealth>();

        if (playerHealth == null)
            Debug.LogWarning("PlayerHealth not found! Movement won't stop when dead.");
    }

    private void Update()
    {
        // Không cho di chuyển khi đã chết
        if (playerHealth != null && playerHealth.IsDead)
            return;

        // Đọc input từ bàn phím
        GetInput();

        // Cập nhật animation dựa trên trạng thái di chuyển
        UpdateAnimation();

        // Lật sprite theo hướng di chuyển
        FlipSprite();

        moveSpeed = PlayerStats.Instance.moveSpeed;
    }

    private void FixedUpdate()
    {
        // Dừng hẳn khi đã chết
        if (playerHealth != null && playerHealth.IsDead)
        {
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
            return;
        }

        // Di chuyển nhân vật
        Move();
    }

    private void GetInput()
    {
        // Đọc input từ WASD hoặc phím mũi tên
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D hoặc Left/Right
        float vertical = Input.GetAxisRaw("Vertical");     // W/S hoặc Up/Down

        moveInput = new Vector2(horizontal, vertical).normalized;
    }

    private void Move()
    {
        if (rb == null) return;
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;
        bool isMoving = moveInput.magnitude > 0.1f;
        animator.SetBool("isRuning", isMoving);
    }

    private void FlipSprite()
    {
        if (spriteRenderer == null) return;
        if (moveInput.x > 0)
            spriteRenderer.flipX = false;
        else if (moveInput.x < 0)
            spriteRenderer.flipX = true;
    }
}
