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

    private Transform ResolveOwnerRoot()
    {
        PlayerEntity owner = GetComponentInParent<PlayerEntity>();
        if (owner != null)
            return owner.RootTransform != null ? owner.RootTransform : owner.transform;

        if (PlayerController.Instance != null)
        {
            PlayerEntity localPlayer = PlayerController.Instance.GetLocalPlayer();
            if (localPlayer != null)
                return localPlayer.RootTransform != null ? localPlayer.RootTransform : localPlayer.transform;
        }

        return transform.root;
    }

    private void RefreshWeaponsFromOwner()
    {
        Transform ownerRoot = ResolveOwnerRoot();
        if (ownerRoot == null)
        {
            Debug.LogError("❌ Cannot resolve owner root for WeaponController!");
            return;
        }

        Debug.Log($"🔍 Looking for weapons in owner root: {ownerRoot.name}");
        Weapon[] allWeapons = ownerRoot.GetComponentsInChildren<Weapon>(true);

        Debug.Log($"🔍 GetComponentsInChildren found: {allWeapons.Length} weapons");

        foreach (var weapon in allWeapons)
        {
            if (!weapons.Contains(weapon))
            {
                weapons.Add(weapon);
                Debug.Log($"  📍 Found weapon on: {weapon.gameObject.name} (component: {weapon.GetType().Name})");
            }
        }
    }

    private void CompactWeaponList()
    {
        weapons.RemoveAll(w => w == null);
    }
    
    private void Start()
    {
        Debug.Log($"🔍 WeaponController.Start() on GameObject: {gameObject.name}");
        RefreshWeaponsFromOwner();
        
        Debug.Log($"🔫 WeaponController: Found {weapons.Count} weapons");
        foreach (var weapon in weapons)
        {
            Debug.Log($"  - {weapon.WeaponName} (Level {weapon.WeaponLevel}/{weapon.MaxLevel})");
        }
    }
    
    /// <summary>
    /// Thêm vũ khí mới khi level up
    /// </summary>
    public bool AddWeapon(GameObject weaponPrefab)
    {
        CompactWeaponList();

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
            if (HasWeapon(weapon.WeaponName))
            {
                Debug.LogWarning($"Weapon '{weapon.WeaponName}' already exists. Skipping duplicate add.");
                Destroy(weaponObj);
                return false;
            }

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
        string weaponId = Weapon.NormalizeWeaponId(weaponName);
        Weapon weapon = GetWeapon(weaponId);
        
        if (weapon == null)
        {
            Debug.LogWarning($"Weapon '{weaponId}' not found!");
            return false;
        }
        
        if (!weapon.CanUpgrade)
        {
            Debug.LogWarning($"Weapon '{weaponId}' is already max level!");
            return false;
        }
        
        weapon.Upgrade();
        Debug.Log($"✨ Upgraded {weaponId} to Level {weapon.WeaponLevel}");
        return true;
    }
    
    /// <summary>
    /// Lấy weapon theo tên
    /// </summary>
    public Weapon GetWeapon(string weaponName)
    {
        CompactWeaponList();
        string weaponId = Weapon.NormalizeWeaponId(weaponName);
        return weapons.Find(w => w != null && w.WeaponName == weaponId);
    }
    
    /// <summary>
    /// Kiểm tra có weapon không
    /// </summary>
    public bool HasWeapon(string weaponName)
    {
        return GetWeapon(weaponName) != null;
    }

    public bool IsWeaponAtMax(string weaponName)
    {
        Weapon weapon = GetWeapon(weaponName);
        return weapon != null && !weapon.CanUpgrade;
    }

    public bool RemoveWeapon(string weaponName)
    {
        CompactWeaponList();

        Weapon weapon = GetWeapon(weaponName);
        if (weapon == null) return false;

        weapons.Remove(weapon);
        Destroy(weapon.gameObject);
        return true;
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
