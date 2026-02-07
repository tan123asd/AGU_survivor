using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeButtonChoice : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private UpgradeData upgradeOption;

    private void Awake()
    {

        Button button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("UpgradeButtonChoice: No Button component found on the GameObject.");
            return;
        }
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }
    public void Setup(UpgradeData upgradeData)
    {
        upgradeOption = upgradeData;
        iconImage.sprite = upgradeData.icon;
        nameText.text = upgradeData.displayName;
        descriptionText.text = upgradeData.description;

        gameObject.SetActive(true);
    }

    private void OnClick()
    {
        UpgradePanel.instance.SelectUpgrade(upgradeOption);
    }
}