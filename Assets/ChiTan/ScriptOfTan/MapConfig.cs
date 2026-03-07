using UnityEngine;

/// <summary>
/// ScriptableObject chứa config cho từng map (time limit, bonus, boss settings).
/// Tạo asset bằng cách: Right Click > Create > Game Config > Map Config
/// </summary>
[CreateAssetMenu(fileName = "NewMapConfig", menuName = "Game Config/Map Config", order = 1)]
public class MapConfig : ScriptableObject
{
    [Header("Map Info")]
    [SerializeField] private string mapName = "Map 1";
    [TextArea(2, 4)]
    [SerializeField] private string mapDescription = "Description of this map";

    [Header("Time Settings")]
    [Tooltip("Thời gian giới hạn tính bằng giây (15 phút = 900 giây)")]
    [SerializeField] private float timeLimitInSeconds = 900f; // 15 phút mặc định

    [Header("Boss Settings")]
    [SerializeField] private GameObject bossPrefab; // Prefab của boss
    [SerializeField] private int initialBossCount = 1; // Số boss xuất hiện đầu tiên
    [SerializeField] private float bossSpawnInterval = 10f; // Spawn thêm boss mỗi X giây
    [SerializeField] private int bossHealthMultiplier = 10; // Boss có máu gấp X lần
    [SerializeField] private int bossDamageMultiplier = 3; // Boss damage gấp X lần

    [Header("Completion Rewards")]
    [SerializeField] private int completionBonusGold = 1000; // Vàng thưởng khi hoàn thành
    [SerializeField] private float survivalTimeRequired = 120f; // Sống thêm X giây sau khi boss xuất hiện

    // Public properties để truy cập
    public string MapName => mapName;
    public string MapDescription => mapDescription;
    public float timeLimit => timeLimitInSeconds;
    public GameObject BossPrefab => bossPrefab;
    public int InitialBossCount => initialBossCount;
    public float BossSpawnInterval => bossSpawnInterval;
    public int BossHealthMultiplier => bossHealthMultiplier;
    public int BossDamageMultiplier => bossDamageMultiplier;
    public int completionBonus => completionBonusGold;
    public float SurvivalTimeRequired => survivalTimeRequired;

    /// <summary>
    /// Lấy time limit theo định dạng MM:SS
    /// </summary>
    public string GetFormattedTimeLimit()
    {
        int minutes = Mathf.FloorToInt(timeLimitInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeLimitInSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    /// <summary>
    /// Validate dữ liệu khi tạo mới hoặc chỉnh sửa trong Inspector
    /// </summary>
    // private void OnValidate()
    // {
    //     // Đảm bảo các giá trị không âm
    //     timeLimitInSeconds = Mathf.Max(5f, timeLimitInSeconds); // Tối thiểu 5 giây (cho test)
    //     initialBossCount = Mathf.Max(1, initialBossCount);
    //     bossSpawnInterval = Mathf.Max(10f, bossSpawnInterval);
    //     bossHealthMultiplier = Mathf.Max(1, bossHealthMultiplier);
    //     bossDamageMultiplier = Mathf.Max(1, bossDamageMultiplier);
    //     completionBonusGold = Mathf.Max(0, completionBonusGold);
    //     survivalTimeRequired = Mathf.Max(0f, survivalTimeRequired);
    // }
}
