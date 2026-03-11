# 🎨 Custom Weapon Effects - Super Simple Guide

## 🎯 **CONCEPT: Mỗi Weapon = 1 Script**

```
Fireball → FireballWeapon.cs  → Bắn nhiều viên
Lightning → LightningWeapon.cs → Tấn công cực nhanh
Laser → LaserWeapon.cs         → Range cực xa
Bomb → BombWeapon.cs           → AOE explosion
Shield → ShieldWeapon.cs       → Defense + heal
```

**KHÔNG CẦN config nhiều!** Chỉ cần override 1-2 methods!

---

## ⚡ **WORKFLOW CỰC ĐƠN GIẢN**

### **Bước 1: Copy Template**

Tôi đã tạo sẵn 5 templates trong `Assets/ChiTan/Weapon/Scripts/`:
- `FireballWeapon.cs` ✅
- `LightningWeapon.cs` ✅
- `LaserWeapon.cs` ✅
- `BombWeapon.cs` ✅
- `ShieldWeapon.cs` ✅

### **Bước 2: Attach vào Prefab**

```
1. Mở weapon prefab (e.g., FireBall.prefab)
2. Remove component "ProjectileWeapon"
3. Add Component → FireballWeapon
4. Kéo WeaponData vào
5. Save
```

### **Bước 3: XONG!**

Weapon tự động có hiệu ứng riêng khi upgrade! 🎉

---

## 🎨 **5 WEAPON EFFECTS ĐÃ TẠO**

### **1. 🔥 FIREBALL - Multi-Projectile**

```csharp
OnUpgrade() {
    if (weaponLevel % 2 == 0) {
        // Level 2, 4 → Bắn thêm viên đạn
        // Tăng projectile count
    }
}
```

**Effect:**
- Level 1: Bắn 1 viên
- Level 2: Bắn 2 viên
- Level 4: Bắn 3 viên

---

### **2. ⚡ LIGHTNING - Ultra Speed**

```csharp
CalculateStats() {
    base.CalculateStats();
    // Extra 5% cooldown reduction per level
    attackInterval *= (1 - extraReduction);
}
```

**Effect:**
- Level 1: Cooldown 1.5s
- Level 5: Cooldown 0.9s (giảm 60% thay vì 40%)
- Tấn công NHANH NHẤT!

---

### **3. 🔫 LASER - Sniper**

```csharp
CalculateStats() {
    base.CalculateStats();
    // Extra 10% range, 5% damage per level
    range *= (1 + extraBonus);
    damage *= (1 + extraBonus);
}
```

**Effect:**
- Level 1: Range 10, Damage 10
- Level 5: Range 18, Damage 16
- Bắn XA NHẤT + damage cao!

---

### **4. 💣 BOMB - AOE Explosion**

```csharp
Attack() {
    SpawnExplosion(targetPosition);
    // Damage TẤT CẢ enemies trong radius
    
    if (weaponLevel >= 3) {
        // Bonus: Stun enemies
    }
}
```

**Effect:**
- Damage multiple enemies cùng lúc
- Level 3+: Stun enemies 0.5s
- Level 5: Stun 0.9s + huge AOE

---

### **5. 🛡️ SHIELD - Defense**

```csharp
OnUpgrade() {
    // Heal player
    playerHealth.Heal(10);
    
    // Reduce incoming damage
    damageReduction += 5%;
}
```

**Effect:**
- Mỗi upgrade: Heal 10 HP
- Giảm damage nhận vào 5% per level
- Level 5: 25% damage reduction!

---

## 🚀 **TẠO WEAPON MỚI (3 PHÚT)**

### **Ví dụ: Tạo "Frost Weapon" - Làm chậm enemies**

**Bước 1: Tạo script (Copy từ template)**

```csharp
// File: Assets/ChiTan/Weapon/Scripts/FrostWeapon.cs
using UnityEngine;

public class FrostWeapon : ProjectileWeapon
{
    [Header("Frost Special")]
    [SerializeField] private float slowAmount = 0.1f; // 10% per level
    
    protected override void Start()
    {
        weaponName = "frost";
        base.Start();
    }
    
    protected override void OnUpgrade()
    {
        Debug.Log($"❄️ FROST UPGRADED! Level {weaponLevel} - FREEZE ENEMIES!");
        
        // Effect: Enemies bị slow
        float totalSlow = weaponLevel * slowAmount;
        Debug.Log($"❄️ Enemy speed reduced: {totalSlow * 100}%");
        
        // Visual: Ice particle around player
        // ParticleEffects.PlayAt(player.position, "ice_aura");
    }
    
    // Optional: Override Attack() để thêm slow effect
    protected override void Attack()
    {
        base.Attack(); // Bắn projectile bình thường
        
        // Thêm slow effect cho enemies gần
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            float distance = Vector2.Distance(player.position, enemy.transform.position);
            if (distance < range * 0.5f) // Trong nửa range
            {
                Enemy enemyScript = enemy.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    // enemyScript.ApplySlow(slowAmount * weaponLevel, 2f);
                }
            }
        }
    }
}
```

**Bước 2: Create WeaponData**

```
Assets/ChiTan/Weapon/Configs/ → Create → Weapons → Weapon Data
→ Rename: FrostWeaponData
→ Config: weaponId="frost", displayName="Frost Bolt"
```

**Bước 3: Create Prefab**

```
1. Create Empty GameObject → Rename "FrostWeapon"
2. Add Component → FrostWeapon script
3. Weapon Data → Kéo FrostWeaponData vào
4. Projectile Prefab → Kéo ice bullet prefab vào
5. Save as prefab
```

**Bước 4: Create UpgradeData**

```
Frost_Add:
  - Weapon Data: FrostWeaponData
  - Weapon Mode: Add
  
Frost_Upgrade:
  - Weapon Data: FrostWeaponData
  - Weapon Mode: Upgrade
```

**XONG!** Frost weapon với slow effect!

---

## 💡 **CHEAT SHEET: Override Methods**

### **OnUpgrade() - Effect khi level up**
```csharp
protected override void OnUpgrade()
{
    // Chạy MỘT LẦN khi upgrade
    // Dùng cho: visual effects, sounds, stat changes
    
    Debug.Log("Weapon upgraded!");
    // PlaySound();
    // SpawnParticle();
    // HealPlayer();
    // AddBuff();
}
```

### **CalculateStats() - Custom stat scaling**
```csharp
protected override void CalculateStats()
{
    base.CalculateStats(); // Gọi base first
    
    // Modify stats theo công thức riêng
    damage *= 1.5f; // Damage tăng nhiều hơn
    range *= 2.0f;  // Range tăng gấp đôi
}
```

### **Attack() - Custom attack pattern**
```csharp
protected override void Attack()
{
    // Thay đổi CÁCH weapon attack
    
    // AOE attack
    DamageAllEnemiesInRadius(range);
    
    // Spawn special projectile
    SpawnHomingMissile();
    
    // Buff player
    BuffPlayerTemporarily();
}
```

---

## 📊 **COMPARISON: Simple vs Complex**

### **❌ CÁC CŨ (Phức tạp):**
```
Muốn thêm effect:
→ Sửa WeaponData
→ Sửa UpgradeData
→ Sửa UpgradePanel logic
→ Sửa ExperienceManager filtering
→ Test lại toàn bộ system

→ Tốn 1 giờ, dễ lỗi
```

### **✅ CÁCH MỚI (Đơn giản):**
```
Muốn thêm effect:
→ Override OnUpgrade() trong weapon script
→ Thêm 5-10 dòng code
→ Test weapon đó thôi

→ Tốn 5 phút, không lỗi
```

---

## 🎯 **EXAMPLES BY EFFECT TYPE**

### **Visual Effects:**
```csharp
OnUpgrade() {
    // Spawn particle
    GameObject particle = Instantiate(upgradeParticlePrefab, player.position, Quaternion.identity);
    Destroy(particle, 2f);
    
    // Change weapon color
    GetComponent<SpriteRenderer>().color = Color.red;
}
```

### **Sound Effects:**
```csharp
OnUpgrade() {
    // Play sound
    AudioSource.PlayClipAtPoint(upgradeSound, player.position);
}
```

### **Stat Changes:**
```csharp
OnUpgrade() {
    // Heal player
    playerHealth.Heal(20);
    
    // Buff player speed
    PlayerStats.Instance.AddModifier("baseMoveSpeed", new FlatAddModifier { Amount = 0.5f });
}
```

### **Gameplay Changes:**
```csharp
OnUpgrade() {
    // Level 5: Transform weapon
    if (weaponLevel == 5) {
        GameObject megaWeapon = Instantiate(megaWeaponPrefab, transform.parent);
        Destroy(gameObject); // Xóa weapon cũ
    }
}
```

---

## 🐛 **COMMON PATTERNS**

### **Every N Levels:**
```csharp
OnUpgrade() {
    if (weaponLevel % 2 == 0) {
        // Mỗi 2 levels (2, 4)
        AddBonusEffect();
    }
}
```

### **Threshold Check:**
```csharp
OnUpgrade() {
    if (weaponLevel >= 3) {
        // Level 3+ có effect mới
        UnlockSpecialAbility();
    }
}
```

### **Progressive Bonus:**
```csharp
OnUpgrade() {
    float bonus = weaponLevel * 0.1f; // 10% per level
    ApplyBonus(bonus);
}
```

---

## 📝 **CHECKLIST KHI TẠO WEAPON MỚI**

```
□ Copy base script (ProjectileWeapon, MeleeWeapon, hoặc Weapon)
□ Rename class và file
□ Set weaponName trong Start()
□ Override OnUpgrade() với effect riêng
□ (Optional) Override CalculateStats() nếu cần custom scaling
□ (Optional) Override Attack() nếu cần custom attack pattern
□ Create WeaponData asset
□ Create prefab với script mới
□ Test in Play mode
□ Create UpgradeData Add + Upgrade
□ Add vào ExperienceManager
```

---

## 💰 **COST COMPARISON**

| Task | Complex System | Simple System |
|------|---------------|---------------|
| Add new weapon effect | 1 hour | 5 minutes ⚡ |
| Modify existing effect | 30 min | 2 minutes ⚡ |
| Test changes | Full regression | Single weapon ⚡ |
| Bug risk | High | Low ✅ |
| Maintenance | Hard | Easy ✅ |

---

## 🎉 **SUMMARY**

✅ **Mỗi weapon = 1 script riêng**  
✅ **Override OnUpgrade() = custom effect**  
✅ **5 examples sẵn để copy**  
✅ **Thêm effect mới chỉ 5 phút**  
✅ **Không ảnh hưởng weapons khác**  
✅ **Dễ test, dễ debug, dễ maintain**  

**Không phức tạp! Chỉ cần 2 methods:**
1. `Start()` → Set weaponName
2. `OnUpgrade()` → Custom effect

**XONG!** 🚀

---

**Version:** 1.0  
**Last Updated:** March 2026  
**Complexity:** ⭐ (Very Simple)
