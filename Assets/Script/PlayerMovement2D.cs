using UnityEngine;

/// <summary>
/// Xử lý di chuyển của Player
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Components")]
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerInput playerInput;
    
    private Vector2 moveInput;
    private bool isMovementEnabled = true;
    
    private void Awake()
    {
        // Lấy các component cần thiết
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerInput = GetComponent<PlayerInput>();
    }
    
    private void Update()
    {
        if (!isMovementEnabled) return;
        
        // Đọc input từ PlayerInput hoặc trực tiếp
        GetInput();
        
        // Cập nhật animation dựa trên trạng thái di chuyển
        UpdateAnimation();
        
        // Lật sprite theo hướng di chuyển
        FlipSprite();
    }
    
    private void FixedUpdate()
    {
        if (!isMovementEnabled) 
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        // Di chuyển nhân vật
        Move();
    }
    
    private void GetInput()
    {
        // Ưu tiên dùng PlayerInput nếu có
        if (playerInput != null)
        {
            moveInput = playerInput.MoveInput;
        }
        else
        {
            // Fallback: đọc trực tiếp
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            moveInput = new Vector2(horizontal, vertical).normalized;
        }
    }
    
    private void Move()
    {
        // Di chuyển nhân vật theo input
        rb.linearVelocity = moveInput * moveSpeed;
    }
    
    private void UpdateAnimation()
    {
        if (animator == null) return;
        
        // Kiểm tra xem nhân vật có đang di chuyển không
        bool isMoving = moveInput.magnitude > 0.1f;
        
        // Set parameter "isRuning" trong Animator
        animator.SetBool("isRuning", isMoving);
    }
    
    private void FlipSprite()
    {
        if (spriteRenderer == null) return;
        
        // Lật sprite sang trái hoặc phải dựa vào hướng di chuyển
        if (moveInput.x > 0)
        {
            spriteRenderer.flipX = false; // Nhìn sang phải
        }
        else if (moveInput.x < 0)
        {
            spriteRenderer.flipX = true; // Nhìn sang trái
        }
    }
    
    #region Public Methods
    
    /// <summary>
    /// Enable/Disable movement
    /// </summary>
    public void SetMovementEnabled(bool enabled)
    {
        isMovementEnabled = enabled;
    }
    
    /// <summary>
    /// Thay đổi tốc độ di chuyển (cho power-ups)
    /// </summary>
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }
    
    public float GetMoveSpeed() => moveSpeed;
    
    #endregion
}
