using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeButtonChoice : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private UpgradeData upgradeOption;

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
        
        // Lấy current weapon level nếu là upgrade mode (để hiển thị "Fireball II", "III"...)
        int currentWeaponLevel = 1;
        if (upgradeData.upgradeType == UpgradeData.UpgradeType.Weapon 
            && upgradeData.weaponMode == UpgradeData.WeaponUpgradeMode.Upgrade)
        {
            WeaponController weaponController = ResolveLocalWeaponController();
            if (weaponController != null)
            {
                Weapon weapon = weaponController.GetWeapon(upgradeData.GetTargetWeaponName());
                if (weapon != null)
                {
                    currentWeaponLevel = weapon.WeaponLevel;
                }
            }
        }
        
        // Dùng getters để lấy info (tự động từ WeaponData hoặc manual)
        iconImage.sprite = upgradeData.GetIcon();
        nameText.text = upgradeData.GetDisplayName(currentWeaponLevel);
        descriptionText.text = upgradeData.GetDescription();

        gameObject.SetActive(true);
    }

    private void OnClick()
    {
        UpgradePanel.instance.SelectUpgrade(upgradeOption);
    }
}