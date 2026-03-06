using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Quản lý thời gian của map và kích hoạt Boss Phase khi hết time limit.
/// </summary>
public class TimeManager : MonoBehaviour
{
    [Header("Map Time Settings")]
    [SerializeField] private MapConfig currentMapConfig; // Reference đến MapConfig
    [SerializeField] private float customTimeLimit = 10f; // 10 giây để test nhanh

    [Header("Boss Phase Settings")]
    [SerializeField] private float bossSpawnInterval = 60f; // Spawn boss mới mỗi 60 giây
    [SerializeField] private float completionTimeAfterBoss = 120f; // Sống thêm 2 phút = hoàn thành

    [Header("Runtime State")]
    [SerializeField] private float elapsedTime = 0f;
    [SerializeField] private float remainingTime = 0f;
    [SerializeField] private bool isTimerRunning = false;

    private float bossPhaseStartTime = 0f;
    private float lastBossSpawnTime = 0f;

    [Header("Events")]
    public UnityEvent<float> OnTimeUpdate; // Gửi remaining time mỗi giây
    public UnityEvent OnTimeLimitReached; // Khi hết time limit

    public float ElapsedTime => elapsedTime;
    public float RemainingTime => remainingTime;
    public float TimeLimit => currentMapConfig != null ? currentMapConfig.timeLimit : customTimeLimit;
    public bool IsTimerRunning => isTimerRunning;
    public float BossPhaseStartTime => bossPhaseStartTime;

    private void Awake()
    {
        // Nếu có MapConfig thì dùng, không thì dùng customTimeLimit
        if (currentMapConfig != null)
        {
            customTimeLimit = currentMapConfig.timeLimit;
        }
        
        remainingTime = TimeLimit;
    }

    private void Update()
    {
        // ✅ CHỈ CHẠY KHI Ở PLAY MODE
        if (!Application.isPlaying) return;
        if (!isTimerRunning) return;

        // Đếm thời gian
        elapsedTime += Time.deltaTime;
        remainingTime = Mathf.Max(0, TimeLimit - elapsedTime);

        // Kiểm tra hết time limit
        if (GameManager.Instance != null && remainingTime <= 0 && !GameManager.Instance.IsBossPhase)
        {
            OnTimeLimitReached?.Invoke();
            TriggerBossPhase();
        }

        // Nếu đang ở Boss Phase
        if (GameManager.Instance != null && GameManager.Instance.IsBossPhase)
        {
            HandleBossPhase();
        }

        // Gửi event cập nhật UI (mỗi frame hoặc mỗi giây)
        OnTimeUpdate?.Invoke(remainingTime);
    }

    /// <summary>
    /// Bắt đầu đếm thời gian.
    /// </summary>
    public void StartTimer()
    {
        isTimerRunning = true;
        elapsedTime = 0f;
        remainingTime = TimeLimit;
        Debug.Log($"Timer started! Time limit: {TimeLimit} seconds ({TimeLimit / 60f} minutes)");
    }

    /// <summary>
    /// Dừng đếm thời gian.
    /// </summary>
    public void StopTimer()
    {
        isTimerRunning = false;
        Debug.Log("Timer stopped!");
    }

    /// <summary>
    /// Kích hoạt Boss Phase khi hết time limit.
    /// </summary>
    private void TriggerBossPhase()
    {
        Debug.Log("=== TIME LIMIT REACHED! Starting Boss Phase ===");
        
        bossPhaseStartTime = Time.time;
        lastBossSpawnTime = Time.time;
        
        // Gọi GameManager để bắt đầu Boss Phase
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartBossPhase();
        }
    }

    /// <summary>
    /// Xử lý logic trong Boss Phase.
    /// </summary>
    private void HandleBossPhase()
    {
        float timeSinceBossPhase = Time.time - bossPhaseStartTime;

        // Spawn thêm boss mỗi [bossSpawnInterval] giây
        if (Time.time - lastBossSpawnTime >= bossSpawnInterval)
        {
            lastBossSpawnTime = Time.time;
            SpawnAdditionalBoss();
        }

        // Kiểm tra điều kiện hoàn thành (sống đủ lâu)
        if (timeSinceBossPhase >= completionTimeAfterBoss)
        {
            CheckCompletionCondition();
        }
    }

    /// <summary>
    /// Spawn thêm boss mỗi phút.
    /// </summary>
    private void SpawnAdditionalBoss()
    {
        SpawnEnemy spawner = FindFirstObjectByType<SpawnEnemy>();
        if (spawner != null)
        {
            spawner.SpawnSingleBoss();
            Debug.Log("Spawned additional BOSS!");
        }
    }

    /// <summary>
    /// Kiểm tra điều kiện hoàn thành map.
    /// </summary>
    private void CheckCompletionCondition()
    {
        // Nếu player còn sống và đã sống đủ lâu → hoàn thành!
        PlayerHealth player = FindFirstObjectByType<PlayerHealth>();
        if (player != null && !player.IsDead)
        {
            int bonusGold = currentMapConfig != null ? currentMapConfig.completionBonus : 1000;
            GameManager.Instance.CompleteGame(bonusGold);
            StopTimer();
        }
    }

    /// <summary>
    /// Format thời gian thành string MM:SS.
    /// </summary>
    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    /// <summary>
    /// Lấy phần trăm thời gian còn lại (0-1).
    /// </summary>
    public float GetTimePercentage()
    {
        return remainingTime / TimeLimit;
    }
}
