# 📘 HƯỚNG DẪN SETUP HỆ THỐNG TIME LIMIT + BOSS WAVE

## 🎯 **Tổng quan**

Hệ thống này bao gồm:
- **Soft time limit** cho mỗi map (15/20/30 phút)
- Khi hết giờ → Xóa tất cả enemy thường → Spawn BOSS mạnh
- Mỗi phút spawn thêm BOSS mới
- Sống đủ lâu = hoàn thành map + bonus vàng

---

## 📂 **Các file đã tạo**

1. **GameManager.cs** - Quản lý game state
2. **TimeManager.cs** - Quản lý thời gian và trigger boss phase
3. **BossEnemy.cs** - Boss với stats cao hơn (kế thừa Enemy)
4. **MapConfig.cs** - ScriptableObject config cho từng map
5. **TimeUI.cs** - UI hiển thị thời gian còn lại
6. **SpawnEnemy.cs** - ĐÃ CẬP NHẬT để hỗ trợ boss spawn

---

## 🔧 **SETUP TRONG UNITY (8 BƯỚC)**

### **BƯỚC 1: Tạo MapConfig Asset**

1. Trong Unity, chuột phải vào folder `Assets/Script`
2. Chọn: **Create > Game Config > Map Config**
3. Đặt tên: `Map1_Config`
4. Trong Inspector:
   - **Map Name**: "Map 1 - Forest"
   - **Time Limit In Seconds**: `900` (15 phút)
   - **Boss Prefab**: Kéo Boss prefab vào (tạo ở Bước 2)
   - **Initial Boss Count**: `1`
   - **Boss Spawn Interval**: `60` (spawn thêm boss mỗi 60 giây)
   - **Boss Health Multiplier**: `10` (boss có máu gấp 10)
   - **Boss Damage Multiplier**: `3` (boss damage gấp 3)
   - **Completion Bonus Gold**: `1000`
   - **Survival Time Required**: `120` (sống thêm 2 phút)

---

### **BƯỚC 2: Tạo Boss Prefab**

1. Duplicate một Enemy prefab hiện có trong folder `Assets/Prefabs`
2. Đổi tên thành: `BossEnemy`
3. Mở prefab, **thêm component**: `BossEnemy` (script)
4. **Xóa component** `Enemy` (nếu có) - Vì BossEnemy đã kế thừa Enemy
5. Trong Inspector của BossEnemy:
   - **Boss Health Multiplier**: `10`
   - **Boss Damage Multiplier**: `3`
   - **Boss Speed Multiplier**: `0.7`
   - **Boss Color**: Màu đỏ (255, 0, 0)
6. Save prefab

---

### **BƯỚC 3: Setup GameManager**

1. Trong Scene Hierarchy, chuột phải → **Create Empty**
2. Đổi tên: `GameManager`
3. **Add Component**: `GameManager` (script)
4. Trong Inspector:
   - **Time Manager**: Kéo TimeManager GameObject vào (tạo ở Bước 4)
   - **Enemy Spawner**: Kéo GameObject chứa `SpawnEnemy` script vào

---

### **BƯỚC 4: Setup TimeManager**

1. Trong Scene Hierarchy, chuột phải → **Create Empty**
2. Đổi tên: `TimeManager`
3. **Add Component**: `TimeManager` (script)
4. Trong Inspector:
   - **Current Map Config**: Kéo `Map1_Config` vào
   - **Boss Spawn Interval**: `60`
   - **Completion Time After Boss**: `120`

---

### **BƯỚC 5: Setup SpawnEnemy (Cập nhật)**

1. Tìm GameObject có component `SpawnEnemy` trong Scene
2. Trong Inspector:
   - **Boss Prefab**: Kéo `BossEnemy` prefab vào
   - **Map Config**: Kéo `Map1_Config` vào
   - **Main Camera**: Kéo Main Camera vào (hoặc để auto-find)

---

### **BƯỚC 6: Setup TimeUI**

#### **Nếu dùng TextMeshPro (khuyến nghị):**

1. Trong Hierarchy, chuột phải → **UI > Canvas** (nếu chưa có)
2. Trong Canvas, chuột phải → **UI > TextMeshPro - Text**
3. Đổi tên: `TimeText`
4. Chỉnh position: Top-Center của màn hình
5. Chỉnh font size: `48`
6. Text: `15:00` (placeholder)

7. Tạo thêm GameObject trong Canvas:
   - Chuột phải → **Create Empty**
   - Đổi tên: `TimeUI`
   - **Add Component**: `TimeUI` (script)
   - Trong Inspector:
     - **Time Text**: Kéo `TimeText` vào
     - **Warning Threshold**: `60` (cảnh báo khi còn 60 giây)
     - **Normal Color**: White
     - **Warning Color**: Red

#### **Nếu KHÔNG có TextMeshPro:**

Cài đặt TextMeshPro:
1. Menu: **Window > TextMeshPro > Import TMP Essential Resources**
2. Làm theo hướng dẫn trên

---

### **BƯỚC 7: Tạo Warning Panel (Optional)**

1. Trong Canvas, chuột phải → **UI > Panel**
2. Đổi tên: `WarningPanel`
3. Chỉnh màu: Đỏ nhạt (Alpha: 50-100)
4. Thêm Text: "BOSS INCOMING!"
5. **Disable** panel trong Inspector (✅ Active → bỏ tick)
6. Trong `TimeUI` component:
   - **Warning Panel**: Kéo `WarningPanel` vào

---

### **BƯỚC 8: Link PlayerHealth với GameManager**

Cập nhật [PlayerHealth.cs](Assets/Script/PlayerHealth.cs) để gọi `GameManager.GameOver()` khi player chết:

```csharp
public void Die()
{
    Debug.Log("=== DIE() CALLED ===");
    
    currentHealth = 0;
    
    if (healthBar != null)
        healthBar.SetHealth(0);

    // Trigger animation chết
    if (animator != null)
    {
        animator.SetTrigger("Die");
    }

    // Disable movement
    PlayerMovement2D movement = GetComponent<PlayerMovement2D>();
    if (movement != null)
        movement.enabled = false;

    // Disable collider
    Collider2D col = GetComponent<Collider2D>();
    if (col != null)
        col.enabled = false;

    // ✅ GỌI GAME MANAGER
    if (GameManager.Instance != null)
        GameManager.Instance.GameOver();
}
```

---

## ✅ **KIỂM TRA HỆ THỐNG**

### **Test 1: Normal Spawn**
1. Play game
2. Enemy spawn mỗi 5 giây
3. Số lượng tăng dần

### **Test 2: Time Limit**
1. Trong `TimeManager` Inspector, đặt **Custom Time Limit** = `60` (1 phút) để test nhanh
2. Play game
3. Sau 60 giây → Console log: "TIME LIMIT REACHED! Starting Boss Phase"
4. Tất cả enemy thường biến mất
5. Boss xuất hiện (màu đỏ, to hơn)

### **Test 3: Boss Spawn**
1. Tiếp tục chơi sau khi Boss xuất hiện
2. Mỗi 60 giây → Spawn thêm 1 Boss mới

### **Test 4: Completion**
1. Sống sót 2 phút sau khi Boss xuất hiện
2. Console log: "GAME COMPLETED! Bonus Gold: 1000"

### **Test 5: Game Over**
1. Để Boss giết player
2. Console log: "GAME OVER"
3. Player không thể di chuyển

---

## 🎨 **TÙY CHỈNH TỪNG MAP**

### **Map 2 - Harder (20 phút)**

Tạo `Map2_Config`:
- **Time Limit**: `1200` (20 phút)
- **Initial Boss Count**: `2` (2 boss cùng lúc)
- **Boss Spawn Interval**: `45` (spawn nhanh hơn)
- **Boss Health Multiplier**: `15`
- **Completion Bonus**: `2000`

### **Map 3 - Hardest (30 phút)**

Tạo `Map3_Config`:
- **Time Limit**: `1800` (30 phút)
- **Initial Boss Count**: `3`
- **Boss Spawn Interval**: `30`
- **Boss Health Multiplier**: `20`
- **Completion Bonus**: `5000`

---

## 🐛 **TROUBLESHOOTING**

### **Lỗi: "GameManager.Instance is null"**
**Giải pháp:**
- Đảm bảo có GameObject `GameManager` trong Scene
- Component `GameManager` phải được enable

### **Lỗi: "Boss prefab is not assigned"**
**Giải pháp:**
- Kiểm tra `SpawnEnemy` component → **Boss Prefab** phải được assign

### **Lỗi: Boss không có màu đỏ**
**Giải pháp:**
- Kiểm tra Boss prefab có component `SpriteRenderer`
- `BossEnemy` script sẽ tự động set màu trong `Awake()`

### **Lỗi: Enemy thường không biến mất khi boss phase**
**Giải pháp:**
- Đảm bảo tất cả enemy prefab có tag `Enemy`
- Check Console log xem có message "Cleared X normal enemies!"

### **Lỗi: TimeUI không hiển thị**
**Giải pháp:**
- Kiểm tra Canvas có **Canvas Scaler** component
- Render Mode: **Screen Space - Overlay**
- Đảm bảo `TimeText` nằm TRONG Canvas

---

## 🚀 **MỞ RỘNG THÊM (OPTIONAL)**

### **1. Boss Health Bar**
Tạo UI riêng cho Boss:
```csharp
// Trong BossEnemy.cs
private void Awake()
{
    base.Awake();
    
    // Spawn boss health bar
    if (bossHealthBarPrefab != null)
    {
        GameObject healthBarObj = Instantiate(bossHealthBarPrefab, transform);
        // Setup health bar...
    }
}
```

### **2. Boss Abilities**
Thêm skill cho Boss:
```csharp
// Trong BossEnemy.cs
private void Update()
{
    base.Update();
    
    // Boss skill mỗi 5 giây
    if (Time.time - lastSkillTime > 5f)
    {
        UseSkill();
        lastSkillTime = Time.time;
    }
}

private void UseSkill()
{
    // Ví dụ: Spawn minions, fire projectiles...
}
```

### **3. Boss Music**
Chơi nhạc boss khi vào Boss Phase:
```csharp
// Trong TimeManager.cs, trong TriggerBossPhase()
AudioSource audioSource = GetComponent<AudioSource>();
if (audioSource != null && bossMusicClip != null)
    audioSource.PlayOneShot(bossMusicClip);
```

### **4. Slow Motion Effect**
Hiệu ứng slow-mo khi boss xuất hiện:
```csharp
// Trong GameManager.cs, trong StartBossPhase()
StartCoroutine(BossIntroSlowMotion());

IEnumerator BossIntroSlowMotion()
{
    Time.timeScale = 0.3f; // Slow motion
    yield return new WaitForSecondsRealtime(2f);
    Time.timeScale = 1f; // Normal speed
}
```

---

## 📝 **CHECKLIST CUỐI CÙNG**

- [ ] ✅ Đã tạo MapConfig asset
- [ ] ✅ Đã tạo Boss prefab với BossEnemy script
- [ ] ✅ GameManager trong Scene và linked
- [ ] ✅ TimeManager trong Scene và linked với MapConfig
- [ ] ✅ SpawnEnemy có Boss Prefab và Map Config
- [ ] ✅ TimeUI hiển thị đúng thời gian
- [ ] ✅ PlayerHealth gọi GameManager.GameOver()
- [ ] ✅ Test qua tất cả 5 case

---

## 🎉 **KẾT QUẢ**

Sau khi setup xong:
- ✅ Game có time limit rõ ràng
- ✅ Boss xuất hiện đúng lúc
- ✅ Boss mạnh hơn rất nhiều so với enemy thường
- ✅ Spawn thêm boss mỗi phút
- ✅ Player có thể hoàn thành map nếu sống đủ lâu
- ✅ Nhận bonus vàng khi hoàn thành

**Chúc bạn thành công!** 🚀
