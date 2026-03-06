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
        var modifiers = upgradeData.GetStatModifiers();
        // Apply each modifier (if that modifier change more than 1 stat) to the player stats
        for (int i = 0; i < modifiers.Length; i++)
        {
            var effect = upgradeData.buffEffects[i];
            var modifier = modifiers[i];
            PlayerStats.Instance.AddModifier(effect.effectName, modifier);
        }

        Time.timeScale = 1f;
        gameObject.SetActive(false);

        foreach (var btn in currentButtons)
        {
            if (btn != null) Destroy(btn.gameObject);
        }
        currentButtons.Clear();
    }
}
