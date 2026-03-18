using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceManager : MonoBehaviour
{
    // ─── Singleton ──────────────────────────────────
    // Scene-level singleton: one ExperienceManager is shared by ALL players.
    public static ExperienceManager Instance { get; private set; }

    // ─── Photon ──────────────────────────────────
    // Requires a PhotonView on the 'Exp bar' GameObject in the Unity Editor.
    private PhotonView _photonView;
    [SerializeField] private AnimationCurve expCurve;
    [SerializeField] private UpgradeData[] upgradeDatas;

    private int currentLevel;
    private int currentExp;
    private int previousLevelExp;
    private int nextLevelExp;

    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Slider expSlider;
    [SerializeField] private ParticleSystem levelUpEffect;

    private void Awake()
    {
        Instance = this;
        _photonView = GetComponent<PhotonView>();
    }

    // ─── Public API ──────────────────────────────────

    /// <summary>
    /// Call this from game code (e.g. PlayerEntity) instead of AddExpRPC directly.
    /// In multiplayer, broadcasts to ALL clients so EXP is shared.
    /// In single-player (no PhotonView), calls AddExpRPC locally.
    /// </summary>
    public static void ShareExp(int amount)
    {
        if (Instance == null)
        {
            Debug.LogWarning("[ExperienceManager] No Instance found in scene.");
            return;
        }

        if (Instance._photonView != null)
            Instance._photonView.RPC(nameof(AddExpRPC), RpcTarget.All, amount);
        else
            Instance.AddExpRPC(amount); // offline fallback
    }

    /// <summary>
    /// Adds EXP locally. Called via RPC in multiplayer, or directly offline.
    /// Do NOT call this directly from gameplay code — use ShareExp() instead.
    /// </summary>
    [PunRPC]
    public void AddExpRPC(int exp)
    {
        currentExp += exp;
        CheckForLevelUp();
        UpdateInterface();
    }

    private void CheckForLevelUp()
    {
        if (currentExp >= nextLevelExp)
        {
            currentLevel++;
            UpdateLevel();

            // Get the local player via PlayerController (multiplayer-safe)
            PlayerEntity localPlayer = PlayerController.Instance != null
                ? PlayerController.Instance.GetLocalPlayer()
                : null;

            if (levelUpEffect != null && localPlayer != null)
            {
                Vector3 spawnPos = localPlayer.RootTransform != null
                    ? localPlayer.RootTransform.position + new Vector3(0, -0.5f, 0)
                    : localPlayer.transform.position + new Vector3(0, -0.5f, 0);

                ParticleSystem psInstance = Instantiate(levelUpEffect, spawnPos, Quaternion.identity);
                psInstance.transform.SetParent(localPlayer.transform);

                // CoroutineRunner: runs on the particle's own active GameObject.
                // WaitForSecondsRealtime works even when timeScale = 0 (upgrade panel).
                float duration = psInstance.main.duration;
                CoroutineRunner.RunDelayed(psInstance.gameObject, duration, () =>
                {
                    if (psInstance != null)
                    {
                        psInstance.transform.SetParent(null);
                        Destroy(psInstance.gameObject, psInstance.main.duration);
                    }
                });
            }
        }
    }



    private void UpdateLevel()
    {
        previousLevelExp = (int)expCurve.Evaluate(currentLevel);
        nextLevelExp = (int)expCurve.Evaluate(currentLevel + 1);
        UpdateInterface();

        UpgradeData[] upgradeDatas = GetRandomUpgrades(3);
        UpgradePanel.instance.OpenUpgradePanel(upgradeDatas);
    }

    private UpgradeData[] GetRandomUpgrades(int count)
    {
        List<UpgradeData> availableUpgrades = new List<UpgradeData>(upgradeDatas);
        List<UpgradeData> selectedUpgrades = new List<UpgradeData>();
        
        // Tìm WeaponController để kiểm tra weapon đã có
        WeaponController weaponController = GetComponent<WeaponController>();
        if (weaponController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Tìm trong Player và con của nó
                weaponController = player.GetComponentInChildren<WeaponController>();
            }
        }
        
        // Fallback: Tìm trong toàn scene
        if (weaponController == null)
        {
            weaponController = FindFirstObjectByType<WeaponController>();
            Debug.LogWarning($"⚠️ WeaponController not on Player! Found at: {(weaponController != null ? weaponController.gameObject.name : "NULL")}");
        }
        
        // Lọc upgrades hợp lệ
        List<UpgradeData> validUpgrades = new List<UpgradeData>();
        foreach (var upgrade in availableUpgrades)
        {
            // Stat upgrade luôn valid
            if (upgrade.upgradeType == UpgradeData.UpgradeType.Stat)
            {
                validUpgrades.Add(upgrade);
                continue;
            }
            
            // Weapon upgrade
            if (upgrade.upgradeType == UpgradeData.UpgradeType.Weapon)
            {
                if (upgrade.weaponMode == UpgradeData.WeaponUpgradeMode.Add)
                {
                    // Add mode: valid nếu chưa có weapon này
                    GameObject weaponPrefab = upgrade.GetWeaponPrefab();
                    if (weaponPrefab == null) continue; // Skip if no prefab
                    
                    Weapon weaponComponent = weaponPrefab.GetComponent<Weapon>();
                    if (weaponComponent == null) continue; // Skip if no Weapon component
                    
                    if (weaponController == null || !weaponController.HasWeapon(weaponComponent.WeaponName))
                    {
                        validUpgrades.Add(upgrade);
                    }
                }
                else if (upgrade.weaponMode == UpgradeData.WeaponUpgradeMode.Upgrade)
                {
                    // Upgrade mode: valid nếu ĐÃ có weapon và chưa max level
                    if (weaponController != null)
                    {
                        string targetWeaponName = upgrade.GetTargetWeaponName();
                        Weapon weapon = weaponController.GetWeapon(targetWeaponName);
                        
                        Debug.Log($"🔍 Checking upgrade '{upgrade.displayName}' for weapon '{targetWeaponName}':");
                        Debug.Log($"   - Weapon found: {weapon != null}");
                        if (weapon != null)
                        {
                            Debug.Log($"   - Can upgrade: {weapon.CanUpgrade} (Level {weapon.WeaponLevel}/{weapon.MaxLevel})");
                        }
                        
                        if (weapon != null && weapon.CanUpgrade)
                        {
                            validUpgrades.Add(upgrade);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ WeaponController is NULL! Cannot check weapon upgrade.");
                    }
                }
            }
        }
        
        // Debug log
        Debug.Log($"📋 Valid upgrades: {validUpgrades.Count}/{availableUpgrades.Count}");
        foreach (var upgrade in validUpgrades)
        {
            Debug.Log($"  ✅ {upgrade.GetDisplayName()}");
        }
        
        // Nếu không có valid upgrades, fallback về tất cả upgrades
        if (validUpgrades.Count == 0)
        {
            Debug.LogWarning("⚠️ No valid upgrades! Using all upgrades as fallback.");
            validUpgrades = availableUpgrades;
        }

        // Random chọn, ưu tiên luôn có ít nhất 1 Stat nếu có trong danh sách hợp lệ
        List<UpgradeData> statUpgrades = validUpgrades.FindAll(u => u.upgradeType == UpgradeData.UpgradeType.Stat);
        if (count > 0 && statUpgrades.Count > 0)
        {
            int statIndex = Random.Range(0, statUpgrades.Count);
            UpgradeData guaranteedStat = statUpgrades[statIndex];
            selectedUpgrades.Add(guaranteedStat);
            validUpgrades.Remove(guaranteedStat);
        }

        // Fill phần còn lại ngẫu nhiên không trùng
        while (selectedUpgrades.Count < count && validUpgrades.Count > 0)
        {
            int randomIndex = Random.Range(0, validUpgrades.Count);
            selectedUpgrades.Add(validUpgrades[randomIndex]);
            validUpgrades.RemoveAt(randomIndex);
        }

        return selectedUpgrades.ToArray();
    }


    private void UpdateInterface()
    {
        float expFill = 0f;
        levelText.text = currentLevel.ToString();
        if (nextLevelExp > previousLevelExp)
        {
            expFill = (float)(currentExp - previousLevelExp) / (nextLevelExp - previousLevelExp);
        }
        expSlider.value = expFill;
    }
}
