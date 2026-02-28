# ⚡ Quick Reference - Upgrade System

## 🎯 **TẠO UPGRADE CHO WEAPON MỚI (3 BƯỚC)**

### **1. Đã có WeaponData? Bỏ qua, chuyển bước 2**
```
Nếu chưa:
→ Create → Weapons → Weapon Data
→ Config weapon stats
→ Add vào WeaponDatabase
```

### **2. Tạo UpgradeData Add (Thêm weapon)**
```
Location: Assets/QuangVinh/Upgrades/Weapons/

→ Right Click → Create → Upgrades → Upgrade Data
→ Rename: "WeaponName_Add"
→ Upgrade Type: Weapon
→ Weapon Data: [KÉO WEAPONDATA VÀO]  ← CHỈ CẦN BƯỚC NÀY
→ Weapon Mode: Add
→ Save

✅ Unity auto fill: displayName, description, icon, prefab
```

### **3. Tạo UpgradeData Upgrade (Nâng cấp weapon)**
```
Location: Assets/QuangVinh/Upgrades/Weapons/

→ Right Click → Create → Upgrades → Upgrade Data
→ Rename: "WeaponName_Upgrade"
→ Upgrade Type: Weapon
→ Weapon Data: [KÉO WEAPONDATA VÀO]  ← CHỈ CẦN BƯỚC NÀY
→ Weapon Mode: Upgrade
→ Save

✅ Unity auto fill: displayName, description, icon, targetWeaponName
```

### **4. Add vào ExperienceManager**
```
→ Hierarchy → GameObject với ExperienceManager
→ Inspector → Upgrade Datas array
→ Kéo 2 UpgradeData vừa tạo vào
→ Save Scene
```

**XONG! Chỉ cần kéo WeaponData vào, còn lại tự động!** 🎉

---

## 🎨 **TẠO UPGRADE CHO STAT (2 BƯỚC)**

### **1. Tạo UpgradeData Stat**
```
Location: Assets/QuangVinh/Upgrades/Stats/

→ Right Click → Create → Upgrades → Upgrade Data
→ Rename: "Speed_Boost"
→ Upgrade Type: Stat  ← QUAN TRỌNG

→ Basic Info:
    Display Name: "Speed Boost"
    Description: "+20% Movement Speed"
    Icon: [icon sprite]

→ Stat Buffs:
    Size: 1
    Element 0:
      Effect Name: "baseMoveSpeed"
      Effect Value: 0.2
      Is Percentage: ✓

→ Save
```

### **2. Add vào ExperienceManager**
```
→ Kéo vào Upgrade Datas array
→ Save Scene
```

**XONG!**

---

## 📁 **FOLDER STRUCTURE**

```
Assets/
├── ChiTan/Weapon/Configs/     ← WeaponData assets
├── QuangVinh/Upgrades/
│   ├── Weapons/               ← Weapon UpgradeData
│   └── Stats/                 ← Stat UpgradeData
└── Resources/
    └── WeaponDatabase.asset   ← Database
```

---

## 📝 **NAMING CONVENTION**

```
WeaponData:       FireballWeaponData.asset
UpgradeData Add:  Fireball_Add.asset
UpgradeData Upgrade: Fireball_Upgrade.asset
Stat Upgrade:     Speed_Boost.asset
```

---

## 🔥 **KEY BENEFITS**

✅ **Không duplicate config** - WeaponData làm source of truth  
✅ **Auto fill** - Chỉ kéo WeaponData vào  
✅ **Dynamic UI** - "Fireball II", "III" tự động  
✅ **Nhanh gấp 2** - 4 phút thay vì 8 phút  
✅ **Không lỗi** - Unity tự validate  

---

## 🐛 **COMMON MISTAKES**

❌ Quên set **Upgrade Type = Weapon**  
❌ Quên set **Weapon Mode** (Add/Upgrade)  
❌ Không kéo **WeaponData** vào  
❌ Quên add vào **ExperienceManager array**  

---

## 💡 **PRO TIPS**

1. **Luôn dùng WeaponData** - Không config thủ công
2. **Tổ chức theo folder** - Weapons riêng, Stats riêng
3. **Naming consistent** - WeaponName_Add, WeaponName_Upgrade
4. **Test ngay** - Play game, level up xem có hiện không

---

## 🎯 **CHECKLIST**

### Thêm weapon mới:
- [ ] Tạo/có WeaponData
- [ ] Tạo UpgradeData Add, kéo WeaponData vào
- [ ] Tạo UpgradeData Upgrade, kéo WeaponData vào
- [ ] Add 2 upgrades vào ExperienceManager
- [ ] Test: Level up xem weapon có hiện không
- [ ] Test: Chọn weapon, verify add thành công
- [ ] Test: Level up lại, chọn upgrade, verify stats tăng

### Thêm stat upgrade:
- [ ] Tạo UpgradeData với Upgrade Type = Stat
- [ ] Config Stat Buffs array
- [ ] Add vào ExperienceManager
- [ ] Test: Chọn upgrade, verify stat tăng

---

**Version:** 2.0  
**Last Updated:** March 2026
