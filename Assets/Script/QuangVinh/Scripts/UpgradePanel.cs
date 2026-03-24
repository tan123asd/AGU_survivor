using System.Collections.Generic;
using UnityEngine;

public class UpgradePanel : MonoBehaviour
{
    public static UpgradePanel instance { get; private set; }
    [SerializeField] private UpgradeButtonChoice[] upgradeButtonChoicePrefab;

    private List<UpgradeButtonChoice> currentButtons = new List<UpgradeButtonChoice>();

    private WeaponController ResolveLocalWeaponController()
    {
        if (PlayerController.Instance == null) return null;

        PlayerEntity localPlayer = PlayerController.Instance.GetLocalPlayer();
        if (localPlayer == null) return null;

        Transform root = localPlayer.RootTransform != null ? localPlayer.RootTransform : localPlayer.transform;
        WeaponController wc = root.GetComponentInChildren<WeaponController>(true);
        if (wc == null)
            wc = localPlayer.GetComponentInChildren<WeaponController>(true);

        return wc;
    }

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
            else if (upgradeData.weaponMode == UpgradeData.WeaponUpgradeMode.Fusion)
            {
                ApplyFusionUpgrade(upgradeData);
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

        WeaponController weaponController = ResolveLocalWeaponController();
        if (weaponController == null)
        {
            Debug.LogError("Local WeaponController not found! Cannot add weapon.");
            return;
        }

        // Dùng WeaponController
        bool success = weaponController.AddWeapon(weaponPrefab);
        if (success)
        {
            Debug.Log($"✅ Added weapon via WeaponController: {weaponPrefab.name}");
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
        
        // Tìm WeaponController local player
        WeaponController weaponController = ResolveLocalWeaponController();
        
        if (weaponController == null)
        {
            Debug.LogError("Local WeaponController not found! Cannot upgrade weapon.");
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

    private void ApplyFusionUpgrade(UpgradeData upgradeData)
    {
        WeaponController weaponController = ResolveLocalWeaponController();
        if (weaponController == null)
        {
            Debug.LogError("Local WeaponController not found! Cannot apply fusion upgrade.");
            return;
        }

        if (!upgradeData.CanApplyFusion(weaponController))
        {
            Debug.LogWarning($"Fusion requirements are not met: {upgradeData.GetDisplayName()}");
            return;
        }

        string sourceA = upgradeData.GetFusionSourceWeaponA();
        string sourceB = upgradeData.GetFusionSourceWeaponB();

        bool removedA = weaponController.RemoveWeapon(sourceA);
        bool removedB = weaponController.RemoveWeapon(sourceB);
        if (!removedA || !removedB)
        {
            Debug.LogWarning($"Fusion failed while removing source weapons: {sourceA}, {sourceB}");
            return;
        }

        GameObject fusedPrefab = upgradeData.GetWeaponPrefab();
        if (fusedPrefab == null)
        {
            Debug.LogError("Fusion weapon prefab is null!");
            return;
        }

        bool addSuccess = weaponController.AddWeapon(fusedPrefab);
        if (!addSuccess)
        {
            Debug.LogError($"Failed to add fused weapon prefab: {fusedPrefab.name}");
            return;
        }

        Debug.Log($"🧬 Fusion success: {sourceA} + {sourceB} -> {upgradeData.GetTargetWeaponName()}");
    }
}
