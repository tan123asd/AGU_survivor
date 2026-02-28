# 🗂️ Hệ Thống Quản Lý Vũ Khí - Weapon Management System

## 📁 **Folder Structure**

```
Assets/ChiTan/Weapon/
├── Scripts/           ← Code (Weapon.cs, WeaponController.cs, etc.)
├── Configs/           ← WeaponData assets (Fireball.asset, Laser.asset)
├── Prefabs/           ← Weapon prefabs (FireballWeapon.prefab)
├── Projectiles/       ← Projectile prefabs (Bullet.prefab)
├── WeaponData.cs      ← ScriptableObject definition
└── WeaponDatabase.cs  ← Database quản lý tất cả weapons

Assets/Resources/
└── WeaponDatabase.asset  ← Database asset (BẮT BUỘC để load runtime)
```

---

## 🚀 **HƯỚNG DẪN SỬ DỤNG**

### **Bước 1: Tạo WeaponDatabase (1 LẦN DUY NHẤT)**

1. Project → `Assets/Resources/` folder
2. Right Click → **Create** → **Weapons** → **Weapon Database**
3. Rename: `WeaponDatabase`
4. Để trong folder **Resources** (QUAN TRỌNG!)

---

### **Bước 2: Tạo WeaponData cho từng vũ khí**

#### **Ví dụ: Tạo Fireball Weapon**

1. **Tạo WeaponData asset:**
   - Navigate: `Assets/ChiTan/Weapon/Configs/`
   - Right Click → **Create** → **Weapons** → **Weapon Data**
   - Rename: `FireballWeaponData`

2. **Cấu hình trong Inspector:**
   ```yaml
   Basic Info:
     Weapon Id: fireball              ⚠️ UNIQUE (auto lowercase)
     Display Name: Fireball
     Description: Launch fireballs at enemies
     Icon: [Kéo icon vào]
   
   Weapon Prefab:
     Weapon Prefab: [Kéo FireBall prefab vào]
   
   Base Stats:
     Base Damage: 10
     Base Attack Interval: 1.5
     Base Range: 10
   
   Level Scaling:
     Max Level: 5
     Damage Per Level: 0.1          (10% tăng mỗi level)
     Cooldown Reduction Per Level: 0.05  (5% giảm cooldown)
     Range Per Level: 0.1            (10% tăng range)
   
   Unlock Requirements:
     Min Player Level: 1
     Required Weapon: (None)
   ```

3. **Save asset** (Ctrl+S)

---

### **Bước 3: Setup Weapon Prefab**

1. **Mở Weapon Prefab** (e.g., `FireBall.prefab`)

2. **Trong Inspector → Weapon Component:**
   ```yaml
   Weapon Config (RECOMMENDED):
     Weapon Data: FireballWeaponData  ⚠️ Kéo asset vào đây
   
   # Phần còn lại sẽ AUTO LOAD từ WeaponData
   # Không cần config thủ công nữa!
   ```

3. **Save prefab**

---

### **Bước 4: Add vào WeaponDatabase**

1. Mở `Assets/Resources/WeaponDatabase.asset`

2. Inspector → **All Weapons** (Array)

3. Tăng **Size** lên 1

4. Kéo `FireballWeaponData` vào element mới

5. **Save** (Ctrl+S)

---

### **Bước 5: Tạo UpgradeData (cho level-up UI)**

#### **5.1. Add Mode (Thêm weapon mới)**

1. Tạo UpgradeData: `Assets/QuangVinh/Upgrades/Upgrade_Fireball_Add`

2. Cấu hình:
   ```yaml
   Display Name: Fireball
   Description: Launch fireballs at enemies
   Icon: [Kéo icon]
   
   Upgrade Type: Weapon
   Weapon Mode: Add
   Weapon Prefab: [Kéo FireBall.prefab]
   ```

#### **5.2. Upgrade Mode (Nâng cấp weapon)**

1. Tạo UpgradeData: `Assets/QuangVinh/Upgrades/Upgrade_Fireball`

2. Cấu hình:
   ```yaml
   Display Name: Upgrade Fireball
   Description: Increase Fireball power
   Icon: [Kéo icon]
   
   Upgrade Type: Weapon
   Weapon Mode: Upgrade
   Target Weapon Name: fireball  ⚠️ PHẢI KHỚP với weaponId trong WeaponData
   ```

3. Add cả 2 vào **ExperienceManager** → Upgrade Datas array

---

## 💡 **LỢI ÍCH CỦA HỆ THỐNG MỚI**

### **✅ Trước đây (Cách cũ):**
```
Thêm 1 weapon mới:
1. Tạo prefab → Config 10 fields trong Inspector
2. Tạo UpgradeData Add → Config lại
3. Tạo UpgradeData Upgrade → Config lại
4. Nhớ target weapon name (dễ sai)
5. Balance stats → Sửa ở 3 chỗ khác nhau ❌
```

### **✅ Bây giờ (Cách mới):**
```
Thêm 1 weapon mới:
1. Tạo WeaponData → Config 1 LẦN duy nhất ✅
2. Kéo vào prefab
3. Kéo vào Database
4. Tạo UpgradeData (reference WeaponData)
5. Balance stats → Sửa CHỈ 1 CHỖ ✅
```

---

## 🔧 **API SỬ DỤNG TRONG CODE**

### **Lấy weapon config:**
```csharp
// Lấy từ database
WeaponData fireballData = WeaponDatabase.Instance.GetWeaponById("fireball");

// Lấy từ weapon instance
Weapon weapon = player.GetComponentInChildren<Weapon>();
WeaponData data = weapon.WeaponData;

// Get stats ở level cụ thể
int damageAtLevel3 = fireballData.GetDamageAtLevel(3);
float cooldownAtLevel5 = fireballData.GetAttackIntervalAtLevel(5);
```

### **Query weapons:**
```csharp
// Lấy tất cả weapons player có thể unlock
List<string> ownedWeapons = new List<string> { "fireball", "laser" };
List<WeaponData> available = WeaponDatabase.Instance.GetAvailableWeapons(playerLevel, ownedWeapons);

// Lấy weapons có thể upgrade
Dictionary<string, int> weaponLevels = new Dictionary<string, int> {
    { "fireball", 3 },
    { "laser", 5 }  // Max level
};
List<WeaponData> upgradeable = WeaponDatabase.Instance.GetUpgradeableWeapons(ownedWeapons, weaponLevels);
// → Chỉ trả về "fireball" (laser đã max)
```

---

## 📝 **WORKFLOW THÊM WEAPON MỚI**

### **Ví dụ: Thêm "Lightning Bolt" weapon**

1. **Tạo WeaponData:**
   - File: `Assets/ChiTan/Weapon/Configs/LightningWeaponData.asset`
   - Weapon ID: `lightning`
   - Display Name: `Lightning Bolt`
   - Base Damage: 15 (mạnh hơn Fireball)
   - Base Attack Interval: 2.5 (chậm hơn)
   - Min Player Level: 5 (unlock sau)
   - Required Weapon: `FireballWeaponData` (cần có Fireball trước)

2. **Tạo/Update Prefab:**
   - File: `Assets/ChiTan/Weapon/Prefabs/LightningWeapon.prefab`
   - Add component: `ProjectileWeapon` (hoặc custom LightningWeapon)
   - Weapon Data: Kéo `LightningWeaponData` vào

3. **Add vào Database:**
   - Mở `WeaponDatabase.asset`
   - All Weapons → Add element
   - Kéo `LightningWeaponData` vào

4. **Tạo UpgradeData:**
   - `Upgrade_Lightning_Add` (Add mode)
   - `Upgrade_Lightning` (Upgrade mode, targetWeaponName = "lightning")

5. **Test:**
   - Play game
   - Lên level 5 → Lightning xuất hiện (đã đủ level)
   - Chỉ hiện nếu đã có Fireball (required weapon)

**XONG!** 🎉

---

## 🐛 **TROUBLESHOOTING**

### **❌ "WeaponDatabase not found in Resources folder!"**
- **Nguyên nhân:** Database asset không ở trong `Assets/Resources/`
- **Fix:** Di chuyển `WeaponDatabase.asset` vào folder `Assets/Resources/`

### **❌ Weapon stats không load từ WeaponData**
- **Nguyên nhân:** Chưa kéo WeaponData vào prefab
- **Fix:** Mở prefab → Inspector → Weapon Data field → Kéo asset vào

### **❌ "Duplicate weapon ID: 'fireball' in WeaponDatabase!"**
- **Nguyên nhân:** 2 WeaponData có cùng weaponId
- **Fix:** Đổi 1 trong 2 IDs thành unique value

### **❌ Target weapon name không khớp**
- **Nguyên nhân:** UpgradeData.targetWeaponName ≠ WeaponData.weaponId
- **Fix:** Check chính tả, case-sensitive (dùng lowercase)

---

## 🎨 **BEST PRACTICES**

### **Naming Convention:**
```
WeaponData assets:    FireballWeaponData, LaserWeaponData
Weapon prefabs:       FireballWeapon.prefab, LaserWeapon.prefab
Projectiles:          FireballProjectile.prefab
UpgradeData Add:      Upgrade_Fireball_Add
UpgradeData Upgrade:  Upgrade_Fireball
```

### **Weapon IDs:**
- Dùng **lowercase + underscore**: `fireball`, `lightning_bolt`, `mega_laser`
- Không dùng dấu cách, ký tự đặc biệt
- Auto format trong `OnValidate()`

### **Stat Balancing:**
```
Early game (Level 1-3):
  - Damage: 10-15
  - Cooldown: 1.5-2.5s
  
Mid game (Level 4-7):
  - Damage: 20-30
  - Cooldown: 2.0-3.0s
  
Late game (Level 8+):
  - Damage: 40+
  - Cooldown: 3.0-5.0s
  - Required weapon: Mid game weapon
```

---

## 🚀 **FUTURE EXTENSIONS**

### **Weapon Evolution:**
```csharp
[Header("Evolution")]
public WeaponData evolvesInto;  // Fireball → MegaFireball at level 5
public int evolutionLevel = 5;
```

### **Weapon Synergies:**
```csharp
[Header("Synergies")]
public WeaponData[] synergyWeapons;  // Fireball + Lightning = Plasma
public GameObject synergyPrefab;
```

### **Weapon Rarity:**
```csharp
public enum Rarity { Common, Rare, Epic, Legendary }
public Rarity rarity = Rarity.Common;
```

---

## 📞 **SUPPORT**

Nếu có vấn đề:
1. Check Console logs
2. Verify folder structure đúng
3. Check WeaponDatabase.asset có trong Resources/
4. Verify weaponId khớp giữa WeaponData và UpgradeData

**Last Updated:** March 2026  
**Version:** 2.0 - ScriptableObject System
