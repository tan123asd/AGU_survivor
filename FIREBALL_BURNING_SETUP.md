# 🔥 FIREBALL BURNING EFFECT - Setup Guide

## 🎯 **HIỆU ỨNG**

Fireball bây giờ gây **BURNING EFFECT** cho enemies:
- **1 damage mỗi giây**
- **Tồn tại 5 giây**
- Nếu enemy bị hit nhiều lần → Refresh lại thời gian

---

## ⚡ **SETUP NHANH (3 BƯỚC)**

### **Bước 1: Tạo Fireball Projectile Prefab**

1. Tìm fireball projectile prefab của bạn (ví dụ: `FireBallProjectile.prefab`)
2. Mở prefab trong Inspector
3. **Remove** component `Projectile`
4. **Add Component** → `FireballProjectile`
5. Config settings:
   - **Burn Damage Per Second:** 1
   - **Burn Duration:** 5
6. Save prefab

### **Bước 2: Link Projectile vào FireballWeapon**

1. Tìm FireBall weapon object/prefab trong Scene hoặc Prefabs folder
2. Mở Inspector
3. Tìm component **FireballWeapon** (hoặc ProjectileWeapon)
4. Trong **Projectile Settings**:
   - **Projectile Prefab:** Kéo FireballProjectile prefab vào đây
5. Save

### **Bước 3: Test**

1. Play game
2. Lấy Fireball weapon
3. Bắn vào enemy
4. **Kiểm tra Console:**
   - `🔥 Applied burning effect to Enemy!`
   - `🔥 Burning damage: 1` (mỗi giây)
   - Enemy sẽ mất máu dần trong 5 giây!

---

## 📁 **FILES ĐÃ TẠO**

```
Assets/ChiTan/Weapon/
├── Effects/
│   └── BurningEffect.cs         ✅ Component gây damage theo thời gian
├── Projectiles/
│   └── FireballProjectile.cs    ✅ Projectile thêm burning effect khi hit
└── Scripts/
    └── FireballWeapon.cs        ✅ Updated với burning info
```

---

## 🔧 **TECHNICAL DETAILS**

### **BurningEffect.cs**
```csharp
// Component tự động gây damage mỗi giây
public class BurningEffect : MonoBehaviour
{
    - damagePerTick = 1
    - tickInterval = 1s
    - duration = 5s
    
    Update() {
        Every 1 second → TakeDamage(1)
        After 5 seconds → Destroy(this)
    }
}
```

### **FireballProjectile.cs**
```csharp
// Kế thừa Projectile, override OnHitEnemy()
public class FireballProjectile : Projectile
{
    OnHitEnemy(enemy) {
        if (enemy has BurningEffect)
            → Refresh duration (reset về 5s)
        else
            → Add new BurningEffect
    }
}
```

### **Projectile.cs (Updated)**
```csharp
// Base class thêm virtual method
OnTriggerEnter2D(enemy) {
    enemy.TakeDamage(damage);
    OnHitEnemy(enemy); // ← NEW! Subclass override được
    Destroy(this);
}
```

---

## 🎨 **CUSTOMIZE**

### **Thay đổi damage/duration:**

Mở FireballProjectile prefab → Inspector:
- **Burn Damage Per Second:** Tăng lên (ví dụ: 2)
- **Burn Duration:** Tăng lên (ví dụ: 10)

### **Thêm visual effect:**

1. Tạo fire particle prefab (ví dụ: `FireParticle.prefab`)
2. Mở BurningEffect.cs
3. Line 15: `[SerializeField] private GameObject fireParticlePrefab;`
4. Trong prefab inspector → kéo FireParticle vào field này
5. Effect sẽ tự spawn và theo enemy!

### **Tăng damage theo level:**

Mở FireballWeapon.cs, thêm vào `OnUpgrade()`:
```csharp
protected override void OnUpgrade()
{
    // Existing code...
    
    // Tăng burn damage theo level
    // burnDamagePerSecond = weaponLevel; // Level 1=1dmg, Level 5=5dmg
}
```

---

## 🐛 **TROUBLESHOOTING**

### **❌ Enemy không bị burning:**
- ✅ Check: Projectile prefab có dùng `FireballProjectile.cs`?
- ✅ Check: Enemy có tag "Enemy"?
- ✅ Check: Console có log "Applied burning effect"?

### **❌ Burning damage không giảm máu:**
- ✅ Check: Enemy có implement `IDamageable`?
- ✅ Check: Enemy.TakeDamage() có hoạt động?
- ✅ Check: Console có log "Burning damage: 1"?

### **❌ Effect không refresh:**
- ✅ Check: FireballProjectile.OnHitEnemy() có gọi RefreshDuration()?
- ✅ Check: Enemy còn sống khi bị hit lần 2?

---

## 📊 **DEMO SCENARIO**

```
Time  Event                          Enemy HP
----  ---------------------------    ---------
0s    Fireball hits enemy            100 HP
0s    Instant damage: -10            90 HP
0s    Burning effect applied         90 HP
1s    Burning tick #1: -1            89 HP
2s    Burning tick #2: -1            88 HP
2.5s  Fireball hits again!           88 HP
2.5s  Instant damage: -10            78 HP
2.5s  Burning REFRESHED (reset 5s)   78 HP
3.5s  Burning tick #3: -1            77 HP
4.5s  Burning tick #4: -1            76 HP
5.5s  Burning tick #5: -1            75 HP
6.5s  Burning tick #6: -1            74 HP
7.5s  Burning tick #7: -1            73 HP
7.5s  Burning effect ends            73 HP
```

**Total damage from 2 fireballs:**
- Instant: 20 HP (10 + 10)
- Burning: 7 HP (1/sec × 7 ticks)
- **Total: 27 HP**

---

## ✅ **SUMMARY**

✅ **Burning Effect = 1 dmg/sec × 5 seconds**  
✅ **Multi-hit = Refresh duration**  
✅ **Setup chỉ 3 bước (< 5 phút)**  
✅ **Customize dễ dàng**  
✅ **Visual effects optional**  

**Fireball bây giờ mạnh hơn nhiều!** 🔥

---

**Version:** 1.0  
**Last Updated:** March 2026  
**Complexity:** ⭐⭐ (Easy)
