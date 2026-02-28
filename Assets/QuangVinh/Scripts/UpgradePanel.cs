using System.Collections.Generic;
using UnityEngine;

public class UpgradePanel : MonoBehaviour
{
    public static UpgradePanel instance { get; private set; }
    [SerializeField] private UpgradeButtonChoice[] upgradeButtonChoicePrefab;

    private List<UpgradeButtonChoice> currentButtons = new List<UpgradeButtonChoice>();

    private void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }

    public void OpenUpgradePanel(UpgradeData[] upgradeDatas)
    {
        Time.timeScale = 0f;
        gameObject.SetActive(true);

        foreach (var button in currentButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }
        currentButtons.Clear();
        for (int i = 0; i < upgradeDatas.Length && i < upgradeButtonChoicePrefab.Length; i++)
        {
            var prefab = Instantiate(upgradeButtonChoicePrefab[i], transform);
            prefab.Setup(upgradeDatas[i]);
            currentButtons.Add(prefab);
        }
    }

    public void SelectUpgrade(UpgradeData upgradeData)
    {
        // Kiểm tra loại upgrade
        if (upgradeData.upgradeType == UpgradeData.UpgradeType.Weapon)
        {
            // Weapon upgrade
            if (upgradeData.weaponMode == UpgradeData.WeaponUpgradeMode.Add)
            {
                // Thêm weapon mới (lấy từ WeaponData hoặc manual config)
                AddWeaponToPlayer(upgradeData.GetWeaponPrefab());
            }
            else if (upgradeData.weaponMode == UpgradeData.WeaponUpgradeMode.Upgrade)
            {
                // Upgrade weapon hiện có (lấy từ WeaponData hoặc manual config)
                UpgradePlayerWeapon(upgradeData.GetTargetWeaponName());
            }
        }
        else if (upgradeData.upgradeType == UpgradeData.UpgradeType.Stat)
        {
            // Stat upgrade
            var modifiers = upgradeData.GetStatModifiers();
            for (int i = 0; i < modifiers.Length; i++)
            {
                var effect = upgradeData.buffEffects[i];
                var modifier = modifiers[i];
                PlayerStats.Instance.AddModifier(effect.effectName, modifier);
            }
        }

        Time.timeScale = 1f;
        gameObject.SetActive(false);

        foreach (var btn in currentButtons)
        {
            if (btn != null) Destroy(btn.gameObject);
        }
        currentButtons.Clear();
    }
    
    /// <summary>
    /// Thêm vũ khí mới vào player
    /// </summary>
    private void AddWeaponToPlayer(GameObject weaponPrefab)
    {
        if (weaponPrefab == null)
        {
            Debug.LogError("Weapon prefab is null!");
            return;
        }
        
        // Tìm player
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found!");
            return;
        }
        
        // Tìm WeaponController trên player
        WeaponController weaponController = player.GetComponentInChildren<WeaponController>();
        
        if (weaponController != null)
        {
            // Dùng WeaponController
            bool success = weaponController.AddWeapon(weaponPrefab);
            if (success)
            {
                Debug.Log($"✅ Added weapon via WeaponController: {weaponPrefab.name}");
            }
        }
        else
        {
            // Fallback: Instantiate trực tiếp
            GameObject weaponObj = Instantiate(weaponPrefab, player.transform);
            Debug.Log($"⚠️ Added weapon directly to Player (no WeaponController): {weaponObj.name}");
        }
    }
    
    /// <summary>
    /// Upgrade vũ khí hiện có của player
    /// </summary>
    private void UpgradePlayerWeapon(string weaponName)
    {
        if (string.IsNullOrEmpty(weaponName))
        {
            Debug.LogError("Weapon name is empty!");
            return;
        }
        
        // Tìm player
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found!");
            return;
        }
        
        // Tìm WeaponController
        WeaponController weaponController = player.GetComponentInChildren<WeaponController>();
        
        if (weaponController == null)
        {
            Debug.LogError("WeaponController not found! Cannot upgrade weapon.");
            return;
        }
        
        // Upgrade weapon
        bool success = weaponController.UpgradeWeapon(weaponName);
        if (success)
        {
            Debug.Log($"✨ Upgraded weapon: {weaponName}");
        }
        else
        {
            Debug.LogWarning($"Failed to upgrade weapon: {weaponName}");
        }
    }
}
