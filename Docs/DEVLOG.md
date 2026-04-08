# DEVLOG

A running log of meaningful changes, decisions, and gotchas for Lost Chambers: Idle Relics. Entries are dated and appended at the top (newest first). This file is intended to be read by both Fabian and Claude at the start of new sessions so context carries across conversations.

**Conventions:** Each entry has a date, a one-line summary, a *What changed* section, a *Why* section for non-obvious decisions, and an *Open items* section for things deliberately left unfinished.

---

## 2026-04-08 — Working Rules added to CLAUDE.md

**Summary.** Promoted two working rules (update DEVLOG after any non-trivial work; open prefabs before trusting class-name grep) from auto-memory into `idlerelics/CLAUDE.md`, so they apply to every Claude session on this repo automatically instead of depending on Fabian remembering to ask.

### What changed

- **`idlerelics/CLAUDE.md`** — added a new `## Working Rules (for Claude)` section immediately before `## Architecture`. Rule 1 makes devlog updates a required part of task completion. Rule 2 codifies the "open the .prefab, check `m_Script` GUID" investigation order to prevent repeats of the Hotel 2 vending-machine mis-read.

### Why

The previous setup relied on Claude's auto-memory (`.auto-memory/`), which is machine-scoped and not part of the repo. That meant the rules only fired when Claude happened to load memory, and any new Claude environment would start blind. Putting the rules in `CLAUDE.md` makes them travel with the repo in git — visible to Fabian, versioned, and loaded automatically by any Claude agent that reads the project's CLAUDE.md at startup.

### Open items

None — self-referential entry; the rule was applied to document its own addition.

---

## 2026-04-08 — Inspector-driven hotel start override

**Summary.** Replaced the earlier hardcoded `_hotel = 2;` debug hack with a proper Inspector-driven override on `GameConfig`, so switching test hotels no longer requires editing code (and no longer risks an accidental commit of a hack).

### What changed

- **`Assets/GorodiskiGames/PerfectHotel/Scripts/Config/GameConfig.cs`** — added a new `[Header("Debug")]` section with a `StartHotelOverride` int field (default `0`). `[Min(0)]` prevents negative values; a `[Tooltip]` explains the behavior in the Inspector. Field is set on the `Resources/GameConfig.asset` ScriptableObject and therefore Inspector-editable.
- **`Assets/GorodiskiGames/PerfectHotel/Scripts/States/GameLoadLevelState.cs`** — after the existing save-load-clamp-and-save-back block, the state now checks `_config.StartHotelOverride`. If greater than 0, it replaces `_hotel` with the override (re-clamping against build settings as a safety). Crucially, the override is applied **after** `model.Save()`, so the save file is untouched — toggling the override back to 0 returns to real progress with zero lingering side effects.

### Why

The earlier approach (edit a line in `GameLoadLevelState.cs`, press Play, revert before commit) worked but had two failure modes: (1) easy to forget to revert, risking a committed hack, and (2) required a recompile every time we wanted to flip hotels. The Inspector-driven override fixes both — you flip a number in the `GameConfig.asset` Inspector and press Play, no code change, no recompile, no risk of committing a test hack.

### How to use

1. Open `Assets/GorodiskiGames/PerfectHotel/Resources/GameConfig.asset` in the Inspector.
2. Scroll to the **Debug** section at the bottom.
3. Set `Start Hotel Override` to the desired hotel scene index (`1` = Hotel1, `2` = Hotel2, etc.).
4. Press Play.
5. To return to normal play, set it back to `0`.

### Open items

- The previous day's *"Hotel 2 debug override still in place"* open item is now **resolved**. `GameLoadLevelState.cs` is back to its original form; no hardcoded override remains.

---

## 2026-04-08 — Relic placeholder prop and collector office architecture

**Summary.** Investigated the Hotel 2 vending machine system, confirmed it is implemented as a reskinned toilet, and added a placeholder cube prop so workers visibly carry a "relic" from chamber to collector office.

### What changed

- **`Assets/GorodiskiGames/PerfectHotel/Scripts/Level/Units/UnitView.cs`** — added `AttachRelicPlaceholder()` and `DetachRelic()` methods plus a private `_carriedRelic` field. `AttachRelicPlaceholder` instantiates a primitive cube at runtime, strips its collider, parents it to the unit transform at local position `(0, 1.25, 0.35)` with scale `0.3`, and activates animation layer 1 so the existing carry upper-body pose blends in. `DetachRelic` destroys the cube and resets the layer. Idempotent.
- **`Assets/GorodiskiGames/PerfectHotel/Scripts/Level/Units/States/UnitInRoomState.cs`** — after `RollHasRelic()` returns true and before routing to the collector, calls `_unit.View.AttachRelicPlaceholder()`.
- **`Assets/GorodiskiGames/PerfectHotel/Scripts/Level/Units/States/UnitInToiletCabineState.cs`** — `Initialize()` now calls `DetachRelic()` at the top. The cube disappears the moment the worker enters the cabine, representing the handoff to the collector.
- **`Assets/GorodiskiGames/PerfectHotel/Scripts/Level/Units/States/UnitWalkToRemoveState.cs`** — safety net: also calls `DetachRelic()` in `Initialize()` so workers who fail to reach a collector (full office, no office in area) don't walk off-screen holding the cube.

### Key architectural finding: there is no separate "collector" entity

What looks like a vending machine in Hotel 2 is mechanically a `ToiletView` with a vending machine mesh on top. Verified by inspecting `SodaBaseHotel2.prefab` — its root component script GUID matches `ToiletView.cs.meta`, not `SodaView.cs.meta`. The `SodaView` class exists in `Assets/.../Level/Entity/Soda/SodaView.cs` but is **orphaned**: no controller, no module, no references anywhere in the code. The `Soda/` folder and `EntityType.Soda` enum value are dead code from an abandoned refactor attempt.

Because the vending machine IS a toilet, the entire worker flow already exists:
1. `UnitInRoomState.LeaveChamber()` calls `_gameManager.RollHasRelic()`.
2. If true, it calls `_gameManager.FindToilet(_unit.Area)` and routes the worker to an available cabine, or to a queue slot on `toilet.Line`, or to despawn if both are full.
3. The loader NPC keeps the cabines stocked with `InventoryType.SodaCan` (not ToiletPaper) via `UtilityModuleView.InventoryMap`.
4. The in-chamber comment on `UnitInRoomState.LeaveChamber` explicitly refers to "log at the collector office" — the intent was always there, just hidden under toilet naming.

### Economy — cash trickles during excavation, not at the collector

The comment on line 100 of `UnitInRoomState.cs` confirms: *"Cash was already trickled during the dig (RoomOccupiedState), so this is purely about whether the worker carries a physical artifact out to log at the collector office."* The collector visit is **theatrical**, not transactional — it's a visual payoff, not an economic one.

**Decision (2026-04-08):** Keep this model for now. Idle players spend a lot of time not watching the screen, and cash that only exists on successful collector delivery would punish AFK play. If we ever want a "bonus on delivery" tier (e.g., rare relics that pay extra when logged), that would layer on top of the trickle, not replace it.

### Design conversation: intended collector office workflow

Fabian confirmed the target workflow matches the current implementation:

1. Worker finishes excavating; if they found a relic, walk to collector office.
2. Office has 3 collectors ("vending machines") behind a table. Each needs parchment (currently `SodaCan`) to accept a relic.
3. Workers queue waiting for a collector to be free and stocked.
4. Parchment delivered by loader or player (already works).

Every step is already implemented. The only visible gap was the worker not carrying a visible prop — fixed above with the cube.

### Open items

- **Hotel 2 debug override still in place.** `GameLoadLevelState.cs` lines ~45–49 contain `_hotel = 2;` hardcoded to force-load Hotel 2 for testing. **Must be reverted before shipping or normal play.**
- **Orphaned `Soda/` folder and `EntityType.Soda` enum value.** Dead code. Fabian OK'd deletion in principle; not yet done.
- **Rename pass `Toilet` → `Collector`.** Three layers of naming mismatch exist: `ToiletController`/`ToiletModule`/etc. in code, vending machine art, "collector office" in the design. Deferred until gameplay is stable; doing it mid-feature would double the debugging surface.
- **Relic prop is a primitive cube.** Placeholder. Needs a proper relic mesh + a hand-bone anchor on the worker rig once art is ready. Current position `(0, 1.25, 0.35)` is a guess and may need tweaking per worker model scale.
- **Workers don't carry different relic types.** All relics look the same. If/when we build the artifact rarity system (Phase 3 of the original plan), the prop would ideally vary by tier.

### Gotchas and lessons learned

- **Open the `.prefab` files directly when investigating what a scene entity actually is.** Folder names and class names lie; prefab `m_Script` GUIDs don't. Grepping for references to a class file name tells you nothing if the prefab uses a *different* class with the same conceptual role.
- **`UnitView` already has a carry-animation layer system** (`SetLayerWeight`, `AnimationType.Carry`) used by staff units via `UnitStaffView.InventoryHolder`. Regular guest/worker units don't have the holder transform, which is why the relic placeholder is parented to the unit transform directly with a hardcoded offset rather than a rigged bone.
- **`UnitStaffView : UnitView`** has an `_inventoryHolder` Transform for properly anchored staff-carried items. If we later upgrade workers to use a rigged carry point, mirror this pattern on `UnitView` (or introduce a `UnitGuestView` subclass).
