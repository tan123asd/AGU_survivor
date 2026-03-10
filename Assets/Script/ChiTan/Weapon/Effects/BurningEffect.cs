using UnityEngine;

/// <summary>
/// Hiệu ứng bỏng - Gây 1 damage mỗi giây trong 5 giây
/// </summary>
public class BurningEffect : MonoBehaviour
{
    [Header("Burning Settings")]
    [SerializeField] private int damagePerTick = 1; // Damage mỗi lần tick
    [SerializeField] private float tickInterval = 1f; // 1 giây 1 lần
    [SerializeField] private float duration = 5f; // Tổng thời gian bỏng

    private float timer = 0f;
    private float tickTimer = 0f;
    private IDamageable target;
    private bool isActive = false;

    // Visual effects (optional)
    [Header("Visual Effects (Optional)")]
    [SerializeField] private GameObject fireParticlePrefab;
    private GameObject currentParticle;

    private void Update()
    {
        if (!isActive) return;

        // Tăng timer
        timer += Time.deltaTime;
        tickTimer += Time.deltaTime;

        // Gây damage mỗi tickInterval
        if (tickTimer >= tickInterval)
        {
            tickTimer = 0f;
            ApplyBurnDamage();
        }

        // Hết thời gian → Destroy effect
        if (timer >= duration)
        {
            RemoveEffect();
        }
    }

    /// <summary>
    /// Khởi tạo burning effect
    /// </summary>
    public void Initialize(int damagePerSecond, float totalDuration)
    {
        damagePerTick = damagePerSecond;
        duration = totalDuration;
        tickInterval = 1f; // Mặc định 1 giây
        
        target = GetComponent<IDamageable>();
        if (target == null)
        {
            Debug.LogError("BurningEffect: Target doesn't implement IDamageable!");
            Destroy(this);
            return;
        }

        isActive = true;
        timer = 0f;
        tickTimer = 0f;

        Debug.Log($"🔥 Burning Effect applied! {damagePerTick} damage/sec for {duration}s");

        // Spawn particle effect nếu có
        if (fireParticlePrefab != null)
        {
            currentParticle = Instantiate(fireParticlePrefab, transform.position, Quaternion.identity, transform);
        }
    }

    private void ApplyBurnDamage()
    {
        if (target != null && isActive)
        {
            target.TakeDamage(damagePerTick);
            Debug.Log($"🔥 Burning damage: {damagePerTick}");
        }
    }

    private void RemoveEffect()
    {
        Debug.Log($"🔥 Burning effect ended!");
        
        // Destroy particle
        if (currentParticle != null)
        {
            Destroy(currentParticle);
        }

        // Destroy effect component
        Destroy(this);
    }

    /// <summary>
    /// Refresh burning effect nếu bị hit nhiều lần
    /// </summary>
    public void RefreshDuration()
    {
        timer = 0f; // Reset timer
        Debug.Log($"🔥 Burning effect refreshed!");
    }

    private void OnDestroy()
    {
        // Cleanup particle nếu còn
        if (currentParticle != null)
        {
            Destroy(currentParticle);
        }
    }
}
