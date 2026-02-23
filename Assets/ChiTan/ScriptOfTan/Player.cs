using UnityEngine;

/// <summary>
/// Class chính của Player, gắn các component con (PlayerHealth, PlayerMovement2D) và implement IDamageable.
/// </summary>
public class Player : MonoBehaviour, IDamageable
{
    [Header("Player Components")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] public PlayerMovement2D playerMovement;

    public PlayerHealth PlayerHealth => playerHealth;
    public PlayerMovement2D PlayerMovement => playerMovement;

    private void Awake()
    {
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement2D>();
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
}
