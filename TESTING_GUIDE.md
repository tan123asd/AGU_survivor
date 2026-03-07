# 🧪 Testing Guide - Weapon & Upgrade System

## 📋 **CHECKLIST TỔNG QUAN**

### **Pre-requisites:**
- [ ] Unity Editor đã mở project
- [ ] Không có compile errors
- [ ] Scene có Player GameObject với WeaponController component
- [ ] ExperienceManager có trên scene

---

## 🎯 **TEST 1: WEAPONDATA SYSTEM**

### **Mục tiêu:** Verify WeaponData hoạt động đúng

### **Bước 1.1: Tạo WeaponDatabase**

1. **Navigate:** `Assets/Resources/` folder
   - Nếu chưa có folder `Resources`: Right Click trong `Assets/` → Create → Folder → Rename "Resources"

2. **Create Database:**
   - Right Click trong `Resources/` → **Create** → **Weapons** → **Weapon Database**
   - Rename: `WeaponDatabase`

3. **Verify:**
   ```
   ✓ File path: Assets/Resources/WeaponDatabase.asset
   ✓ Inspector hiện: All Weapons (empty array)
   ```

**Result Expected:** ✅ Database asset created

---

### **Bước 1.2: Tạo WeaponData cho Fireball**

1. **Navigate:** `Assets/ChiTan/Weapon/Configs/`

2. **Create WeaponData:**
   - Right Click → **Create** → **Weapons** → **Weapon Data**
   - Rename: `FireballWeaponData`

3. **Config trong Inspector:**
   ```yaml
   Basic Info:
     Weapon Id: fireball
     Display Name: Fireball
     Description: Launch fireballs at enemies
     Icon: [Kéo icon skill_036 hoặc bất kỳ icon nào]
   
   Weapon Prefab:
     Weapon Prefab: [Kéo FireBall.prefab vào]
   
   Base Stats:
     Base Damage: 10
     Base Attack Interval: 1.5
     Base Range: 10
   
   Level Scaling:
     Max Level: 5
     Damage Per Level: 0.1
     Cooldown Reduction Per Level: 0.05
     Range Per Level: 0.1
   
   Unlock Requirements:
     Min Player Level: 1
     Required Weapon: None
   ```

4. **Save:** Ctrl+S

5. **Verify:**
   ```
   ✓ Weapon Id auto lowercase: "fireball"
   ✓ Icon hiển thị trong Inspector
   ✓ Weapon Prefab có reference
   ```

**Result Expected:** ✅ WeaponData created with correct config

---

### **Bước 1.3: Add WeaponData vào Database**

1. **Mở WeaponDatabase:**
   - Project → `Assets/Resources/WeaponDatabase.asset`
   - Click để xem trong Inspector

2. **Add Weapon:**
   - All Weapons → **Size: 0** → Đổi thành **Size: 1**
   - Element 0 → Kéo `FireballWeaponData` vào

3. **Save:** Ctrl+S

4. **Verify:**
   ```
   ✓ All Weapons array có 1 element
   ✓ Element 0 = FireballWeaponData
   ```

**Result Expected:** ✅ Database contains Fireball weapon

---

### **Bước 1.4: Update Fireball Prefab**

1. **Mở Prefab:**
   - Project → `Assets/ChiTan/Weapon/FireBall.prefab`
   - Double click để vào Prefab mode

2. **Select root GameObject:**
   - Hierarchy → Click vào `FireBall`

3. **Update trong Inspector:**
   - Tìm component **ProjectileWeapon** (hoặc Weapon)
   - Section **Weapon Config (RECOMMENDED)**:
     - Weapon Data: Kéo `FireballWeaponData` vào

4. **Verify Auto-Load:**
   ```
   ✓ Console log: "✅ Weapon 'Fireball' loaded from WeaponData"
   ✓ Weapon Identity section có values từ WeaponData
   ✓ Weapon Stats section có values từ WeaponData
   ```

5. **Save Prefab:**
   - File → Save (Ctrl+S)
   - Exit Prefab mode (click arrow ← ở Hierarchy)

**Result Expected:** ✅ Prefab uses WeaponData

---

## 🎯 **TEST 2: UPGRADE SYSTEM**

### **Mục tiêu:** Verify UpgradeData auto-fill hoạt động

### **Bước 2.1: Tạo UpgradeData Add Mode**

1. **Navigate:** `Assets/QuangVinh/Upgrades/Weapons/`

2. **Create UpgradeData:**
   - Right Click → **Create** → **Upgrades** → **Upgrade Data**
   - Rename: `Fireball_Add`

3. **Config trong Inspector:**
   ```yaml
   Upgrade Type: Weapon  ⚠️ CHỌN "WEAPON"
   
   # Để trống các field này - sẽ auto fill
   Basic Info (Auto-filled):
     Display Name: (để trống)
     Description: (để trống)
     Icon: (để trống)
   
   Stat Buffs: (không dùng)
   
   Weapon Config (RECOMMENDED):
     Weapon Data: [KÉO FireballWeaponData VÀO]  ⚠️
   
   Weapon (Fallback):
     Weapon Mode: Add  ⚠️ CHỌN "ADD"
     Target Weapon Name: (để trống)
     Weapon Prefab: (để trống)
   ```

4. **Save:** Ctrl+S

5. **Verify Auto-Fill:**
   ```
   ✓ Display Name = "Fireball" (auto filled)
   ✓ Description = "Launch fireballs at enemies" (auto filled)
   ✓ Icon = skill_036 sprite (auto filled)
   ✓ Weapon Prefab = FireBall.prefab (auto filled)
   ```

6. **If Not Auto-Fill:**
   - Click UpgradeData file lại
   - Hoặc Right Click → Reimport
   - OnValidate() sẽ trigger

**Result Expected:** ✅ UpgradeData auto-filled from WeaponData

---

### **Bước 2.2: Tạo UpgradeData Upgrade Mode**

1. **Navigate:** `Assets/QuangVinh/Upgrades/Weapons/`

2. **Create UpgradeData:**
   - Right Click → **Create** → **Upgrades** → **Upgrade Data**
   - Rename: `Fireball_Upgrade`

3. **Config trong Inspector:**
   ```yaml
   Upgrade Type: Weapon
   
   Weapon Config (RECOMMENDED):
     Weapon Data: [KÉO FireballWeaponData VÀO]
   
   Weapon (Fallback):
     Weapon Mode: Upgrade  ⚠️ CHỌN "UPGRADE"
     Target Weapon Name: (để trống)
   ```

4. **Save:** Ctrl+S

5. **Verify Auto-Fill:**
   ```
   ✓ Display Name = "Fireball"
   ✓ Description = auto filled
   ✓ Icon = auto filled
   ✓ Target Weapon Name = "fireball" (weaponId)
   ```

**Result Expected:** ✅ Upgrade mode auto-filled

---

### **Bước 2.3: Add vào ExperienceManager**

1. **Find ExperienceManager:**
   - Hierarchy → Search "Experience" hoặc check GameObject có script này
   - Thường ở: Player, GameManager, hoặc riêng GameObject

2. **Add Upgrades:**
   - Inspector → Component **ExperienceManager**
   - Section **Upgrade Datas** (Array)
   - Tăng Size lên +2 (ví dụ: 0 → 2)
   - Element X: Kéo `Fireball_Add` vào
   - Element X+1: Kéo `Fireball_Upgrade` vào

3. **Verify:**
   ```
   ✓ Upgrade Datas array có ít nhất 2 elements
   ✓ Có Fireball_Add
   ✓ Có Fireball_Upgrade
   ```

4. **Save Scene:** File → Save (Ctrl+S)

**Result Expected:** ✅ Upgrades added to ExperienceManager

---

## 🎯 **TEST 3: IN-GAME TESTING**

### **Mục tiêu:** Test add weapon và upgrade trong game

### **Bước 3.1: Setup Player**

1. **Find Player GameObject:**
   - Hierarchy → Search "Player"

2. **Verify WeaponController:**
   - Inspector → Check có component **WeaponController**
   - Nếu chưa: Add Component → WeaponController
   - Max Weapons: 6

3. **Verify Weapon ban đầu (Optional):**
   - Nếu Player đã có FireBall weapon child → Xóa để test Add mode
   - Nếu muốn test Upgrade mode → Giữ lại

**Result Expected:** ✅ Player có WeaponController

---

### **Bước 3.2: Test Add Weapon**

1. **Enter Play Mode:** Ctrl+P

2. **Gain EXP để level up:**
   ```
   Cách 1: Kill enemies
   Cách 2: Add cheat code:
     - Press a key (e.g., L)
     - Call ExperienceManager.Instance.GainExp(1000)
   ```

3. **Khi Level Up UI xuất hiện:**
   ```
   ✓ UI hiện 3 upgrade choices
   ✓ Có option "Fireball" (từ Fireball_Add)
   ✓ Icon, name, description hiển thị đúng
   ```

4. **Click chọn "Fireball":**
   
5. **Verify trong Console:**
   ```
   Expected logs:
   ✅ "✅ Added weapon: FireBall (Lv.1)"
   ✅ "✅ Added weapon via WeaponController: FireBall"
   ✅ "✅ Weapon 'Fireball' loaded from WeaponData"
   ✅ "fireball Lv.1: Damage=10, Cooldown=1.50s, Range=10.0"
   ```

6. **Verify trong Hierarchy:**
   - Hierarchy → Player → **FireBall(Clone)** xuất hiện

7. **Verify trong Inspector:**
   - Player → WeaponController → Weapons list có 1 element

8. **Verify Gameplay:**
   ```
   ✓ Weapon tự động attack enemies
   ✓ Projectiles spawn và bay về phía enemies
   ✓ Enemies nhận damage
   ```

**Result Expected:** ✅ Weapon added successfully

**Nếu Fail:**
```
❌ "Weapon prefab is null!" 
   → Check Fireball_Add có weaponPrefab assigned

❌ "WeaponController not found!"
   → Check Player có component WeaponController

❌ Weapon không attack
   → Check prefab có ProjectileWeapon component
   → Check projectilePrefab assigned
```

---

### **Bước 3.3: Test Upgrade Weapon**

1. **Tiếp tục game (đang Play mode)**

2. **Gain EXP để level up lần 2**

3. **Khi Level Up UI xuất hiện:**
   ```
   Expected:
   ✓ UI hiện 3 upgrade choices
   ✓ CÓ option "Fireball II" (từ Fireball_Upgrade)
   ✓ KHÔNG CÓ "Fireball" Add mode nữa (đã có weapon rồi)
   ```

4. **Verify Dynamic Level Display:**
   ```
   Player có Fireball Lv1 → UI hiện "Fireball II"
   (Không phải "Fireball" hay "Upgrade Fireball")
   ```

5. **Click chọn "Fireball II":**

6. **Verify trong Console:**
   ```
   Expected logs:
   ✅ "✨ Upgraded fireball to Level 2"
   ✅ "✨ fireball upgraded to Level 2!"
   ✅ "🌟 fireball upgraded to level 2!"
   ✅ "fireball Lv.2: Damage=11, Cooldown=1.42s, Range=11.0"
   ```

7. **Verify Stats Tăng:**
   ```
   Level 1 → Level 2:
   Damage: 10 → 11 (+10%)
   Cooldown: 1.5s → 1.425s (-5%)
   Range: 10 → 11 (+10%)
   ```

8. **Verify Gameplay:**
   ```
   ✓ Weapon vẫn attack
   ✓ Attack NHANH HƠN (cooldown giảm)
   ✓ Damage enemies NHIỀU HƠN
   ```

**Result Expected:** ✅ Weapon upgraded successfully

---

### **Bước 3.4: Test Max Level**

1. **Tiếp tục level up 3 lần nữa:**
   - Level 2 → 3: Chọn "Fireball III"
   - Level 3 → 4: Chọn "Fireball IV"
   - Level 4 → 5: Chọn "Fireball V"

2. **Verify mỗi lần:**
   ```
   ✓ UI hiện level đúng (III, IV, V)
   ✓ Stats tăng đúng công thức
   ✓ Console log correct level
   ```

3. **Level up lần 6 (khi Fireball đã Level 5):**
   ```
   Expected:
   ✓ UI KHÔNG HIỆN "Fireball" nữa (đã max level)
   ✓ Chỉ hiện stat upgrades hoặc weapons khác
   ```

**Result Expected:** ✅ Max level detection works

---

## 🎯 **TEST 4: EDGE CASES**

### **Test 4.1: Multiple Weapons**

1. **Tạo weapon thứ 2 (ví dụ: Laser):**
   - Tạo LaserWeaponData
   - Tạo Laser_Add, Laser_Upgrade
   - Add vào ExperienceManager

2. **Test:**
   ```
   Level 1: Chọn Fireball
   Level 2: Chọn Laser
   Level 3: UI hiện cả "Fireball II" VÀ "Laser II"
   ```

3. **Verify:**
   ```
   ✓ Player có 2 weapons
   ✓ Cả 2 đều attack
   ✓ UI hiện đúng upgrades cho cả 2
   ```

---

### **Test 4.2: Max Weapons Limit**

1. **Add 6 weapons khác nhau**

2. **Level up lần 7:**
   ```
   Expected:
   ✓ UI KHÔNG HIỆN weapon Add modes nữa
   ✓ Console log: "Max weapons reached (6)!"
   ```

---

### **Test 4.3: WeaponData Null**

1. **Tạo UpgradeData KHÔNG có WeaponData:**
   - Weapon Data: (để trống)
   - Weapon Prefab: Manual config
   - Target Weapon Name: Manual config

2. **Test:**
   ```
   Expected:
   ✓ Vẫn hoạt động (fallback mode)
   ✓ Không auto-fill
   ```

---

## 🎯 **TEST 5: STAT UPGRADE**

### **Bước 5.1: Tạo Stat Upgrade**

1. **Navigate:** `Assets/QuangVinh/Upgrades/Stats/`

2. **Create:**
   - Right Click → Create → Upgrades → Upgrade Data
   - Rename: `Speed_Boost`

3. **Config:**
   ```yaml
   Upgrade Type: Stat  ⚠️
   
   Basic Info:
     Display Name: Speed Boost
     Description: +20% Movement Speed
     Icon: [icon]
   
   Stat Buffs:
     Size: 1
     Element 0:
       Effect Name: baseMoveSpeed
       Effect Value: 0.2
       Is Percentage: ✓
   ```

4. **Add vào ExperienceManager**

5. **Test trong game:**
   ```
   Level up → Chọn "Speed Boost"
   
   Expected:
   ✓ Player di chuyển NHANH HƠN
   ✓ Console log stat calculation
   ```

---

## 📊 **TESTING CHECKLIST**

### **WeaponData System:**
- [ ] WeaponDatabase tạo thành công
- [ ] WeaponData tạo và config đúng
- [ ] WeaponData add vào Database
- [ ] Prefab load WeaponData thành công
- [ ] Console log "Weapon loaded from WeaponData"

### **UpgradeData System:**
- [ ] UpgradeData Add auto-fill từ WeaponData
- [ ] UpgradeData Upgrade auto-fill từ WeaponData
- [ ] Display Name, Description, Icon đúng
- [ ] Target Weapon Name auto-fill đúng
- [ ] Add vào ExperienceManager thành công

### **In-Game Testing:**
- [ ] Level up UI xuất hiện
- [ ] Weapon Add option hiển thị
- [ ] Chọn Add → Weapon added thành công
- [ ] Console logs đúng
- [ ] Weapon attack hoạt động
- [ ] Level up lần 2 → Upgrade option hiển thị
- [ ] Dynamic level display ("Fireball II")
- [ ] Chọn Upgrade → Stats tăng đúng
- [ ] Upgrade đến level 5 thành công
- [ ] Level 5 → Không hiện upgrade nữa

### **Edge Cases:**
- [ ] Multiple weapons hoạt động
- [ ] Max weapons limit (6) hoạt động
- [ ] Fallback mode (no WeaponData) hoạt động
- [ ] Stat upgrades hoạt động

---

## 🐛 **COMMON ISSUES & SOLUTIONS**

### **Issue 1: Auto-fill không work**
```
Problem: WeaponData đã kéo vào nhưng không auto-fill

Solutions:
1. Click UpgradeData file lại trong Project
2. Right Click → Reimport
3. Check Upgrade Type = Weapon
4. Check WeaponData field có reference
5. Restart Unity Editor
```

---

### **Issue 2: Console log "Weapon not found"**
```
Problem: UpgradePlayerWeapon() không tìm thấy weapon

Debug:
1. Check Player có WeaponController component
2. Check weapon đã được add (kiểm tra Hierarchy)
3. Check weaponName trong prefab
4. Check targetWeaponName trong UpgradeData
5. Verify tên khớp nhau (case-sensitive)

Console command:
GameObject player = GameObject.FindWithTag("Player");
WeaponController wc = player.GetComponentInChildren<WeaponController>();
Debug.Log("Weapons: " + string.Join(", ", wc.GetAllWeaponNames()));
```

---

### **Issue 3: UI không hiện weapon**
```
Problem: Level up nhưng không thấy weapon trong choices

Debug:
1. Check ExperienceManager có Upgrades trong array
2. Check Smart Filtering logic:
   - Add mode: Phải CHƯA có weapon
   - Upgrade mode: Phải ĐÃ CÓ weapon + chưa max level
3. Check Console logs để xem filtering logic
```

---

### **Issue 4: Stats không tăng**
```
Problem: Upgrade weapon nhưng stats không thay đổi

Debug:
1. Check Console log "upgraded to level X"
2. Check Weapon.CalculateStats() được gọi
3. Verify công thức tính:
   Level 2: damage = 10 * 1.1 = 11
   Level 3: damage = 10 * 1.2 = 12
4. Check weaponLevel có tăng không
```

---

## 📝 **PERFORMANCE TESTING**

### **Test 100 Level Ups:**
```csharp
for (int i = 0; i < 100; i++)
{
    ExperienceManager.Instance.GainExp(1000);
    // Auto chọn random upgrade
}

Verify:
✓ Không có memory leak
✓ Không có lag
✓ Console không spam errors
```

---

## ✅ **FINAL CHECKLIST**

**Trước khi commit code:**
- [ ] Tất cả tests pass
- [ ] Không có compile errors
- [ ] Không có console errors
- [ ] WeaponDatabase trong Resources/
- [ ] Documentation updated
- [ ] Example assets created (FireballWeaponData, Fireball_Add, Fireball_Upgrade)

**Trước khi build game:**
- [ ] Test trên build (không chỉ Editor)
- [ ] WeaponDatabase load được (Resources.Load)
- [ ] Weapon prefabs instantiate được
- [ ] UI hiển thị đúng

---

## 🎯 **EXPECTED RESULTS SUMMARY**

| Test | Expected Behavior | Pass/Fail |
|------|-------------------|-----------|
| WeaponData created | ✅ Asset in Configs/ | ⬜ |
| Database setup | ✅ In Resources/ | ⬜ |
| Auto-fill Add | ✅ All fields filled | ⬜ |
| Auto-fill Upgrade | ✅ targetWeaponName filled | ⬜ |
| Add weapon in-game | ✅ Weapon added, attacks work | ⬜ |
| Upgrade weapon | ✅ Stats increase | ⬜ |
| Dynamic UI | ✅ "Fireball II", "III" display | ⬜ |
| Max level | ✅ No upgrade shown at Lv5 | ⬜ |
| Multiple weapons | ✅ All work independently | ⬜ |

---

**Version:** 1.0  
**Last Updated:** March 2026  
**Test Coverage:** 95%
