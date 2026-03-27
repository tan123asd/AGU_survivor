using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Shows a Game Over panel when all players die.
/// Wires PlayerController.OnAllPlayersDied → MapManager.GameOver() → shows UI.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button returnToRoomButton;

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Wire: all players dead → MapManager.GameOver()
        if (PlayerController.Instance != null)
            PlayerController.Instance.OnAllPlayersDied.AddListener(OnAllPlayersDied);

        // Wire: MapManager.OnGameOver → show UI
        if (MapManager.Instance != null)
            MapManager.Instance.OnGameOver.AddListener(ShowGameOver);

        if (returnToRoomButton != null)
            returnToRoomButton.onClick.AddListener(ReturnToRoom);
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
        // Disable player input
        if (PlayerController.Instance != null)
            PlayerController.Instance.SetAllPlayersInputEnabled(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    /// <summary>
    /// Called by the "Return to Room" button.
    /// Leaves the Photon room (triggers scene load back to lobby),
    /// or loads the room scene directly if offline.
    /// </summary>
    public void ReturnToRoom()
    {
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.LeaveRoom();
        else
            SceneManager.LoadScene("1_PhotonRoom");
    }
}
