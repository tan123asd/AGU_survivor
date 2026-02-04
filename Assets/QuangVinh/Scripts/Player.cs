using UnityEngine;

/// <summary>
/// Class chính của Player, gắn các component con (PlayerHealth, PlayerMovement2D) và implement IDamageable.
/// </summary>
public class Player : MonoBehaviour, IDamageable
{
    [Header("Player Components")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] public PlayerMovement2D playerMovement;
    [SerializeField] private ExperienceManager experienceManage;

    public PlayerHealth PlayerHealth => playerHealth;
    public PlayerMovement2D PlayerMovement => playerMovement;

    private void Awake()
    {
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement2D>();
        if (experienceManage == null)
            experienceManage = GetComponent<ExperienceManager>();
    }

    private void Update()
    {
        // Test: nhấn Space để nhận damage (có thể xóa khi không cần)
        if (Input.GetKeyDown(KeyCode.Space) && playerHealth != null)
            TakeDamage(20);
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
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Exp1"))
        {
            Destroy(collision.gameObject);
            experienceManage.AddExp(100);
        }
        else if (collision.CompareTag("Exp2"))
        {
            Destroy(collision.gameObject);
            experienceManage.AddExp(200);
        }
    }
}