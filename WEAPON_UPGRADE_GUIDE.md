# 🔫 HƯỚNG DẪN SETUP WEAPON UPGRADE SYSTEM

## ✅ **ĐÃ HOÀN THÀNH:**

1. ✅ Mở rộng **UpgradeData** để hỗ trợ weapon type
2. ✅ Cập nhật **UpgradePanel** để add weapon khi chọn
3. ✅ Tích hợp với **WeaponController** có sẵn

---

## 🎯 **CÁCH HOẠT ĐỘNG:**

```
Player nhặt EXP → Level Up
    ↓
UpgradePanel hiển thị 3 choices random
    ↓
Player chọn weapon upgrade
    ↓
WeaponController.AddWeapon() → Thêm vũ khí vào player
    ↓
Vũ khí mới bắt đầu hoạt động!
```

---

## 🔧 **SETUP TRONG UNITY (5 BƯỚC):**

### **BƯỚC 1: Tạo Weapon Prefab (nếu chưa có)**

1. Tạo Empty GameObject tên: `FireballWeapon`
2. **Add Component** → Chọn weapon script (ví dụ: `ProjectileWeapon`)
3. Setup:
   - Base Damage: 10
   - Cooldown: 2s
   - Projectile Prefab: Kéo projectile prefab vào
4. **Drag vào folder Prefabs** để tạo prefab
5. Xóa khỏi scene

---

### **BƯỚC 2: Tạo Weapon UpgradeData Asset**

1. Trong Project window (folder QuangVinh/Upgrades hoặc tạo mới)
2. Chuột phải → **Create > NewUpgrade**
3. Đặt tên: `Upgrade_FireballWeapon`
4. Chọn asset vừa tạo, trong Inspector:

```
Display Name: "Fireball"
Description: "Shoots fireballs at enemies"
Icon: [Kéo icon sprite vào]

Upgrade Type: Weapon ⚠️ (QUAN TRỌNG!)

Weapon Prefab: [Kéo FireballWeapon prefab vào] ⚠️ (QUAN TRỌNG!)
```

**LƯU Ý:** 
- **Upgrade Type** phải chọn **Weapon**
- **Weapon Prefab** phải assign

---

### **BƯỚC 3: Thêm vào ExperienceManager**

1. Chọn GameObject có **ExperienceManager** script (thường là Player)
2. Trong Inspector, component `ExperienceManager`
3. Tìm field **Upgrade Datas** (array)
4. Tăng **Size** lên (ví dụ: từ 3 → 4)
5. Kéo **Upgrade_FireballWeapon** vào slot mới

**Kết quả:** Weapon upgrade sẽ xuất hiện random khi level up!

---

### **BƯỚC 4: Kiểm tra Player có WeaponController**

1. Chọn **Player** GameObject trong Hierarchy
2. Kiểm tra có component **WeaponController** không?
   - ✅ CÓ → OK!
   - ❌ KHÔNG → Add Component → WeaponController

**Cấu trúc Player nên như sau:**
```
Player
├── PlayerHealth
├── PlayerMovement2D
├── ExperienceManager
└── WeaponController ← Phải có!
    └── (Các weapon con sẽ spawn vào đây)
```

---

### **BƯỚC 5: Test**

1. Play game
2. Nhặt EXP để level up
3. Khi UpgradePanel hiển thị:
   - Nếu có **Fireball** option → Click chọn
4. Kiểm tra:
   - Console có log: **"Added weapon: FireballWeapon to player!"**
   - Weapon mới xuất hiện dưới Player trong Hierarchy
   - Weapon bắt đầu tấn công enemy

---

## 📦 **TẠO THÊM WEAPON UPGRADES:**

### **Weapon 2: Lightning**

```
Weapon Prefab: LightningWeapon
UpgradeData:
  - Display Name: "Lightning"
  - Description: "Chain lightning strikes enemies"
  - Upgrade Type: Weapon
  - Weapon Prefab: LightningWeapon prefab
```

### **Weapon 3: Ice Blast**

```
Weapon Prefab: IceBlastWeapon
UpgradeData:
  - Display Name: "Ice Blast"
  - Description: "Freezes and damages enemies"
  - Upgrade Type: Weapon
  - Weapon Prefab: IceBlastWeapon prefab
```

---

## 🎨 **MIX STAT & WEAPON UPGRADES:**

Trong **ExperienceManager > Upgrade Datas**, bạn có thể mix:

```
Slot 0: Upgrade_MovementSpeed (Stat)
Slot 1: Upgrade_Health (Stat)
Slot 2: Upgrade_FireballWeapon (Weapon)
Slot 3: Upgrade_Damage (Stat)
Slot 4: Upgrade_LightningWeapon (Weapon)
Slot 5: Upgrade_IceBlastWeapon (Weapon)
```

→ Khi level up, random 3 trong số này sẽ hiển thị!

---

## 🔍 **TROUBLESHOOTING:**

### **Lỗi: "WeaponController not found on Player!"**

**Nguyên nhân:** Player không có WeaponController component

**Giải pháp:**
1. Chọn Player GameObject
2. Add Component → WeaponController

---

### **Lỗi: Weapon không hoạt động sau khi add**

**Nguyên nhân:** Weapon prefab thiếu setup

**Giải pháp:**
1. Mở Weapon prefab
2. Kiểm tra:
   - ✅ Có weapon script (ProjectileWeapon, MeleeWeapon...)
   - ✅ Cooldown > 0
   - ✅ Damage > 0
   - ✅ Projectile Prefab assigned (nếu là projectile weapon)

---

### **Lỗi: Weapon upgrade không hiện trong level up panel**

**Nguyên nhân:** Chưa add vào ExperienceManager

**Giải pháp:**
1. Chọn GameObject có ExperienceManager
2. Upgrade Datas → Thêm UpgradeData asset vào

---

### **Lỗi: Click weapon upgrade nhưng không có gì xảy ra**

**Kiểm tra:**
1. Console có log gì không?
2. UpgradeData **Upgrade Type** = **Weapon**?
3. UpgradeData **Weapon Prefab** đã assign?

---

## 💡 **TIPS:**

### **1. Upgrade weapon hiện có (thay vì add mới)**

Hiện tại hệ thống chỉ **add weapon mới**. Nếu muốn **upgrade weapon hiện có**:

```csharp
// Trong WeaponController.cs
public void UpgradeWeapon(string weaponName)
{
    Weapon weapon = weapons.Find(w => w.name.Contains(weaponName));
    if (weapon != null)
    {
        weapon.IncreaseDamage(5); // Cần implement method này trong Weapon
        weapon.DecreaseCooldown(0.1f);
    }
}
```

### **2. Giới hạn số weapon tối đa**

```csharp
// Trong WeaponController.cs
public int maxWeapons = 6;

public void AddWeapon(GameObject weaponPrefab)
{
    if (weapons.Count >= maxWeapons)
    {
        Debug.Log("Max weapons reached!");
        return;
    }
    // ... rest của code
}
```

### **3. Hiển thị icon weapon trong UI**

Icon trong UpgradeData sẽ tự động hiển thị trong UpgradeButtonChoice!

---

## 📋 **CHECKLIST HOÀN THÀNH:**

- [ ] ✅ Đã tạo weapon prefabs
- [ ] ✅ Đã tạo weapon UpgradeData assets
- [ ] ✅ UpgradeData có Upgrade Type = Weapon
- [ ] ✅ UpgradeData có Weapon Prefab assigned
- [ ] ✅ Đã thêm vào ExperienceManager > Upgrade Datas
- [ ] ✅ Player có WeaponController component
- [ ] ✅ Test level up và chọn weapon → Weapon hoạt động!

---

## 🎉 **KẾT QUẢ:**

Sau khi setup xong:
- ✅ Player level up → Chọn được weapon mới
- ✅ Weapon tự động add vào player
- ✅ Có thể có nhiều weapon cùng lúc
- ✅ Mỗi weapon hoạt động độc lập

**Chúc bạn thành công!** 🚀
