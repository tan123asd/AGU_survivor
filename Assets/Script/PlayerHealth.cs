using UnityEngine;
using System;

/// <summary>
/// Quản lý máu của Player
/// </summary>
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    // Events cho UI subscribe
    public event Action<int, int> OnHealthChanged; // (current, max)
    public event Action OnPlayerDeath;
    
    private void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (debugMode)
        {
            Debug.Log($"Player Health initialized: {currentHealth}/{maxHealth}");
        }
    }
    
    private void Update()
    {
        // DEBUG: Bấm K để test damage
        if (debugMode && Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(10);
            Debug.Log("DEBUG: Pressed K - Take 10 damage");
        }
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); // Không âm
        
        if (debugMode)
        {
            Debug.Log($"Player took {damage} damage. HP: {currentHealth}/{maxHealth}");
        }
        
        // Fire event cho UI
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // Không vượt max
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (debugMode)
        {
            Debug.Log($"Player healed {amount}. HP: {currentHealth}/{maxHealth}");
        }
    }
    
    public void Die()
    {
        if (debugMode)
        {
            Debug.Log("Player Died!");
        }
        
        OnPlayerDeath?.Invoke();
        
        // Game Over
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        
        // TODO: Play death animation
        // TODO: Disable player controls
        gameObject.SetActive(false);
    }
    
    // Public getters
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
}
