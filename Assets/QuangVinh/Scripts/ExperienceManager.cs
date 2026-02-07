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

        for(int i = 0; i < count && availableUpgrades.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableUpgrades.Count);
            selectedUpgrades.Add(availableUpgrades[randomIndex]);
            availableUpgrades.RemoveAt(randomIndex);
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
