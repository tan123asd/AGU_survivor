using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Self-contained Game Over UI.
/// Creates its own Canvas, panel, text, and button at runtime.
/// Just add this component to any GameObject in the gameplay scene.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    private GameObject gameOverPanel;

    private void Start()
    {
        BuildUI();

        if (PlayerController.Instance != null)
            PlayerController.Instance.OnAllPlayersDied.AddListener(OnAllPlayersDied);

        if (MapManager.Instance != null)
            MapManager.Instance.OnGameOver.AddListener(ShowGameOver);
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance != null)
            PlayerController.Instance.OnAllPlayersDied.RemoveListener(OnAllPlayersDied);

        if (MapManager.Instance != null)
            MapManager.Instance.OnGameOver.RemoveListener(ShowGameOver);
    }

    private void OnAllPlayersDied()
    {
        if (MapManager.Instance != null)
            MapManager.Instance.GameOver();
    }

    private void ShowGameOver()
    {
        if (PlayerController.Instance != null)
            PlayerController.Instance.SetAllPlayersInputEnabled(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void ReturnToRoom()
    {
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
        else
            SceneManager.LoadScene(1);
    }

    private void BuildUI()
    {
        // --- Canvas ---
        GameObject canvasObj = new GameObject("GameOverCanvas");
        canvasObj.transform.SetParent(transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // --- Dark overlay panel ---
        gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvasObj.transform, false);
        Image panelImage = gameOverPanel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.75f);
        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // --- "GAME OVER" text ---
        GameObject textObj = new GameObject("GameOverText");
        textObj.transform.SetParent(gameOverPanel.transform, false);
        TextMeshProUGUI gameOverText = textObj.AddComponent<TextMeshProUGUI>();
        gameOverText.text = "GAME OVER";
        gameOverText.fontSize = 80;
        gameOverText.color = Color.red;
        gameOverText.alignment = TextAlignmentOptions.Center;
        gameOverText.fontStyle = FontStyles.Bold;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.6f);
        textRect.anchorMax = new Vector2(0.5f, 0.6f);
        textRect.sizeDelta = new Vector2(800, 120);
        textRect.anchoredPosition = Vector2.zero;

        // --- "Return to Room" button ---
        GameObject buttonObj = new GameObject("ReturnButton");
        buttonObj.transform.SetParent(gameOverPanel.transform, false);
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 1f, 1f);
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.3f, 0.7f, 1f, 1f);
        colors.pressedColor = new Color(0.1f, 0.4f, 0.8f, 1f);
        button.colors = colors;
        button.onClick.AddListener(ReturnToRoom);
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.35f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.35f);
        buttonRect.sizeDelta = new Vector2(400, 70);
        buttonRect.anchoredPosition = Vector2.zero;

        // --- Button label ---
        GameObject labelObj = new GameObject("ButtonLabel");
        labelObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI buttonLabel = labelObj.AddComponent<TextMeshProUGUI>();
        buttonLabel.text = "Return to Room";
        buttonLabel.fontSize = 36;
        buttonLabel.color = Color.white;
        buttonLabel.alignment = TextAlignmentOptions.Center;
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        gameOverPanel.SetActive(false);
    }
}
