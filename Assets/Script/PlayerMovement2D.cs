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
        // Lấy các component cần thiết
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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
        // Di chuyển nhân vật theo input
        rb.linearVelocity = moveInput * moveSpeed;
    }
    
    private void UpdateAnimation()
    {
        // Kiểm tra xem nhân vật có đang di chuyển không
        bool isMoving = moveInput.magnitude > 0.1f;
        
        // Set parameter "isRuning" trong Animator
        animator.SetBool("isRuning", isMoving);
    }
    
    private void FlipSprite()
    {
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
}
