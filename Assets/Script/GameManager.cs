using UnityEngine;

/// <summary>
/// Singleton Game Manager - quản lý game state
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game State")]
    public bool isGamePlaying = false;
    public float gameTime = 0f;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        StartGame();
    }
    
    private void Update()
    {
        if (isGamePlaying)
        {
            gameTime += Time.deltaTime;
        }
    }
    
    public void StartGame()
    {
        isGamePlaying = true;
        gameTime = 0f;
        Time.timeScale = 1f;
        Debug.Log("Game Started!");
    }
    
    public void PauseGame()
    {
        isGamePlaying = false;
        Time.timeScale = 0f;
        Debug.Log("Game Paused");
    }
    
    public void ResumeGame()
    {
        isGamePlaying = true;
        Time.timeScale = 1f;
        Debug.Log("Game Resumed");
    }
    
    public void GameOver()
    {
        isGamePlaying = false;
        Debug.Log($"Game Over! Survived: {gameTime:F1} seconds");
    }
}
