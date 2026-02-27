using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI hiển thị thời gian còn lại và cảnh báo khi sắp hết giờ.
/// </summary>
public class TimeUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timeText; // Hoặc Text nếu không dùng TextMeshPro
    [SerializeField] private Image timerFillBar; // Fill bar hiển thị thời gian (optional)
    [SerializeField] private GameObject warningPanel; // Panel cảnh báo khi sắp hết giờ

    [Header("Warning Settings")]
    [SerializeField] private float warningThreshold = 60f; // Cảnh báo khi còn 60 giây
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;

    private TimeManager timeManager;
    private bool isWarningActive = false;

    private void Start()
    {
        // ✅ CHỈ CHẠY KHI Ở PLAY MODE
        if (!Application.isPlaying) return;
        
        // Tìm TimeManager
        timeManager = FindFirstObjectByType<TimeManager>();
        
        if (timeManager == null)
        {
            Debug.LogError("TimeManager not found! TimeUI will not work.");
            return;
        }

        // Ẩn warning panel ban đầu
        if (warningPanel != null)
            warningPanel.SetActive(false);

        // Subscribe to time update event
        if (timeManager != null)
        {
            timeManager.OnTimeUpdate.AddListener(UpdateTimeDisplay);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe khi destroy
        if (timeManager != null)
        {
            timeManager.OnTimeUpdate.RemoveListener(UpdateTimeDisplay);
        }
    }

    /// <summary>
    /// Cập nhật hiển thị thời gian.
    /// </summary>
    private void UpdateTimeDisplay(float remainingTime)
    {
        // Cập nhật text
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            timeText.text = $"{minutes:00}:{seconds:00}";

            // Đổi màu khi sắp hết giờ
            if (remainingTime <= warningThreshold && remainingTime > 0)
            {
                timeText.color = warningColor;
                ShowWarning();
            }
            else
            {
                timeText.color = normalColor;
                HideWarning();
            }
        }

        // Cập nhật fill bar
        if (timerFillBar != null && timeManager != null)
        {
            timerFillBar.fillAmount = timeManager.GetTimePercentage();
            
            // Đổi màu fill bar
            if (remainingTime <= warningThreshold && remainingTime > 0)
                timerFillBar.color = warningColor;
            else
                timerFillBar.color = normalColor;
        }

        // Khi hết giờ - hiển thị "BOSS PHASE!"
        if (remainingTime <= 0)
        {
            if (timeText != null)
                timeText.text = "BOSS PHASE!";
            HideWarning();
        }
    }

    /// <summary>
    /// Hiển thị cảnh báo.
    /// </summary>
    private void ShowWarning()
    {
        if (isWarningActive) return;
        
        isWarningActive = true;
        if (warningPanel != null)
            warningPanel.SetActive(true);
    }

    /// <summary>
    /// Ẩn cảnh báo.
    /// </summary>
    private void HideWarning()
    {
        if (!isWarningActive) return;
        
        isWarningActive = false;
        if (warningPanel != null)
            warningPanel.SetActive(false);
    }

    /// <summary>
    /// Hiển thị message custom (cho Victory/Game Over).
    /// </summary>
    public void ShowCustomMessage(string message)
    {
        if (timeText != null)
        {
            timeText.text = message;
            timeText.color = Color.yellow;
        }
    }
}
