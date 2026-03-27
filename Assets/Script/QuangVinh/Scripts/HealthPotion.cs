using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    [Header("Heal Settings")]
    [SerializeField] private int healAmount = 25;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
        if (playerHealth == null)
            playerHealth = collision.GetComponentInChildren<PlayerHealth>();
        if (playerHealth == null)
            playerHealth = collision.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null || playerHealth.IsDead) return;

        // Don't heal if already at full health
        if (playerHealth.CurrentHealth >= playerHealth.MaxHealth) return;

        playerHealth.Heal(healAmount);
        Destroy(gameObject);
    }
}
