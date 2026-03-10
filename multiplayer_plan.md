# AGU Survivor — Photon Multiplayer Plan

## Overview

This is a **survivor-roguelite game** (top-down 2D). The project already has a complete Photon PUN2 **lobby/login/room** system in `Assets/_Data/`. The game logic scripts are in `Assets/Script/`. The goal is to **bridge these two worlds** — connect the existing lobby flow with the actual gameplay so multiple players can join, spawn, move, fight enemies, and share a game state over the network.

---

## 📁 Current Architecture — What You Have

### Photon Lobby System (`Assets/_Data/`)
| Script | Role | Status |
|---|---|---|
| `PhotonLogin.cs` | Connect to Photon master, set nickname | ✅ Done |
| `PhotonRoom.cs` | Create/Join room, room list UI | ✅ Done |
| `PhotonPlaying.cs` | In-game: load room players, spawn `PhotonPlayer`, handle leave | ✅ Done |
| `PhotonPlayer.cs` | Network player object: follows mouse, syncs via `IPunObservable` | ✅ Done (tutorial style) |
| `PhotonEventManager.cs` | Listens for Photon custom events (e.g. `onNumberClaimed`) | ✅ Done |
| `GameManager.cs` | Tutorial game manager (spawns numbers, not survivor yet) | ⚠️ Needs full rewrite for survivor |
| `PlayerProfile.cs` / `RoomProfile.cs` | Data classes | ✅ Done |

### Game Logic (`Assets/Script/`)
| Script | Role | Multiplayer Status |
|---|---|---|
| `PlayerSpawner.cs` | Spawns player prefab(s) locally | ❌ Not network-aware |
| `PlayerController.cs` | Singleton, tracks all `Player` instances, fires events | ❌ Not network-aware |
| `PlayerMovement2D.cs` | Reads keyboard input, applies velocity | ❌ No ownership check |
| `PlayerHealth.cs` | Manages HP, calls `Die()` | ❌ Not synced |
| `Enemy.cs` | Chases nearest player (by tag), damages on contact | ❌ Hard-coded to 1 player tag |
| `CameraDirector.cs` | Follows local player via `PlayerController.GetLocalPlayer()` | ⚠️ Needs `photonView.IsMine` filter |

---

## 🔗 Scene Flow (Existing → Target)

```
Scene 0: 0_PhotonLogin  →  PhotonLogin.cs (connect + set nickname)
         ↓
Scene 1: 1_PhotonRoom   →  PhotonRoom.cs (create/join room, wait for players, Start Game)
         ↓
Scene 2: 2_PhotonGame   →  NEW survivor gameplay scene (all clients join here simultaneously)
```

`PhotonRoom.StartGame()` already calls `PhotonNetwork.LoadLevel("2_PhotonGame")`. You need to **build this scene** and wire it up.

---

## 📋 Step-by-Step Plan

### Phase 1 — Adapt PlayerSpawner for Network Spawning

**Problem:** `PlayerSpawner.cs` uses local `Instantiate()`. In multiplayer, each client must spawn **only its own player** using `PhotonNetwork.Instantiate()`.

**Plan:**
- Replace `PlayerSpawner.cs` with a new `NetworkPlayerSpawner.cs` (or modify existing).
- This script should be used **only in the game scene (2_PhotonGame)**.
- Inherit from `MonoBehaviourPunCallbacks` (or just `MonoBehaviour` and use `PhotonNetwork` static calls).
- In `Start()` or `OnJoinedRoom()`, call:
  ```csharp
  GameObject playerObj = PhotonNetwork.Instantiate("NetworkPlayer", spawnPos, Quaternion.identity);
  ```
- The **prefab** must be placed inside `Assets/Resources/` and named `"NetworkPlayer"`.
- After instantiate, only run local setup (camera assign, health bar) for **`photonView.IsMine`** objects.

> The existing `PhotonPlaying.cs` already does `PhotonNetwork.Instantiate(photonPlayerName, ...)` in `SpawnPlayer()`. You can extend this class or replace `PlayerSpawner` with it.

---

### Phase 2 — Create/Adapt the Player Network Prefab

**Problem:** Your current player prefab uses `PlayerMovement2D`, `PlayerHealth`, `Player` (implicit). None of these have `PhotonView` or ownership checks.

**Plan — Add these components to the player prefab:**

| Component | What to add |
|---|---|
| `PhotonView` | Required. Add to the root of the prefab. |
| `PhotonTransformView` | Syncs position/rotation automatically over the network. |
| `PhotonAnimatorView` | (Optional) Syncs Animator parameters (isRunning, Die, etc.). |

**Plan — Modify `PlayerMovement2D.cs`:**
- Add `using Photon.Pun;` at the top.
- Make the class implement `MonoBehaviourPun` instead of `MonoBehaviour` (or get PhotonView via `GetComponent<PhotonView>()`).
- In `Update()` and `FixedUpdate()`, add an ownership check at the very top:
  ```csharp
  private PhotonView _photonView;
  // In Awake: _photonView = GetComponent<PhotonView>() or GetComponentInParent<PhotonView>();
  // In Update: if (_photonView != null && !_photonView.IsMine) return;
  ```
- This ensures only the **owning client** reads keyboard input and moves the character. Other clients see the position synced by `PhotonTransformView`.

**Plan — Modify `PlayerHealth.cs`:**
- Add `[PunRPC]` to `TakeDamage(int damage)`.
- When an enemy hits, instead of calling `TakeDamage()` directly, call:
  ```csharp
  photonView.RPC("TakeDamage", RpcTarget.All, damageAmount);
  ```
- This ensures the health change runs on **all clients** and the health bar stays in sync.
- `Die()` should also be an RPC or triggered via the same path (since `TakeDamage` already calls `Die()`).

---

### Phase 3 — Fix Enemy AI for Multi-Player

**Problem:** `Enemy.cs` uses `GameObject.FindWithTag("Player")` which only finds **one** player. It also calls `Destroy(gameObject)` directly which is not network-safe.

**Plan:**
- **Enemy ownership:** Only the **MasterClient** should control enemies. Non-master clients should see enemy movement synced.
- Add `PhotonView` to the enemy prefab.
- Add `PhotonTransformView` to sync enemy position.
- In `Enemy.Start()`, replace `GameObject.FindWithTag("Player")` with `PlayerController.Instance.GetNearestPlayer(transform.position)` — this already works in your `PlayerController.cs`!
- In `Enemy.Update()`, add: `if (!PhotonNetwork.IsMasterClient) return;` at the top so only master runs AI logic. Clients receive position via `PhotonTransformView`.
- In `Enemy.Die()`, replace `Destroy(gameObject, 1.0f)` with `PhotonNetwork.Destroy(gameObject)` (MasterClient only).
- EXP spawning in `SpawnExp()` should also only happen on MasterClient:
  ```csharp
  if (PhotonNetwork.IsMasterClient) PhotonNetwork.Instantiate(expPrefabName, ...);
  ```

---

### Phase 4 — Fix CameraDirector

**Problem:** `CameraDirector` follows `PlayerController.GetLocalPlayer()` which returns index 0. In multiplayer, "local player" must be the one with `photonView.IsMine`.

**Plan:**
- Option A (Simple): In `PhotonPlaying.SpawnPlayer()`, after `PhotonNetwork.Instantiate`, check `IsMine`, then call `CameraDirector.SetTarget(playerObj.transform)`.
- Option B (Auto): Add a method to `CameraDirector`:
  ```csharp
  public void SetTargetForMyPhotonPlayer(GameObject playerObj) {
      PhotonView pv = playerObj.GetComponent<PhotonView>();
      if (pv != null && pv.IsMine) SetTarget(playerObj.transform);
  }
  ```
- The camera should only follow the **local player's** transform, not all players.

---

### Phase 5 — Adapt PlayerController Singleton

**Problem:** `PlayerController.cs` uses `DontDestroyOnLoad` and tracks all local `Player` instances. In multiplayer, it needs to distinguish between local and remote players.

**Plan:**
- Keep `PlayerController` as a local manager, **not** a networked object.
- When a new player object is instantiated via Photon, the one with `photonView.IsMine` should call `PlayerController.Instance.RegisterPlayer(player)` only for the local player. Remote players' `Awake()` can skip registration, OR register them all so enemy `GetNearestPlayer()` can target anyone.
- **For enemy targeting** (multi-player co-op), you WANT all players registered in `PlayerController` so `GetNearestPlayer()` works correctly. Each client can register its own player; the enemy only runs on MasterClient and queries `PlayerController.GetNearestPlayer()`.

> **Decision needed:** Do enemies chase ANY player or only the one closest on the MasterClient? Since enemy runs on MasterClient, `GetNearestPlayer()` only has access to players registered on the MasterClient. **Remote players need to be registered on MasterClient too.** This requires syncing player positions somehow (which `PhotonTransformView` already does). The fix is to let **all** players (IsMine or not) register in `PlayerController` after they're instantiated.

---

### Phase 6 — Game State Sync (Start / Over)

**Problem:** The `GameManager.cs` currently manages a number-puzzle mini-game, not the survivor game. The survivor game needs: wave start, game over (all dead), win/lose sync.

**Plan — Create `SurvivorGameManager.cs`:**
- Inherit from `MonoBehaviourPunCallbacks`.
- MasterClient tracks game state (wave number, enemy spawns).
- When ALL players die (listen to `PlayerController.OnAllPlayersDied`), MasterClient broadcasts game over via RPC:
  ```csharp
  photonView.RPC("OnGameOver", RpcTarget.All);
  ```
- `[PunRPC] void OnGameOver()` — show game over screen and offer return to lobby.
- Enemy wave spawning: MasterClient spawns enemies via `PhotonNetwork.Instantiate()`. All clients see them via `PhotonTransformView`.

---

## 🆕 New Scripts to Create

| Script | Location | Purpose |
|---|---|---|
| `NetworkPlayerSpawner.cs` | `Assets/_Data/` | Replaces `PlayerSpawner.cs` in multiplayer scene. Calls `PhotonNetwork.Instantiate` for the local player only |
| `SurvivorGameManager.cs` | `Assets/_Data/` | Game state management: wave spawning, game over RPC, results screen |
| `NetworkPlayer.cs` | `Assets/Script/` | Wrapper on the player prefab root: handles `IsMine` check, links `PlayerMovement2D`, registers with `PlayerController` |

---

## ✏️ Scripts to Modify

| Script | File | Changes |
|---|---|---|
| `PlayerMovement2D` | `Assets/Script/PlayerMovement2D.cs` | Add `PhotonView` ownership check in `Update()` / `FixedUpdate()` |
| `PlayerHealth` | `Assets/Script/PlayerHealth.cs` | Make `TakeDamage()` an `[PunRPC]`, propagate across network |
| `Enemy` | `Assets/Script/Enemy.cs` | MasterClient-only AI; use `PlayerController.GetNearestPlayer()`; use `PhotonNetwork.Destroy` |
| `CameraDirector` | `Assets/Script/CameraDirector.cs` | Follow only `IsMine` player |
| `PhotonPlaying` | `Assets/_Data/PhotonPlaying.cs` | Extend `SpawnPlayer()` to wire camera and health bar after network instantiate |

---

## 🎮 Player Prefab Setup (In Unity Editor)

The network player prefab (place in `Assets/Resources/NetworkPlayer.prefab`) needs:

```
NetworkPlayer (root)
├── PhotonView          ← Required
├── PhotonTransformView ← Syncs position
├── PhotonAnimatorView  ← (Optional) syncs animations
├── Rigidbody2D
├── Collider2D
├── SpriteRenderer
├── Animator
├── PlayerMovement2D    ← Modified (IsMine check)
└── PlayerHealth (child "PlayerHealth")
    └── HealthBar wiring done at runtime
```

---

## 🏗️ Enemy Prefab Setup (in Unity Editor)

Add to existing enemy prefab:

```
Enemy (root)
├── PhotonView          ← Required, ViewID assigned by MasterClient
├── PhotonTransformView ← Syncs position from MasterClient to all clients
├── Animator
├── Collider2D
└── Enemy.cs            ← Modified (IsMine / IsMasterClient checks)
```

---

## 📐 Photon Data Flow Diagram

```
[Client A: Local Player]            [Client B: Remote Player]       [MasterClient (could be A)]
        │                                       │                               │
PlayerMovement2D (IsMine → reads input)         │                               │
        │                                       │                               │
        ├──────── PhotonTransformView ──────────┤◄── Position sync              │
        │                                       │                               │
PlayerHealth.TakeDamage() ─── RPC ────────────►│◄── HP synced on all clients   │
        │                                       │                               │
        │                                       │      Enemy.Update() (MasterClient only)
        │                                       │      ├── GetNearestPlayer() → target nearest alive
        │                                       │      └── PhotonTransformView → sync enemy pos to all
        │                                       │
PlayerController.OnAllPlayersDied ─────────────►SurvivorGameManager.RPC("OnGameOver", All)
```

---

## ✅ Phased Execution Order

| Phase | Task | Key Script |
|---|---|---|
| 1 | Set up player network prefab (PhotonView, TransformView) | Unity Editor |
| 2 | Create `NetworkPlayerSpawner.cs` | New File |
| 3 | Modify `PlayerMovement2D` with `IsMine` check | Modify |
| 4 | Modify `PlayerHealth` with `[PunRPC]` | Modify |
| 5 | Fix `CameraDirector` to follow `IsMine` player | Modify |
| 6 | Fix `Enemy.cs` for MasterClient-only AI + `PhotonNetwork.Destroy` | Modify |
| 7 | Create `SurvivorGameManager.cs` for wave + game over RPC | New File |
| 8 | Build scene `2_PhotonGame` in Unity Editor | Unity Editor |
| 9 | Register all scenes in Build Settings | Unity Editor |
| 10 | Test with 2 clients (Unity Editor + Build) | Testing |

---

## ⚠️ Key Decisions Before Implementation

> [!IMPORTANT]
> **Enemy ownership model:** All enemies are owned and controlled by MasterClient only. Non-master clients just receive synchronized positions. If MasterClient leaves, Photon can transfer ownership — plan for this with `OnMasterClientSwitched` callback in `SurvivorGameManager`.

> [!IMPORTANT]
> **Health sync model:** Use `[PunRPC]` on `TakeDamage` so all clients reflect the same HP. Do NOT use `IPunObservable` for health as it adds latency. RPC is immediate.

> [!WARNING]
> **Assembly Definition (`AGU_Data.asmdef`):** The `_Data` folder has an assembly definition. Make sure `Script/` scripts (PlayerMovement2D, PlayerHealth, etc.) are either in the same assembly or the asmdef references the Photon assemblies correctly before adding `using Photon.Pun;` to scripts in `Assets/Script/`.
