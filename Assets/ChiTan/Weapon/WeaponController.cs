using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Quản lý tất cả vũ khí của player
/// </summary>
public class WeaponController : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] private List<Weapon> weapons = new List<Weapon>();
    
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
    public void AddWeapon(GameObject weaponPrefab)
    {
        if (weaponPrefab == null) return;
        
        GameObject weaponObj = Instantiate(weaponPrefab, transform);
        Weapon weapon = weaponObj.GetComponent<Weapon>();
        
        if (weapon != null)
        {
            weapons.Add(weapon);
            Debug.Log($"Added weapon: {weaponObj.name}");
        }
    }
    
    /// <summary>
    /// Upgrade vũ khí hiện có
    /// </summary>
    public void UpgradeWeapon(int weaponIndex)
    {
        if (weaponIndex >= 0 && weaponIndex < weapons.Count)
        {
            // TODO: Implement upgrade logic
            Debug.Log($"Upgraded weapon {weaponIndex}");
        }
    }
}
