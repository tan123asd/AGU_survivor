using UnityEngine;

/// <summary>
/// Viên đạn bay và gây damage khi chạm enemy
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifetime = 5f; // Tự hủy sau 5s nếu không chạm gì
    [SerializeField] private float zPosition = -0.5f; // Z position để hiển thị giữa player và background
    
    private Vector2 direction;
    private Rigidbody2D rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0; // Không chịu trọng lực
        }
    }
    
    private void Start()
    {
        // Đảm bảo z position đúng để hiển thị trên layer phù hợp
        Vector3 pos = transform.position;
        pos.z = zPosition;
        transform.position = pos;
        
        // Tự hủy sau lifetime
        Destroy(gameObject, lifetime);
    }
    
    private void FixedUpdate()
    {
        // Bay theo hướng
        rb.linearVelocity = direction * speed;
    }
    
    /// <summary>
    /// Set hướng bay và damage
    /// </summary>
    public void Initialize(Vector2 dir, int dmg)
    {
        direction = dir.normalized;
        damage = dmg;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem có phải enemy không
        if (other.CompareTag("Enemy"))
        {
            // Gây damage
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Debug.Log($"Projectile hit enemy for {damage} damage!");
            }
            
            // Hủy viên đạn
            Destroy(gameObject);
        }
    }
}
