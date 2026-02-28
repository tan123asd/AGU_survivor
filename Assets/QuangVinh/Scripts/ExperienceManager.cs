using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceManager : MonoBehaviour
{
    [SerializeField] private AnimationCurve expCurve;
    [SerializeField] private UpgradeData[] upgradeDatas;

    private int currentLevel;
    private int currentExp;
    private int previousLevelExp;
    private int nextLevelExp;

    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Slider expSlider;
    [SerializeField] private ParticleSystem levelUpEffect;

    public void AddExp(int exp)
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
            GameObject player = GameObject.FindWithTag("Player");
            if (levelUpEffect != null && player != null)
            {
                ParticleSystem psInstance = Instantiate(levelUpEffect, player.transform.position + new Vector3(0, -0.5f, 0), Quaternion.identity);
                psInstance.transform.SetParent(player.transform);
                StartCoroutine(StopFollowingAndDestroy(psInstance.gameObject, psInstance.main.duration));
            }
        }
    }

    private IEnumerator StopFollowingAndDestroy(GameObject psObj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (psObj != null)
        {
            psObj.transform.SetParent(null);
            ParticleSystem ps = psObj.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                Destroy(ps.gameObject, ps.main.duration);
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
                weaponController = player.GetComponentInChildren<WeaponController>();
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
                        if (weapon != null && weapon.CanUpgrade)
                        {
                            validUpgrades.Add(upgrade);
                        }
                    }
                }
            }
        }
        
        // Nếu không có valid upgrades, fallback về tất cả upgrades
        if (validUpgrades.Count == 0)
        {
            validUpgrades = availableUpgrades;
        }

        // Random chọn
        for(int i = 0; i < count && validUpgrades.Count > 0; i++)
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
