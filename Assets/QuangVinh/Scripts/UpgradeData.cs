using UnityEngine;

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Upgrades/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("Upgrade Type")]
    public UpgradeType upgradeType = UpgradeType.Stat; // Loại upgrade
    
    [Header("Basic Info (Auto-filled if using WeaponData)")]
    public string displayName;
    [TextArea(2, 3)]
    public string description;
    public Sprite icon;
    
    [Header("Stat Buffs (for Stat type)")]
    public BuffEffect[] buffEffects;
    
    [Header("Weapon Config (RECOMMENDED - Use WeaponData)")]
    [Tooltip("Kéo WeaponData vào đây, tự động load thông tin")]
    public WeaponData weaponData;
    
    [Header("Weapon (Fallback - Manual Config)")]
    public WeaponUpgradeMode weaponMode = WeaponUpgradeMode.Add; // Add mới hay Upgrade hiện có
    [Tooltip("CHỈ CẦN nếu mode = Upgrade và KHÔNG dùng WeaponData")]
    public string targetWeaponName = ""; // Tên weapon cần upgrade (nếu mode = Upgrade)
    [Tooltip("CHỈ CẦN nếu mode = Add và KHÔNG dùng WeaponData")]
    public GameObject weaponPrefab; // Prefab vũ khí để thêm vào player (nếu mode = Add)
    
    [System.Serializable]
    public struct BuffEffect
    {
        public string effectName;
        public float effectValue;
        public bool isPercentage;
    }
    
    public enum UpgradeType
    {
        Stat,    // Upgrade stat (speed, damage, health...)
        Weapon   // Thêm vũ khí mới hoặc upgrade weapon hiện có
    }
    
    public enum WeaponUpgradeMode
    {
        Add,     // Thêm weapon mới
        Upgrade  // Upgrade weapon hiện có
    }
    
    public IStatModifier[] GetStatModifiers()
    {
        IStatModifier[] mods = new IStatModifier[buffEffects.Length];
        for (int i = 0; i < buffEffects.Length; i++)
        {
            var effect = buffEffects[i];
            if (effect.isPercentage)
            {
                mods[i] = new PercentageMultiplyModifier { Multiplier = 1 + effect.effectValue };
            }
            else
            {
                mods[i] = new FlatAddModifier { Amount = effect.effectValue };
            }
        }

        return mods;
    }
    
    /// <summary>
    /// Get weapon prefab (từ WeaponData hoặc manual)
    /// </summary>
    public GameObject GetWeaponPrefab()
    {
        if (weaponData != null)
            return weaponData.weaponPrefab;
        return weaponPrefab;
    }
    
    /// <summary>
    /// Get target weapon name (từ WeaponData hoặc manual)
    /// </summary>
    public string GetTargetWeaponName()
    {
        if (weaponData != null)
            return weaponData.weaponId;
        return targetWeaponName;
    }
    
    /// <summary>
    /// Get display name (từ WeaponData hoặc manual)
    /// </summary>
    public string GetDisplayName(int currentLevel = 1)
    {
        if (weaponData != null)
        {
            // Nếu là upgrade mode, thêm level vào tên
            if (weaponMode == WeaponUpgradeMode.Upgrade)
            {
                string[] romans = { "", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };
                int nextLevel = currentLevel + 1;
                if (nextLevel <= romans.Length)
                    return $"{weaponData.displayName} {romans[nextLevel - 1]}";
            }
            return weaponData.displayName;
        }
        return displayName;
    }
    
    /// <summary>
    /// Get description (từ WeaponData hoặc manual)
    /// </summary>
    public string GetDescription()
    {
        if (weaponData != null)
            return weaponData.description;
        return description;
    }
    
    /// <summary>
    /// Get icon (từ WeaponData hoặc manual)
    /// </summary>
    public Sprite GetIcon()
    {
        if (weaponData != null && weaponData.icon != null)
            return weaponData.icon;
        return icon;
    }
    
    /// <summary>
    /// Auto-fill info từ WeaponData khi validate
    /// </summary>
    private void OnValidate()
    {
        if (weaponData != null && upgradeType == UpgradeType.Weapon)
        {
            // Auto fill basic info từ WeaponData
            if (string.IsNullOrEmpty(displayName))
                displayName = weaponData.displayName;
            
            if (string.IsNullOrEmpty(description))
                description = weaponData.description;
            
            if (icon == null && weaponData.icon != null)
                icon = weaponData.icon;
            
            // Auto fill weapon prefab/name dựa vào mode
            if (weaponMode == WeaponUpgradeMode.Add)
            {
                weaponPrefab = weaponData.weaponPrefab;
            }
            else if (weaponMode == WeaponUpgradeMode.Upgrade)
            {
                targetWeaponName = weaponData.weaponId;
            }
        }
    }
}
