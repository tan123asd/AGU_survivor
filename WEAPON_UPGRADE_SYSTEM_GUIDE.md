# 🎮 Hướng Dẫn Hệ Thống Nâng Cấp Vũ Khí

## 📚 Tổng Quan Hệ Thống

Hệ thống cho phép:
- ✅ **Thêm vũ khí mới** khi lên level (Add Mode)
- ✅ **Nâng cấp vũ khí hiện có** từ Level 1→5 (Upgrade Mode)
- ✅ **Tự động lọc** các lựa chọn hợp lý (không hiện vũ khí đã có khi Add, không hiện vũ khí chưa có khi Upgrade)
- ✅ **Tăng sức mạnh** theo level: Damage +10%, Cooldown -5%, Range +10%

---

## 🔧 BƯỚC 1: Setup WeaponController trên Player

1. Chọn **Player GameObject** trong Scene
2. Add Component → **WeaponController**
3. Trong Inspector, set:
   ```
   Max Weapons: 6  (tối đa 6 vũ khí cùng lúc)
   ```

---

## ⚔️ BƯỚC 2: Tạo Weapon Prefab

### 2.1. Tạo GameObject cho Weapon
1. Hierarchy → Right Click → **Create Empty**
2. Rename thành: `FireballWeapon`
3. Add Component → Chọn script vũ khí (ví dụ: **ProjectileWeapon**)

### 2.2. Cấu hình Weapon Script
Trong Inspector của FireballWeapon:

```yaml
ProjectileWeapon Component:
  # Base Weapon Stats (từ Weapon.cs)
  Weapon Name: "FireballWeapon"  ⚠️ QUAN TRỌNG - dùng để tìm weapon khi upgrade
  Weapon Level: 1
  Max Level: 5
  
  Damage: 10
  Cooldown: 2.0
  Range: 5.0
  
  Debug Mode: ✓  (bật để xem log)
  
  # ProjectileWeapon Settings
  Projectile Prefab: [Kéo prefab viên đạn vào đây]
  Fire Point: [Kéo Transform spawn point vào, hoặc để trống]
```

### 2.3. Tạo Prefab
1. Kéo GameObject `FireballWeapon` từ Hierarchy vào thư mục:  
   ```
   Assets/ChiTan/Weapon/Prefabs/
   ```
2. Delete GameObject trong Scene (giữ lại prefab)

---

## 📦 BƯỚC 3: Tạo UpgradeData cho ADD Weapon

### 3.1. Tạo ScriptableObject
1. Project → Right Click trong thư mục `Assets/QuangVinh/Upgrades/`
2. Create → **UpgradeData**
3. Rename: `Upgrade_Fireball_Add`

### 3.2. Cấu hình UpgradeData
```yaml
UpgradeData Settings:
  Upgrade Name: "Fireball"
  Icon: [Kéo icon vào đây]
  Description: "Launch fireballs at enemies"
  
  ⚠️ Upgrade Type: Weapon  (chọn dropdown)
  
  ⚠️ Weapon Mode: Add  (chọn dropdown)
  Weapon Prefab: FireballWeapon  (kéo prefab vào)
  
  # Không cần điền phần này khi Add mode:
  Target Weapon Name: (để trống)
  Buff Effect: (không cần)
```

---

## 📦 BƯỚC 4: Tạo UpgradeData cho UPGRADE Weapon

### 4.1. Tạo Upgrade Levels (Level 2-5)
Tạo **4 UpgradeData** cho mỗi level:
- `Upgrade_Fireball_Level2`
- `Upgrade_Fireball_Level3`
- `Upgrade_Fireball_Level4`
- `Upgrade_Fireball_Level5`

### 4.2. Cấu hình cho Level 2
```yaml
UpgradeData Settings:
  Upgrade Name: "Fireball II"
  Icon: [Same icon hoặc icon khác]
  Description: "Upgrade Fireball (Lv.2) - +10% Damage, -5% Cooldown"
  
  ⚠️ Upgrade Type: Weapon
  
  ⚠️ Weapon Mode: Upgrade  (chọn dropdown)
  Weapon Prefab: (để trống)
  
  ⚠️ Target Weapon Name: "FireballWeapon"  (phải khớp với weaponName trong prefab)
  
  # Không cần điền:
  Buff Effect: (không cần)
```

### 4.3. Tương tự cho Level 3, 4, 5
Chỉ thay đổi:
- `Upgrade Name`: "Fireball III", "Fireball IV", "Fireball V"
- `Description`: Cập nhật mô tả level

---

## 🎯 BƯỚC 5: Thêm UpgradeData vào ExperienceManager

1. Hierarchy → Chọn GameObject có **ExperienceManager** (thường là GameManager)
2. Inspector → Tìm component **ExperienceManager**
3. Mở mục **Upgrade Datas** (Array)
4. Kéo TẤT CẢ UpgradeData vào array:
   ```
   [0] Upgrade_Fireball_Add
   [1] Upgrade_Fireball_Level2
   [2] Upgrade_Fireball_Level3
   [3] Upgrade_Fireball_Level4
   [4] Upgrade_Fireball_Level5
   [5] ...các upgrade khác (Stat upgrades, weapons khác)
   ```

---

## 🧪 BƯỚC 6: Test Trong Unity

### Test Flow:
1. **Play Mode** → Thu thập EXP
2. **Lên level lần 1** → UI hiện 3 lựa chọn:
   - ✅ "Fireball" (Add mode) - chưa có weapon
   - ✅ Các stat upgrades
   - ❌ KHÔNG hiện Fireball Level 2-5 (vì chưa có weapon)

3. **Chọn "Fireball"** → Console log:
   ```
   ✅ Added weapon: FireballWeapon to player
   🔥 Fireball weapon đang hoạt động!
   ```

4. **Lên level lần 2** → UI hiện:
   - ✅ "Fireball II" (Upgrade mode) - đã có weapon level 1
   - ✅ Các stat upgrades
   - ❌ KHÔNG hiện "Fireball" Add mode nữa (đã có rồi)

5. **Chọn "Fireball II"** → Console log:
   ```
   ✨ Upgraded FireballWeapon to level 2
   🌟 FireballWeapon upgraded to level 2!
   ```

6. **Stats tăng tự động:**
   - Level 1: Damage 10, Cooldown 2.0s, Range 5
   - Level 2: Damage 11, Cooldown 1.9s, Range 5.5
   - Level 3: Damage 12.1, Cooldown 1.805s, Range 6.05
   - Level 4: Damage 13.31, Cooldown 1.715s, Range 6.655
   - Level 5: Damage 14.64, Cooldown 1.629s, Range 7.32

7. **Lên level khi Fireball đã Max (level 5):**
   - ❌ KHÔNG hiện "Fireball V" nữa (đã max)
   - ✅ Chỉ hiện các weapon/stat khác có thể upgrade

---

## 🛠️ Customize Weapon Subclass

### Nếu bạn tạo weapon mới (ví dụ: LaserWeapon)

1. **Tạo script LaserWeapon.cs kế thừa Weapon**
   ```csharp
   public class LaserWeapon : Weapon
   {
       protected override void Awake()
       {
           base.Awake();
           weaponName = "LaserWeapon"; // ⚠️ Unique name
       }

       protected override void OnUpgrade()
       {
           Debug.Log($"⚡ {weaponName} upgraded to level {weaponLevel}!");
           // Thêm effect: particle, sound, bonus stat, etc.
       }

       protected override void Attack()
       {
           // Logic bắn laser
       }
   }
   ```

2. **Tạo prefab LaserWeapon** với script trên
3. **Tạo UpgradeData:**
   - 1 Add mode: `Upgrade_Laser_Add`
   - 4 Upgrade mode: `Upgrade_Laser_Level2/3/4/5`

---

## 🐛 Troubleshooting

### ❌ "Weapon not found for upgrade!"
- **Nguyên nhân:** `targetWeaponName` trong UpgradeData không khớp với `weaponName` trong prefab
- **Fix:** Kiểm tra chính tả, case-sensitive (FireballWeapon ≠ fireballweapon)

### ❌ Upgrade không tăng stats
- **Nguyên nhân:** Forgot to call `CalculateStats()` trong `Upgrade()` method
- **Fix:** Đã có sẵn trong base class Weapon.cs, không cần sửa

### ❌ UI hiện Fireball Level 2 khi chưa có Fireball
- **Nguyên nhân:** ExperienceManager không tìm thấy WeaponController
- **Fix:** Đảm bảo Player GameObject có component **WeaponController**

### ❌ Chọn upgrade nhưng không có gì xảy ra
- **Nguyên nhân:** UpgradePanel.cs đang dùng version cũ
- **Fix:** Code đã updated, check console log để debug

---

## 📝 Code Flow Summary

```
Player lên level
   ↓
ExperienceManager.LevelUp()
   ↓
GetRandomUpgrades() → Lọc thông minh:
   • Weapon Add mode: Chỉ hiện nếu chưa có
   • Weapon Upgrade mode: Chỉ hiện nếu có + chưa max level
   • Stat upgrade: Luôn hiện
   ↓
UpgradePanel.Show(3 upgrades)
   ↓
User chọn 1 trong 3
   ↓
SelectUpgrade(UpgradeData)
   ↓
[IF Weapon + Add mode]
   → AddWeaponToPlayer()
   → weaponController.AddWeapon(prefab)
   → Weapon.Awake() → set weaponName
   
[IF Weapon + Upgrade mode]
   → UpgradePlayerWeapon()
   → weaponController.UpgradeWeapon(targetWeaponName)
   → weapon.Upgrade()
      → weaponLevel++
      → CalculateStats() → Damage/Cooldown/Range scale
      → OnUpgrade() → Custom effects
      
[IF Stat upgrade]
   → PlayerStats.ApplyBuffs()
```

---

## 🎨 Best Practices

1. **Weapon Naming Convention:**
   - Prefab: `FireballWeapon`, `LaserWeapon`, `SwordWeapon`
   - weaponName field: Same as prefab name
   - UpgradeData Add: `Upgrade_Fireball_Add`
   - UpgradeData Upgrade: `Upgrade_Fireball_Level2/3/4/5`

2. **Max Level = 5:** Hiện tại hard-coded trong Weapon.cs. Nếu muốn thay đổi, sửa:
   ```csharp
   public int maxLevel = 5; // Đổi thành 10 nếu muốn
   ```

3. **Max Weapons = 6:** Trong WeaponController. Player không thể có quá 6 vũ khí cùng lúc.

4. **Stat Scaling:** Hiện tại trong `Weapon.CalculateStats()`:
   ```csharp
   // Level 1: base stats
   // Level 2: damage *1.1, cooldown *0.95, range *1.1
   // Level 3: damage *1.21, cooldown *0.9025, range *1.21
   // ...
   ```

---

## 🚀 Next Steps

1. ✅ Tạo thêm weapon types (MeleeWeapon, LaserWeapon, etc.)
2. ✅ Tạo UpgradeData assets cho tất cả weapons
3. ✅ Test trong game thực tế
4. 🎨 Customize OnUpgrade() effects (particles, sounds, screen shake)
5. 🎨 Add weapon evolution system (Level 5 → Transform to stronger variant)

---

**📞 Support:**
Nếu có lỗi, check:
1. Console log (Debug.Log messages)
2. WeaponController có đúng trên Player GameObject không
3. weaponName field có set đúng trong Awake() không
4. UpgradeData targetWeaponName có khớp với weaponName không
