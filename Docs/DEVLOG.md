# DEVLOG

A running log of meaningful changes, decisions, and gotchas for Lost Chambers: Idle Relics. Entries are dated and appended at the top (newest first). This file is intended to be read by both Fabian and Claude at the start of new sessions so context carries across conversations.

**Conventions:** Each entry has a date, a one-line summary, a *What changed* section, a *Why* section for non-obvious decisions, and an *Open items* section for things deliberately left unfinished.

---

## pre-2026-04-08 — Chamber lighting, torch-lighting maid, and collector office design (backfilled)

**Summary.** Chain of decisions from an earlier Cowork session that established the chamber lighting loop, re-fictioned the maid as a torch-lighter, added player-vs-NPC item stealing, and locked in the collector office design. Backfilled into this log after the fact.

### Chamber lighting is driven by room state, not worker count

Chambers start **dark** and only light up while the maid's torches are burning. The lighting trigger lives in the room state machine, not in the worker-count events:

- `RoomView.Awake` — chambers spawn dark.
- `RoomAvailableState` — lit (torches burning, room ready for workers).
- `RoomOccupiedState` — lit (workers digging under torchlight).
- `RoomUsedState` — dark (torches burned out, maid coming to relight).

An earlier attempt tied lighting to `OnWorkersChanged` (lit while any worker is physically inside). It was correct under the fiction "lit = someone is in the room," but got reverted once the maid-as-torch-lighter frame took over. State transitions are simpler and match the new fiction exactly.

### The maid is now a torch-lighter, not a cleaner (pure re-skin)

The existing cleaner NPC, pathfinding, queue logic, and 3-point item loop are all reused verbatim. Only the *fiction* changed: the maid lights 3 torches instead of cleaning 3 dirty points. **No new NPC, no new code path.**

One real behavior change: newly purchased chambers now transition into `RoomUsedState` instead of `RoomAvailableState` after `RoomReadyToPurchaseState`. This means the very first thing that happens to a fresh chamber is the maid arriving to light it — a small discovery moment for the player, and it keeps the loop consistent (every lit chamber was lit *by the maid*).

Loop: Purchased → dark → maid lights 3 torches → Available (lit) → workers arrive and dig → `StayDuration` expires → Used (dark, torches burned out) → maid re-queued.

Eventual rename candidate (deferred): `RoomAvailableState` → `RoomLitState`, `RoomUsedState` → `RoomUnlitState`, etc. Not worth the churn until the mechanic is fully locked in.

### Player can steal torch-lighting tasks from the maid (soft-claim system)

**Why:** the player should always feel more powerful than the NPC. Previously, the cleaner would call `_gameManager.RemoveItem()` the moment she picked a target, silently removing it from the player's `FindClosestUsedItem` lookup. Result: one of the 3 torches was effectively invisible to the player until the maid finished it.

**How it works now:**
- `ItemController.Claim(claimer)` / `ReleaseClaim()` — items carry a soft claim tag.
- `ItemRegistry.FindUsedItem` skips claimed items for *cleaners* (so cleaners don't fight over the same torch).
- Player's `FindClosestUsedItem` is **intentionally unfiltered** — player sees everything.
- When the player walks onto a claimed torch, `PlayerItemState.Initialize` calls `_item.Claim(this)`. The claimer change fires `CLAIM_REVOKED`. Both `CleanerWalkToItemState` and `CleanerCleaningState` subscribe and abort to `CleanerIdleState` on revoke.
- If the player walks away mid-light (joystick interrupt), they release the claim before re-adding the item to the registry, so the maid can pick it back up.
- When the cleaner finishes a torch normally, she now releases the claim *and* removes the item from the registry — without this, the `ClaimedBy` reference persisted across dig cycles (ghost claims), and the item stayed in the registry causing the maid to re-target the just-finished torch at zero distance (the "shaking maid" bug).

### Collector office design (agreed but mostly uncoded)

The plan for turning toilets into a relics collector office:

- Single inventory type: **parchment** (not parchment + ink). Keeps the change minimal and leaves a second inventory slot free for a future soda-machine-equivalent station where the helper has a separate job.
- Re-uses the toilet cabin/seat system verbatim (`ItemToiletController` stays as-is conceptually). Worker walks to free seat, hands over relic, collector logs it for `StayDuration`, then available again until parchment runs out.
- **Workers visit the collector only if they found a relic.** Empty-handed workers go home directly.
- `HasRelic` is hardcoded at **75%** for now — deliberately a placeholder so a future Power-Up system can modulate the find rate.
- **Relic finding does not replace cash generation.** Cash still trickles during `RoomOccupiedState`. The relic is a *separate post-dig outcome* that plays out when the worker leaves the chamber. This is why the collector visit can be theatrical/optional without breaking the economy.
- Class renames (`RelicCollectorController`, `CollectorOfficeModule`, etc.) deliberately deferred until the mechanic is locked in.

### Open items

- Empty-handed worker visual (slumped walk / "—" particle / nothing) — not decided.
- `RoomLit`/`RoomUnlit` rename pass — deferred.
- Collector class/module rename pass — deferred.
- Unified "dig outcome roll" that produces both the `HasRelic` boolean and (eventually) the rarity tier from a single random call — not built yet; current code is the 75% placeholder only.

---

## 2026-04-08 — Rule 0 added: read DEVLOG at task start

**Summary.** Added a "Rule 0" to CLAUDE.md's Working Rules mandating that every new task read `Docs/DEVLOG.md` (at minimum the most recent 3–4 entries) before answering the user's first substantive question.

### Why

First real test of the cross-session memory system revealed the gap: a new task started in the project loaded CLAUDE.md fine, but when asked "what did we do today?" the new Claude answered from inference instead of reading the devlog. CLAUDE.md *mentioned* the devlog existed but didn't *require* reading it, so the instruction was interpreted as "this file exists" rather than "open this file now." Rule 0 closes the gap by making the read explicit and positioning it *before* Rule 1 (the write rule) so it's the first thing a new session sees.

### Open items

- This is still a norm, not a hard guarantee. If a future task's first response feels uninformed, the reliable manual workaround is for Fabian to say "read Docs/DEVLOG.md first" as the opening message.

---

## 2026-04-08 — Chamber upgrade economics: dual-lever design

**Summary.** Locked in the upgrade-economics rule: chamber level controls both worker capacity *and* `StayFee` per dig. Verified against the existing `RoomOccupiedState` code, which already supports this model — it's a config decision, not a code change.

### The rule

Upgrading a chamber raises its **level**, and the level drives two things:

1. **Worker capacity.** Level 0 = 1 worker slot, level 1 = 2, level 2 = 3. The chamber's physical size gates how many workers can dig in parallel.
2. **`StayFee` per dig.** Each level tier in `RoomConfig.Lvls[]` can list a higher `StayFee`, so a higher-level chamber is intrinsically richer per completed dig.

The two levers compound: a level-2 chamber earns `3 workers × higher StayFee × faster throughput` compared to a level-0 chamber. This is deliberate — it's the classic idle-game curve and it gives upgrading a meaningful punch at every step.

### How it actually works in `RoomOccupiedState`

Cash and dig duration are both scaled by `ActiveWorkerCount` in `OnTick`:

```csharp
scaledDt = dt * workers;
_stayDuration -= scaledDt;                        // dig finishes worker× faster
int trickle = _baseTrickleAmount * workers;       // cash arrives worker× faster
```

Important nuance: the **total payout for a single dig** is always `StayFee`, regardless of how many workers are inside. Workers don't each print independent money — they *speed up* the dig, which raises throughput (digs-per-minute), which is what translates into more cash per minute. The per-dig amount is set once in `Initialize()` from `_room.Model.StayFee`, which itself comes from `RoomController.StayFee = Lvls[Lvl].StayFee`.

This means there are exactly two places to tune chamber economy:

- **`RoomConfig.Lvls[Lvl].StayFee`** — the per-dig payout for each level tier. Bump this to make upgrades feel richer per dig.
- **Worker capacity per level** — currently 1/2/3, could be tuned later if throughput feels off.

Duration tuning lives on `StayDuration`, which is also per-level via the same `Lvls[]` array, so you can independently decide whether higher-level chambers dig *longer* (more cash per dig but slower loop) or *shorter* (faster loop, less per dig).

### Why the dual-lever model instead of "workers are the only lever"

A pure "workers = money, chamber is just a container" rule was considered (flat `StayFee` across all levels, upgrading only unlocks worker slots). Rejected because it collapses two useful tuning knobs into one and makes late-game chambers feel economically identical to early-game ones except for being bigger. The dual-lever version lets Site 2's chambers feel genuinely richer per dig *as well as* bigger, which matters for per-site progression and for the eventual Site 2 unlock.

### Open items

- Actual `StayFee` / `StayDuration` / worker-cap numbers per level — not tuned, just the shape is decided.
- Whether the `StayFee` curve should be linear, exponential, or hand-tuned per level — deferred until there's enough chambers to playtest.
- Worker-hiring and worker-pool mechanics — deliberately deferred, not part of this decision.

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
