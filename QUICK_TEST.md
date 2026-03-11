# ⚡ Quick Test Guide - 5 Phút

## 🎯 **TEST NHANH - Follow theo thứ tự**

### **SETUP (1 phút)**

```
1. WeaponDatabase
   → Assets/Resources/ → Create → Weapons → Weapon Database
   → Rename: "WeaponDatabase"
   ✓ Done

2. FireballWeaponData
   → Assets/ChiTan/Weapon/Configs/ → Create → Weapons → Weapon Data
   → Rename: "FireballWeaponData"
   → Config: weaponId="fireball", damage=10, cooldown=1.5
   → Kéo FireBall.prefab vào Weapon Prefab field
   ✓ Done

3. Add to Database
   → Mở WeaponDatabase.asset
   → All Weapons → Size = 1
   → Element 0 = FireballWeaponData
   ✓ Done
```

---

### **UPGRADE DATA (2 phút)**

```
4. Fireball_Add
   → Assets/QuangVinh/Upgrades/Weapons/ → Create → Upgrades → Upgrade Data
   → Rename: "Fireball_Add"
   → Upgrade Type: Weapon
   → Weapon Data: [KÉO FireballWeaponData VÀO]
   → Weapon Mode: Add
   → Save → Verify: displayName, icon, prefab auto-filled
   ✓ Done

5. Fireball_Upgrade
   → Same folder → Create → Upgrades → Upgrade Data
   → Rename: "Fireball_Upgrade"
   → Upgrade Type: Weapon
   → Weapon Data: [KÉO FireballWeaponData VÀO]
   → Weapon Mode: Upgrade
   → Save → Verify: targetWeaponName = "fireball"
   ✓ Done

6. Add to ExperienceManager
   → Hierarchy → Find ExperienceManager
   → Inspector → Upgrade Datas array → Tăng size
   → Kéo Fireball_Add và Fireball_Upgrade vào
   → Save Scene
   ✓ Done
```

---

### **PLAY TEST (2 phút)**

```
7. Enter Play Mode
   → Press Ctrl+P
   ✓ Game running

8. Level Up (Kill enemies hoặc cheat)
   → UI xuất hiện
   → Verify: Có option "Fireball" với icon
   ✓ UI OK

9. Chọn "Fireball"
   → Click button
   → Check Console:
     ✅ "✅ Added weapon: FireBall"
     ✅ "fireball Lv.1: Damage=10, Cooldown=1.50s"
   → Check Gameplay: Weapon shoots projectiles
   ✓ Add OK

10. Level Up lần 2
    → UI xuất hiện
    → Verify: Có option "Fireball II" (not "Fireball")
    ✓ Dynamic UI OK

11. Chọn "Fireball II"
    → Click button
    → Check Console:
      ✅ "✨ Upgraded fireball to Level 2"
      ✅ "fireball Lv.2: Damage=11, Cooldown=1.42s"
    → Check Gameplay: Attack faster
    ✓ Upgrade OK
```

---

## ✅ **PASS CRITERIA**

```
✓ WeaponDatabase created in Resources/
✓ WeaponData auto-fills UpgradeData
✓ Level up shows "Fireball"
✓ Add weapon works (console log + gameplay)
✓ Level up shows "Fireball II" (not "Fireball")
✓ Upgrade works (stats increase)
```

---

## ❌ **COMMON FAILURES**

### **Auto-fill không work:**
```
→ Click UpgradeData file lại
→ Right Click → Reimport
→ Restart Unity
```

### **"Weapon not found":**
```
→ Check Player có WeaponController component
→ Check weaponName trong prefab = "fireball"
→ Check targetWeaponName trong UpgradeData = "fireball"
```

### **UI không hiện weapon:**
```
→ Check Upgrade Datas array có elements
→ Check Player chưa có weapon (cho Add mode)
→ Check Player đã có weapon (cho Upgrade mode)
```

---

## 🎯 **NEXT STEPS**

```
✓ Test pass → Add thêm weapons (Laser, Lightning...)
✓ Test fail → Check TESTING_GUIDE.md (detailed debug)
```

---

**Thời gian:** 5 phút  
**Tests:** 11 bước  
**Coverage:** Core flow
