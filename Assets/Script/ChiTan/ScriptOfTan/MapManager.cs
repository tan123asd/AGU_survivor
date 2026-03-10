using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Singleton quản lý toàn bộ game state và điều phối các hệ thống.
/// </summary>
public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private bool isGameStarted = false;
    [SerializeField] private bool isGameOver = false;
    [SerializeField] private bool isBossPhase = false;

    [Header("References")]
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private SpawnEnemy enemySpawner;

    [Header("Events")]
    public UnityEvent OnGameStart;
    public UnityEvent OnBossPhaseStart;
    public UnityEvent OnGameComplete;
    public UnityEvent OnGameOver;

    public bool IsGameStarted => isGameStarted;
    public bool IsGameOver => isGameOver;
    public bool IsBossPhase => isBossPhase;

    private void Awake()
    {
        // ✅ CHỈ CHẠY KHI Ở PLAY MODE
        if (!Application.isPlaying) return;
        
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Giữ qua scene (optional)

        // Auto-find references nếu chưa assign
        if (timeManager == null)
            timeManager = FindFirstObjectByType<TimeManager>();
        if (enemySpawner == null)
            enemySpawner = FindFirstObjectByType<SpawnEnemy>();
    }

    private void Start()
    {
        // ✅ CHỈ CHẠY KHI Ở PLAY MODE
        if (!Application.isPlaying) return;
        
        StartGame();
    }

    /// <summary>
    /// Bắt đầu game.
    /// </summary>
    public void StartGame()
    {
        if (isGameStarted) return;

        isGameStarted = true;
        isGameOver = false;
        isBossPhase = false;

        Debug.Log("=== GAME STARTED ===");
        OnGameStart?.Invoke();

        // Bắt đầu đếm thời gian
        if (timeManager != null)
            timeManager.StartTimer();
    }

    /// <summary>
    /// Kích hoạt Boss Phase khi hết time limit.
    /// </summary>
    public void StartBossPhase()
    {
        if (isBossPhase || isGameOver) return;

        isBossPhase = true;

        Debug.Log("=== BOSS PHASE STARTED ===");
        OnBossPhaseStart?.Invoke();

        // Xóa tất cả enemy thường
        ClearAllNormalEnemies();

        // Bắt đầu spawn boss
        if (enemySpawner != null)
            enemySpawner.StartBossSpawn();
    }

    /// <summary>
    /// Xóa tất cả enemy thường (không phải boss).
    /// </summary>
    private void ClearAllNormalEnemies()
    {
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        
        foreach (Enemy enemy in allEnemies)
        {
            // Chỉ xóa enemy thường, không xóa boss
            BossEnemy boss = enemy.GetComponent<BossEnemy>();
            if (boss == null)
            {
                Destroy(enemy.gameObject);
            }
        }

        Debug.Log($"Cleared {allEnemies.Length} normal enemies!");
    }

    /// <summary>
    /// Hoàn thành map (sống sót qua boss phase).
    /// </summary>
    public void CompleteGame(int survivalBonus)
    {
        if (isGameOver) return;

        isGameOver = true;

        Debug.Log($"=== GAME COMPLETED! Bonus Gold: {survivalBonus} ===");
        OnGameComplete?.Invoke();

        // TODO: Thêm vàng, hiện UI Victory, chuyển scene...
        // PlayerInventory.AddGold(survivalBonus);
        // SceneManager.LoadScene("VictoryScene");
    }

    /// <summary>
    /// Player chết - Game Over.
    /// </summary>
    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        Debug.Log("=== GAME OVER ===");
        OnGameOver?.Invoke();

        // TODO: Hiện UI Game Over, chuyển scene...
        // Invoke("LoadGameOverScene", 2f);
    }

    /// <summary>
    /// Kiểm tra xem có đủ điều kiện hoàn thành không.
    /// </summary>
    public bool CanCompleteGame()
    {
        // Ví dụ: Phải sống sót ít nhất 2 phút sau khi boss xuất hiện
        if (isBossPhase && timeManager != null)
        {
            float bossElapsedTime = Time.time - timeManager.BossPhaseStartTime;
            return bossElapsedTime >= 120f; // 2 phút
        }
        return false;
    }
}
