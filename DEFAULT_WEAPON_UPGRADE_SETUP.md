# 🔫 DEFAULT WEAPON UPGRADE - Quick Setup

## 📋 **HIỆN TẠI:**
- Player có component **ProjectileWeapon**
- Weapon Name = **"DefaultWeapon"**
- Projectile Prefab = **Bullet 1**
- Không dùng WeaponData (manual config)

---

## ⚡ **SETUP UPGRADE (3 BƯỚC):**

### **BƯỚC 1: Tạo UpgradeData Asset**

```
1. Project tab → Assets/QuangVinh/Upgrades/Weapons/
2. Right click → Create → Upgrades → Upgrade Data
3. Rename: "DefaultWeapon_Upgrade"
```

### **BƯỚC 2: Config UpgradeData**

Mở file vừa tạo, config trong Inspector:

```
✅ UPGRADE TYPE:
   → Upgrade Type: Weapon

✅ BASIC INFO:
   → Display Name: "Default Gun II" (hoặc "Súng Cơ Bản II")
   → Description: "Tăng damage và giảm cooldown"
   → Icon: Kéo icon vào (sprite của weapon)

✅ WEAPON CONFIG:
   → Weapon Data: None (bỏ trống vì dùng manual)

✅ WEAPON (FALLBACK - MANUAL):
   → Weapon Mode: Upgrade ← QUAN TRỌNG!
   → Target Weapon Name: "DefaultWeapon" ← Khớp với tên trong Player
   → Weapon Prefab: None (không cần vì mode = Upgrade)
```

### **BƯỚC 3: Add vào ExperienceManager**

```
1. Scene → CHITANI → Player
2. Component "Experience Manager"
3. Upgrade Data Array → +
4. Kéo "DefaultWeapon_Upgrade" vào element mới
5. Save Scene
```

## ✅ **XONG! Test:**

```
1. Play game
2. Kill enemy → Gain EXP → Level up
3. Upgrade panel hiện "Default Gun II"
4. Chọn → Console log:
   "✨ Upgraded DefaultWeapon to Level 2"
```

---

## 🎯 **GIẢI THÍCH:**

### **Tại sao không cần prefab?**
```
Mode = Add    → Cần weapon prefab (thêm vũ khí MỚI)
Mode = Upgrade → Chỉ cần tên (upgrade vũ khí CÓ SẴN) ✅
```

### **Làm sao hệ thống tìm weapon?**
```
1. WeaponController.Start() 
   → GetComponentsInChildren<Weapon>()
   → Tìm thấy ProjectileWeapon trên Player
   → Add vào weapons list

2. UpgradePanel.ApplyUpgrade()
   → weaponController.UpgradeWeapon("DefaultWeapon")
   → Tìm weapon theo name
   → weapon.Upgrade() ← Tăng level!
```

### **Target Weapon Name phải khớp:**
```
✅ ĐÚNG:
Player → ProjectileWeapon → weaponName = "DefaultWeapon"
UpgradeData → targetWeaponName = "DefaultWeapon"
→ Khớp! Upgrade thành công!

❌ SAI:
Player → weaponName = "DefaultWeapon"
UpgradeData → targetWeaponName = "default_weapon"
→ Không khớp! Upgrade thất bại!
```

---

## 🔧 **TÙY CHỈNH NÂNG CAO:**

### **Option 1: Tạo WeaponData (Recommended)**

Nếu muốn quản lý tốt hơn:

```
1. Create → Weapons → Weapon Data
2. Config:
   - Weapon ID: "DefaultWeapon"
   - Display Name: "Default Gun"
   - Base Damage: 2 (từ Player inspector)
   - Base Attack Interval: 1
   - Base Range: 10
   - Weapon Prefab: None (không cần vì đã có sẵn trên Player)
3. Weapon Scaling:
   - Damage Multiplier: 0.1 (tăng 10% mỗi level)
   - Cooldown Reduction: 0.05 (giảm 5% cooldown)
   - Range Multiplier: 0.1 (tăng 10% range)

4. Kéo WeaponData vào Player → ProjectileWeapon → Weapon Data field
5. Kéo WeaponData vào UpgradeData → Weapon Config → Weapon Data
```

**Lợi ích:**
- Auto stats scaling theo level
- Auto display name với Roman numerals (II, III, IV)
- Tập trung config 1 chỗ

### **Option 2: Manual Scaling (Current)**

Nếu giữ nguyên không dùng WeaponData:
- Weapon.cs tự tính stats theo formula cố định
- Mỗi level: +10% damage, -5% cooldown, +10% range

---

## 📊 **LƯU Ý:**

### **Max Level:**
```
Weapon.cs inspector → Max Level: 5
→ Chỉ upgrade được đến level 5
→ Sau level 5, upgrade option sẽ không hiện nữa
```

### **Multiple Levels:**
```
Tạo nhiều UpgradeData:
- DefaultWeapon_Upgrade_2 (Level 1→2)
- DefaultWeapon_Upgrade_3 (Level 2→3)
- DefaultWeapon_Upgrade_4 (Level 3→4)
...

HOẶC dùng 1 file duy nhất (smart filtering tự động):
- ExperienceManager check weapon.CanUpgrade
- Nếu đã max level → Không show nữa
```

### **Smart Filtering:**
```csharp
// ExperienceManager.GetRandomUpgrades() tự động check:

if (mode == Upgrade) {
    if (!HasWeapon(weaponName)) {
        // Skip - chưa có weapon
    }
    if (!weapon.CanUpgrade) {
        // Skip - đã max level
    }
}
```

---

## 🐛 **TROUBLESHOOTING:**

### **❌ "Weapon 'DefaultWeapon' not found!"**
- Check: Player → ProjectileWeapon → Weapon Name = "DefaultWeapon"
- Check: UpgradeData → Target Weapon Name = "DefaultWeapon"
- Check: Đúng chính tả, case-sensitive!

### **❌ Upgrade option không hiện:**
```
1. Check Console: "Valid upgrades: X"
2. Nếu X = 0:
   - Check weapon đã max level? (Level 5/5)
   - Check UpgradeData trong ExperienceManager array
3. Nếu X > 0 nhưng không show:
   - Check UpgradePanel.ShowUpgradeOptions()
   - Check button prefab
```

### **❌ "Already max level!"**
- Weapon đã level 5
- Increase Max Level trong Weapon inspector
- Hoặc tạo upgrade option khác

---

## ✅ **CHECKLIST:**

```
□ Player có component ProjectileWeapon (hoặc Weapon subclass)
□ Weapon Name = "DefaultWeapon" (hoặc tên khác, nhớ note lại)
□ Tạo UpgradeData với Weapon Mode = Upgrade
□ Target Weapon Name khớp với weapon name trên Player
□ Add UpgradeData vào ExperienceManager → Upgrade Data array
□ Test: Level up → Chọn upgrade → Check Console log
```

---

**Version:** 1.0  
**Last Updated:** March 2026  
**Complexity:** ⭐ (Very Simple)
