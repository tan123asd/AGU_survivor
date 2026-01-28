using UnityEngine;

/// <summary>
/// Quản lý máu của Enemy
/// </summary>
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 30;
    private int currentHealth;
    
    [Header("Drops")]
    [SerializeField] private GameObject expGemPrefab; // Assign trong Inspector
    [SerializeField] private int expAmount = 5;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private void Start()
    {
        currentHealth = maxHealth;
        
        if (debugMode)
        {
            Debug.Log($"Enemy spawned with {currentHealth} HP");
        }
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        if (debugMode)
        {
            Debug.Log($"Enemy took {damage} damage. HP: {currentHealth}/{maxHealth}");
        }
        
        // TODO: Play hurt animation/effect
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Die()
    {
        if (debugMode)
        {
            Debug.Log("Enemy died!");
        }
        
        // Drop exp gem
        if (expGemPrefab != null)
        {
            Instantiate(expGemPrefab, transform.position, Quaternion.identity);
        }
        else if (debugMode)
        {
            Debug.Log($"Would drop {expAmount} exp here (no prefab assigned)");
        }
        
        // TODO: Play death animation
        // TODO: Spawn death VFX
        
        Destroy(gameObject);
    }
}
