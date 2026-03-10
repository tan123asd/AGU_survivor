using UnityEngine;

/// <summary>
/// Interface cho tất cả objects có thể nhận damage
/// </summary>
public interface IDamageable
{
    void TakeDamage(int damage);
    void Die();
}