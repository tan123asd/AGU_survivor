using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Components")]
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    
    private Vector2 moveInput;
    
    private void Awake()
    {
        // Lấy component trên chính object này hoặc trên parent (không dùng transform.parent để tránh null khi parent chưa có)
        rb = GetComponentInParent<Rigidbody2D>();
        animator = GetComponentInParent<Animator>();
        spriteRenderer = GetComponentInParent<SpriteRenderer>();
    }
    
    private void Update()
    {
        // Đọc input từ bàn phím
        GetInput();
        
        // Cập nhật animation dựa trên trạng thái di chuyển
        UpdateAnimation();
        
        // Lật sprite theo hướng di chuyển
        FlipSprite();
    }
    
    private void FixedUpdate()
    {
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
