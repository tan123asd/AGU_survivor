using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

/// <summary>
/// Base class cho tất cả vũ khí
/// </summary>
public abstract class Weapon : MonoBehaviour
{
    [Header("Weapon Config (RECOMMENDED)")]
    [Tooltip("Kéo WeaponData ScriptableObject vào đây để dùng config tập trung")]
    [SerializeField] protected WeaponData weaponData;
    
    [Header("Weapon Identity (Fallback if no WeaponData)")]
    [SerializeField] protected string weaponName = "DefaultWeapon"; // Tên unique để identify weapon
    [SerializeField] protected int weaponLevel = 1; // Level hiện tại
    [SerializeField] protected int maxLevel = 5; // Level tối đa
    
    [Header("Weapon Stats (Fallback if no WeaponData)")]
    [SerializeField] protected int baseDamage = 10;
    [SerializeField] protected float baseAttackInterval = 3f; // Bắn mỗi 3 giây
    [SerializeField] protected float baseRange = 10f;
    
    // Stats sau khi upgrade (calculated)
    protected int damage;
    protected float attackInterval;
    protected float range;
    
    [Header("Debug")]
    [SerializeField] protected bool debugMode = true;

    [Header("Enemy Query")]
    [SerializeField] protected int enemyQueryBufferSize = 64;
    
    protected float attackTimer = 0f;
    protected Transform player;
    protected PlayerHealth playerHealth;
    protected PlayerEntity ownerPlayer;
    protected IEnemyTargetProvider enemyTargetProvider;
    private readonly List<GameObject> enemyQueryResults = new List<GameObject>(32);

    public static string NormalizeWeaponId(string rawId)
    {
        if (string.IsNullOrWhiteSpace(rawId)) return string.Empty;
        return rawId.Trim().ToLowerInvariant().Replace(" ", "_");
    }
    
    // Properties
    public string WeaponName => weaponData != null ? NormalizeWeaponId(weaponData.weaponId) : NormalizeWeaponId(weaponName);
    public int WeaponLevel => weaponLevel;
    public int MaxLevel => weaponData != null ? weaponData.maxLevel : maxLevel;
    public bool CanUpgrade => weaponLevel < MaxLevel;
    public WeaponData WeaponData => weaponData;

    protected virtual bool ShouldRunWeaponLogic()
    {
        // Multiplayer authority: only MasterClient drives enemy damage logic.
        // Offline/single-player: local instance runs normally.
        return !PhotonNetwork.IsConnected || PhotonNetwork.IsMasterClient;
    }

    protected virtual IEnemyTargetProvider CreateEnemyTargetProvider()
    {
        return new PhysicsEnemyTargetProvider(enemyQueryBufferSize);
    }

    protected virtual void EnsureEnemyTargetProvider()
    {
        if (enemyTargetProvider == null)
            enemyTargetProvider = CreateEnemyTargetProvider();
    }

    protected virtual void BindOwnerReferences()
    {
        ownerPlayer = GetComponentInParent<PlayerEntity>();
        if (ownerPlayer != null)
        {
            player = ownerPlayer.RootTransform != null ? ownerPlayer.RootTransform : ownerPlayer.transform;
            playerHealth = ownerPlayer.PlayerHealth;
            return;
        }

        if (PlayerController.Instance != null)
        {
            PlayerEntity localPlayer = PlayerController.Instance.GetLocalPlayer();
            if (localPlayer != null)
            {
                player = localPlayer.RootTransform != null ? localPlayer.RootTransform : localPlayer.transform;
                playerHealth = localPlayer.PlayerHealth;
                return;
            }
        }

        // Fallback for legacy scenes without PlayerController setup.
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null && debugMode)
        {
            Debug.LogWarning("Weapon: Cannot find Player!");
            return;
        }

        if (player != null)
        {
            Transform playerHealthTransform = player.Find("PlayerHealth");
            if (playerHealthTransform != null)
            {
                playerHealth = playerHealthTransform.GetComponent<PlayerHealth>();
                if (debugMode)
                {
                    Debug.Log($"Weapon {weaponName} (Lv.{weaponLevel}) found PlayerHealth successfully!");
                }
            }
            else if (debugMode)
            {
                Debug.LogWarning("Weapon: Cannot find PlayerHealth child GameObject!");
            }
        }
    }
    
    protected virtual void Start()
    {
        // Load values từ WeaponData nếu có
        if (weaponData != null)
        {
            weaponName = NormalizeWeaponId(weaponData.weaponId);
            maxLevel = weaponData.maxLevel;
            baseDamage = weaponData.baseDamage;
            baseAttackInterval = weaponData.baseAttackInterval;
            baseRange = weaponData.baseRange;
            
            if (debugMode)
            {
                Debug.Log($"✅ Weapon '{weaponData.displayName}' loaded from WeaponData");
            }
        }
        else
        {
            weaponName = NormalizeWeaponId(weaponName);
        }
        
        // Tính toán stats dựa trên base stats và level
        CalculateStats();

        BindOwnerReferences();
        EnsureEnemyTargetProvider();
    }
    
    /// <summary>
    /// Tính toán stats dựa trên level
    /// </summary>
    protected virtual void CalculateStats()
    {
        // Mỗi level tăng 10% damage, giảm 5% cooldown, tăng 10% range
        float levelMultiplier = 1 + (weaponLevel - 1) * 0.1f;
        float cooldownMultiplier = 1 - (weaponLevel - 1) * 0.05f;
        
        damage = Mathf.RoundToInt(baseDamage * levelMultiplier);
        attackInterval = baseAttackInterval * Mathf.Max(0.5f, cooldownMultiplier); // Min 50% cooldown
        range = baseRange * levelMultiplier;
        
        if (debugMode)
        {
            Debug.Log($"{weaponName} Lv.{weaponLevel}: Damage={damage}, Cooldown={attackInterval:F2}s, Range={range:F1}");
        }
    }
    
    /// <summary>
    /// Upgrade weapon lên level tiếp theo
    /// </summary>
    public virtual void Upgrade()
    {
        if (!CanUpgrade)
        {
            Debug.LogWarning($"{weaponName} is already max level ({maxLevel})!");
            return;
        }
        
        weaponLevel++;
        CalculateStats();
        
        // Hook để subclass thêm effect đặc biệt khi upgrade
        OnUpgrade();
        
        if (debugMode)
        {
            Debug.Log($"✨ {weaponName} upgraded to Level {weaponLevel}!");
        }
    }
    
    /// <summary>
    /// Override method này để thêm effect khi upgrade (ví dụ: tăng số projectiles)
    /// </summary>
    protected virtual void OnUpgrade()
    {
        // Subclass override để thêm logic đặc biệt
    }
    
    protected virtual void Update()
    {
        if (!ShouldRunWeaponLogic())
        {
            return;
        }

        if (player == null)
        {
            BindOwnerReferences();
            if (player == null) return;
        }

        // Không tấn công nếu Player đã chết
        if (playerHealth != null && playerHealth.IsDead)
        {
            return;
        }
        
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
        if (player == null) return null;

        EnsureEnemyTargetProvider();
        return enemyTargetProvider.FindNearestEnemy(player.position, range);
    }

    protected IReadOnlyList<GameObject> FindEnemiesInRange(float queryRange)
    {
        if (player == null)
        {
            enemyQueryResults.Clear();
            return enemyQueryResults;
        }

        EnsureEnemyTargetProvider();
        enemyTargetProvider.FindEnemiesInRange(player.position, queryRange, enemyQueryResults);
        return enemyQueryResults;
    }
}
