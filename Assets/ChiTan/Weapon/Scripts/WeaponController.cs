using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Quản lý tất cả vũ khí của player
/// </summary>
public class WeaponController : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] private List<Weapon> weapons = new List<Weapon>();
    [SerializeField] private int maxWeapons = 6; // Số weapon tối đa
    
    private void Start()
    {
        // Lấy tất cả weapons con của player
        Weapon[] childWeapons = GetComponentsInChildren<Weapon>();
        weapons.AddRange(childWeapons);
        
        Debug.Log($"Player has {weapons.Count} weapons");
    }
    
    /// <summary>
    /// Thêm vũ khí mới khi level up
    /// </summary>
    public bool AddWeapon(GameObject weaponPrefab)
    {
        if (weaponPrefab == null)
        {
            Debug.LogError("Weapon prefab is null!");
            return false;
        }
        
        // Kiểm tra giới hạn
        if (weapons.Count >= maxWeapons)
        {
            Debug.LogWarning($"Max weapons reached ({maxWeapons})!");
            return false;
        }
        
        GameObject weaponObj = Instantiate(weaponPrefab, transform);
        Weapon weapon = weaponObj.GetComponent<Weapon>();
        
        if (weapon != null)
        {
            weapons.Add(weapon);
            Debug.Log($"✅ Added weapon: {weapon.WeaponName} (Lv.{weapon.WeaponLevel})");
            return true;
        }
        
        Debug.LogError("Weapon component not found on prefab!");
        return false;
    }
    
    /// <summary>
    /// Upgrade vũ khí theo tên
    /// </summary>
    public bool UpgradeWeapon(string weaponName)
    {
        Weapon weapon = GetWeapon(weaponName);
        
        if (weapon == null)
        {
            Debug.LogWarning($"Weapon '{weaponName}' not found!");
            return false;
        }
        
        if (!weapon.CanUpgrade)
        {
            Debug.LogWarning($"Weapon '{weaponName}' is already max level!");
            return false;
        }
        
        weapon.Upgrade();
        Debug.Log($"✨ Upgraded {weaponName} to Level {weapon.WeaponLevel}");
        return true;
    }
    
    /// <summary>
    /// Lấy weapon theo tên
    /// </summary>
    public Weapon GetWeapon(string weaponName)
    {
        return weapons.Find(w => w.WeaponName == weaponName);
    }
    
    /// <summary>
    /// Kiểm tra có weapon không
    /// </summary>
    public bool HasWeapon(string weaponName)
    {
        return GetWeapon(weaponName) != null;
    }
    
    /// <summary>
    /// Upgrade vũ khí theo index (deprecated, dùng UpgradeWeapon(string) thay thế)
    /// </summary>
    public void UpgradeWeapon(int weaponIndex)
    {
        if (weaponIndex >= 0 && weaponIndex < weapons.Count)
        {
            weapons[weaponIndex].Upgrade();
        }
    }
}
