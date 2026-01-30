using UnityEngine;

/// <summary>
/// Base class cho tất cả vũ khí
/// </summary>
public abstract class Weapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    [SerializeField] protected int damage = 10;
    [SerializeField] protected float attackInterval = 3f; // Bắn mỗi 3 giây
    [SerializeField] protected float range = 10f;
    
    [Header("Debug")]
    [SerializeField] protected bool debugMode = true;
    
    protected float attackTimer = 0f;
    protected Transform player;
    
    protected virtual void Start()
    {
        // Tìm player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (player == null && debugMode)
        {
            Debug.LogWarning("Weapon: Cannot find Player!");
        }
    }
    
    protected virtual void Update()
    {
        attackTimer += Time.deltaTime;
        
        if (attackTimer >= attackInterval)
        {
            Attack();
            attackTimer = 0f;
        }
    }
    
    /// <summary>
    /// Override method này trong subclass để implement cách tấn công
    /// </summary>
    protected abstract void Attack();
    
    /// <summary>
    /// Tìm enemy gần nhất trong range
    /// </summary>
    protected GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float nearestDistance = range;
        
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(player.position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = enemy;
            }
        }
        
        return nearest;
    }
}
