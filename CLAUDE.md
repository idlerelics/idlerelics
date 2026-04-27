# Lost Chambers: Idle Relics

## IMPORTANT: Theme Change
This project was originally "WizDorm - Idle Magic School" (wizard school dorm) and before that a "Perfect Hotel" clone. It is now being re-themed to **Lost Chambers: Idle Relics** — an archaeology exploration idle management game (Indiana Jones-style). The "PerfectHotel" folder naming is legacy. All new work should use archaeology terminology.

## Game Concept
An idle management game where you play as a lead archaeologist exploring ancient sites (pyramids, jungle temples, etc.). You manage expedition workers, deliver supplies, excavate sealed tomb chambers, and collect valuable artifacts. Inspired by My Perfect Hotel but with a narrative reason for room unlocking: archaeological discovery.

- **Engine:** Unity 6 (6000.3.11f1)
- **Render Pipeline:** URP 2D
- **Target Platform:** Mobile (iOS & Android), portrait orientation
- **Art Style:** Cartoony & colorful 3D (Supercell-quality), warm golden lighting, chunky stylized proportions

## Tooling Available (MCP Servers)

This project has two MCP servers configured. Both expose tools prefixed `mcp__<server>__*`.

### Unity MCP (`mcp__UnityMCP__*`)
Live connection to the running Unity Editor. Use it for any task that touches the project's Unity state — scenes, prefabs, scripts, components, assets, play mode, tests, console.

- **Read state via resources first:** `mcpforunity://instances` (active Unity sessions), `mcpforunity://custom-tools` (per-project tools), `editor_state`, `project_info`, `project_tags`, etc.
- **Mutate state via tools:** `manage_scene`, `manage_gameobject`, `manage_prefabs`, `manage_components`, `manage_asset`, `manage_script` / `script_apply_edits` / `apply_text_edits`, `manage_editor` (play mode, tags/layers), `run_tests`, `read_console`, `execute_menu_item`, `execute_code`, `unity_reflect`, `unity_docs`, etc.
- **After every script create/edit, call `read_console`** to check for compilation errors before assuming a new component/type is usable. Poll `editor_state.isCompiling` if needed.
- **Path convention:** all paths are relative to `Assets/`; use forward slashes.
- **Multiple Unity instances:** if more than one is connected, call `set_active_instance` once per session, or pass `unity_instance` per call.
- The server's full instructions are loaded automatically into every Claude session — no need to redocument them here.

### Blender MCP (`mcp__blender__*`)
Live connection to a running Blender instance. Used for the 3D character pipeline (mesh generation, decimation, rigging, FBX export).

- **The addon does NOT auto-start.** Before the first MCP call of any new-scene Blender task, pause and ask Fabian to enable it. (See auto-memory: `feedback_blender_mcp_addon.md`.)
- **Generation backends:**
  - **Hunyuan3D** (`generate_hunyuan3d_model`, `poll_hunyuan_job_status`, `import_generated_asset_hunyuan`) — works via MCP. Default for image-to-mesh.
  - **Hyper3D Rodin** — `mcp__blender__generate_hyper3d_model_via_images` is **broken** (Sharp library error on Hyper3D's API server). Have Fabian generate from the Rodin Blender plugin UI instead. (See auto-memory: `feedback_rodin_via_blender_plugin.md`.)
- **Asset libraries:** `search_polyhaven_assets` / `download_polyhaven_asset`, `search_sketchfab_models` / `download_sketchfab_model`.
- **Direct Blender control:** `execute_blender_code` (arbitrary Python in Blender's context), `get_scene_info`, `get_object_info`, `get_viewport_screenshot`, `set_texture`.
- **Always read `Docs/3D_PIPELINE.md` before any Blender MCP / 3D modeling task** (Working Rule #2). It has the full cookbook: decimation targets, material strategy, rig migration, FBX export settings, gotchas. Update its Asset Log table when creating or modifying any 3D asset.

## Theme Mapping (Hotel → Archaeology)

When working on this codebase, always think in archaeology terms. The code still uses hotel naming but the GAME is archaeology:

| Code Name (Legacy) | Game Concept | Description |
|---|---|---|
| Hotel | Expedition Site | Each "hotel" is a dig site (Pyramid, Jungle Temple, etc.) |
| Room | Tomb Chamber | Excavated over time, supports multi-worker (Lvl 1-3), progressive visual reveal |
| Room Level | Worker Capacity | Lvl 1 = 1 worker, Lvl 2 = 2, Lvl 3 = 3 max |
| Guest/Customer | Expedition Worker | Arrive WITH own tools, register at desk, dig, develop needs (thirst/hunger) |
| Reception | Base Camp Desk | Where workers register. Pacing gate (upgrade = faster registration) |
| Cleaning | Supply Delivery (mid-dig) | Need icons appear above workers. Player delivers water/food from storeroom |
| Cash Pile | Artifact Trickle | Gold/artifacts trickle out at chamber door as workers dig |
| Toilet | Findings Deposit Counter | Workers carry findings to central counter. Player collects there. Limited sorting slots |
| Elevator | Tunnel/Passage | Transitions between expedition sites |
| Cleaner NPC | NPC Supply Runner | Automates supply delivery to workers |
| Loader NPC | Artifact Transporter | Automated workers moving relics to base camp |
| Room Cleaning Time | Supply Need Interval | How often workers develop needs (thirst/hunger) |
| Stay Duration | Excavation Time | How long a worker digs before excavation is complete and they leave |
| Stay Fee | Artifact Value | Base value for rarity roll (randomized per excavation) |
| Area/Floor | Dig Zone | Sections of the site unlocked progressively |

## New Systems to Build

### 1. Revised Worker Flow
- Workers arrive WITH own tools (no player equipping needed)
- Register at base camp desk (pacing gate, upgradable)
- Enter chamber and start excavating immediately
- Mid-dig: need icons (thirst/hunger) appear above workers' heads
- Player delivers consumable supplies (water, food) from storeroom
- If need not met: worker pauses (no penalty, casual-friendly)
- Key files: `Level/Units/`, `Modules/ReceptionModule/`, `Level/Entity/Item/`

### 2. Findings Deposit Counter (replaces Toilet/Bathroom)
- Workers carry accumulated small finds to a central sorting table near base camp
- Counter has limited sorting slots — workers queue when full
- Player taps counter to collect (primary money collection point)
- Each collection = micro-reveal with rarity color glow
- Upgradable: more slots, faster sorting
- Key files: replaces/extends `Modules/ToiletModule/`, `Level/Entity/Toilet/`

### 3. Multi-Worker Chambers (Room Levels = Worker Capacity)
- Level 1 = 1 worker, Level 2 = 2, Level 3 = 3 (max)
- More workers = faster excavation + more supply needs
- Workers visually staggered inside chamber
- Key files: `Level/Entity/Room/`, `Config/RoomConfig.cs`

### 4. Progressive Chamber Reveal
- 4 visual stages: Sealed → Partially Cleared → Mostly Excavated → Fully Revealed
- Permanent progress (never resets)
- Tied to cumulative excavation work across all worker sessions
- Implementation: toggle overlay meshes (dirt/rubble/dust layers) or swap prefab variants
- Key files: `Level/Entity/Room/RoomView.cs`, `Level/Entity/Room/States/`

### 5. Artifact Loot System & Collection Album
- Rarity roll on collection: 70% common, 20% rare, 8% epic, 2% legendary
- Artifacts slot into a Collection Album organized by themed sets per site
- Completing a set = permanent bonus (faster excavation, better odds, cosmetics)
- Duplicates auto-convert to currency
- New `ArtifactModule` + `ArtifactConfig` ScriptableObject
- Key files: new `Modules/ArtifactModule/`

### 6. Discovery Reveal Sequence
- Triggers when chamber reaches Fully Revealed state (stage 4)
- DOTween sequence: camera zoom, particle burst, UI popup
- Uses existing DOTween dependency
- Key files: new `Modules/DiscoveryModule/` or extend `RoomController`

### 7. Branching System (extends existing room/area unlock)
- Add `BranchId` (int) and `BranchDependency` (int, -1 for none) to room configs
- Group chambers into branches
- When all chambers in a branch are completed, dependent branch transitions from `RoomHiddenState` to `RoomReadyToPurchaseState`
- 1-2 branching points per site maximum
- Key files: extend `Config/RoomConfig.cs`, `Modules/EntityModule.cs`

### 8. NPC Supply Runner
- Unlockable NPC that automates supply delivery
- Upgrade path: number of runners, carrying capacity
- Key files: extends `Level/Entity/Cleaner/` pattern

### 9. Second Playable Character
- `PlayerConfig` system already supports multiple characters via `GameConfig.PlayersMap`
- Add second entry with different mesh, icon, sex
- Character selection exists via `PlayerSelectionState`
- Male: Indiana Jones archetype (fedora, leather jacket, brown/khaki)
- Female: Original explorer (auburn hair, olive green shirt, aviator goggles, no Lara Croft copy)

## Development Plan (Mechanics First, Art Later)

### Phase 1: Core Re-theme (config only)
- Update `GameConfig` ScriptableObject timing values (excavation times, supply durations)
- Adjust `ReceptionModule` timing for base camp feel (faster registration)
- Test the existing MPH loop as an archaeology game with placeholder hotel assets
- No code changes — config and balance only

### Phase 2: Revised Worker Flow
- Workers arrive with own tools, register at desk, go to chamber
- Implement need icons (thirst/hunger) mid-excavation
- Player delivers consumable supplies (water, food) from storeroom
- Touches: `Level/Units/`, `Modules/ReceptionModule/`, supply delivery logic

### Phase 3: Findings Deposit Counter
- Replace toilet/bathroom with deposit counter
- Workers carry findings to counter periodically, player collects there
- Limited sorting slots, upgrade path
- Touches: `Modules/ToiletModule/`, `Level/Entity/Toilet/`

### Phase 4: Multi-Worker Chambers & Room Levels
- Chamber levels 1-3 = worker capacity 1-3
- Visual staggering of workers inside chambers
- Supply needs scale with worker count

### Phase 5: Progressive Chamber Reveal
- 4 visual stages (sealed → fully revealed), permanent progress
- Toggle overlay meshes or swap prefab variants

### Phase 6: Artifact Loot System & Collection Album
- `ArtifactModule` with rarity tables + micro-reveal at deposit counter
- Album UI with themed sets, completion bonuses, duplicate handling

### Phase 7: Discovery Reveal Sequence
- DOTween sequence when chamber reaches fully revealed state

### Phase 8: Branching System
- BranchId/BranchDependency in RoomConfig, branch completion checks

### Phase 9: NPC Supply Runner
- Unlockable automated delivery NPC, upgrade path

### Phase 10: Second Character
- Second PlayerConfig entry, duplicate model with different material

### Phase 11: Site 2 — Jungle Temple
- New Unity scene, different layout, harder configs

## Working Rules (for Claude)

These are non-negotiable process rules for any Claude session working in this repo. They exist because we've been burned without them.

**0. Read `Docs/DEVLOG.md` at the start of every new task, before answering the first substantive question.** At minimum read the most recent 3–4 entries; skim further back if the user's question touches a system you don't recognise. The devlog is the canonical cross-session memory — decisions, design rules, gotchas, and open items from prior tasks live there, not in your loaded context. Do not answer "what did we do recently?" or "how does system X work?" from CLAUDE.md alone — CLAUDE.md is the *rulebook*, DEVLOG.md is the *history*. If you skip this step and answer from inference, you will contradict decisions the user already made and force them to re-explain their own game.

**1. Update `Docs/DEVLOG.md` as part of the work, not as an optional postscript.** At the end of any non-trivial code change, design decision, architectural investigation, or bug fix, append a dated entry to `Docs/DEVLOG.md` *before* considering the task complete. Newest entries go at the top (under the file header, above prior entries). Each entry should contain: a `## YYYY-MM-DD — short title` heading, a one-line **Summary**, a **What changed** section (file-by-file for code changes), a **Why** section for non-obvious decisions, and **Open items** for things deliberately left unfinished. This log is the canonical cross-session memory for the project — Fabian and future Claude sessions both read it at the start of new work. Treat missing devlog entries as incomplete work.

**2. Read `Docs/3D_PIPELINE.md` at the start of any 3D modeling, Blender MCP, or asset creation task.** This file contains the full pipeline cookbook — Hunyuan3D generation workflow, decimation targets, material assignment strategy, rigging/animation notes, FBX export settings, and Blender MCP gotchas. It also has an Asset Log table at the bottom; update it when creating or modifying any 3D asset.

**3. Open `.prefab` files directly when investigating scene entities — don't trust class-name grep.** In this project, prefab files often reference a different MonoBehaviour than their folder/file name suggests (e.g. the Hotel 2 "vending machines" are actually `ToiletView` prefabs, not `SodaView`). For any question of the form *"is feature X implemented?"* or *"how is system Y wired up?"*, the investigation order is: (1) find the relevant prefab(s) under `Assets/GorodiskiGames/PerfectHotel/ResourcesStatic/Prefabs/Levels/HotelN/`, (2) grep inside those prefab files for `m_Script` lines, (3) cross-reference the GUIDs against `.cs.meta` files to find the *actual* class driving the entity, (4) only then investigate that class. Folder names, file names, and enum values are not evidence that a feature exists — they may be aspirational or orphaned.

## Architecture

### Entry Point
- `Assets/GorodiskiGames/PerfectHotel/Scripts/GameStartBehaviour.cs` — bootstraps the game via a custom IoC `Context`

### Game Flow (State Machine)
```
GameStartBehaviour → GameInitializeState → GameLoadLevelState → GamePlayState
```
Managed by `GameStateManager` with `State` base class.

### Core Patterns
- **Dependency Injection** — custom `Context` / `Injector` / `Inject` attribute (not Zenject)
- **Observer Pattern** — `Observable<T>` / `IObserver` for events
- **Module System** — feature modules (CashModule, EntityModule, ReceptionModule, ToiletModule, etc.)
- **MVC-style UI** — `BehaviourWithModel<T>` / `Mediator` base classes

### Module Initialization Order (in GamePlayState)
1. `ReceptionModule` — Base Camp / worker registration
2. `EntityModule` — Areas, chambers, tunnel/passage, excavation crew
3. `ToiletModule` — Rest/supply station management
4. `UtilityModule` — Supply distribution system
5. `CashModule` — Currency (artifact value) collection
6. `UISpritesModule` — Sprite management
7. `UINotificationModule` — On-screen notifications

### Key Managers (installed in Context)
- `GameStateManager` — state machine
- `GameManager` — central hub (items, rooms, areas, player, events)
- `ItemRegistry` — item add/remove/find, ITEM_ADDED event
- `GameEventBus` — all game-wide events
- `HudManager` — UI management
- `ResourcesManager` — asset loading
- `AdsManager` — ad provider abstraction
- `IAPManager` — Unity IAP integration

### Room (Chamber) State Machine
```
RoomInitializeState
  ├→ RoomHiddenState (not enough progress OR branch locked)
  ├→ RoomReadyToPurchaseState (enough progress, awaiting purchase)
  ├→ RoomUsedState (purchased, needs supplies)
  └→ RoomAvailableState (purchased, supplied, ready for worker)

RoomAvailableState → RoomOccupiedState (worker assigned, excavating)
RoomOccupiedState → RoomUsedState (excavation done, artifact found, needs resupply)
RoomUsedState → RoomAvailableState (supplies delivered)
```

### Player Interaction System
`PlayerInteractionFactory` maps `ItemType` → `PlayerState`:
- `ItemType.Clean` → `PlayerCleaningState` (delivering supplies)
- `ItemType.ReceptionDesk` → `PlayerReceptionState` (registering workers)
- `ItemType.BuyUpdate` → `PlayerOnItemState` (purchasing/upgrading chambers)
- `ItemType.ShowHud` → `PlayerElevatorState` (tunnel/passage UI)

To add new interactions: register in `GamePlayState.Initialize()` via `_interactionFactory.Register()`

### Persistence
- **PlayerPrefs** with JSON-serialized `GameModel`
- Entity keys: `"Hotel" + hotelIndex + entityType + number` (e.g., `"Hotel1Room3"`)
- Per-entity: `"IsUsed"`, `"IsPurchased"`, `"Lvl"`, `"Cash"` + entity ID
- Large numbers stored as strings

## Project Structure

```
Assets/GorodiskiGames/PerfectHotel/
├── Scripts/
│   ├── Core/          # DI container, state machine, observers, timers
│   ├── Config/        # ScriptableObject configs (GameConfig, HotelConfig, RoomConfig, etc.)
│   ├── Domain/        # GameModel (save data)
│   ├── Managers/      # Game-wide managers (Ads, IAP, HUD, Resources, Login)
│   ├── Level/         # Gameplay mechanics
│   │   ├── Cash/      # Currency collection & piles
│   │   ├── Entity/    # Rooms, elevators, reception, cleaners, loaders
│   │   ├── Inventory/ # Player inventory (max 3 items)
│   │   ├── Player/    # Player character & controls
│   │   ├── Units/     # NPCs / guests / workers
│   │   └── Place/     # Level/area entities
│   ├── Modules/       # Feature modules (Cash, Entity, Reception, Toilet, Utility, UI)
│   ├── States/        # Game states (Initialize, LoadLevel, Play)
│   ├── UI/            # UI views & mediators
│   ├── Camera/        # CameraController
│   ├── Plugins/       # Joystick input
│   └── Utils/         # Helpers
├── Scenes/
│   ├── Gameplay.unity # Main scene
│   ├── Hotel1.unity   # Site 1: Egyptian Pyramid
│   └── Hotel2.unity   # Site 2: Jungle Temple
├── Resources/         # Dynamic-loaded assets (prefabs, sprites, configs)
└── ResourcesStatic/   # Static assets (animations, materials, models, shaders, textures, fonts)
```

## Key Class Locations

All paths relative to `Assets/GorodiskiGames/PerfectHotel/Scripts/`.

### Managers & Config
- `GameManager` → `Managers/GameManager.cs`
- `ItemRegistry` → `Managers/ItemRegistry.cs`
- `GameEventBus` → `Managers/GameEventBus.cs`
- `GameConfig` → `Config/GameConfig.cs`
- `GameModel` → `Domain/GameModel.cs`

### Player
- `PlayerController` → `Level/Player/PlayerController.cs`
- `PlayerInteractionFactory` → `Level/Player/PlayerInteractionFactory.cs`
- Player states → `Level/Player/PlayerStates/`

### Entities (Chambers, Base Camp, Tunnel)
- Room/Chamber → `Level/Entity/Room/`
- Room states → `Level/Entity/Room/States/`
- Elevator/Tunnel → `Level/Entity/Elevator/`
- Reception/Base Camp → `Level/Entity/Reception/`
- Cleaner/Excavation Crew → `Level/Entity/Cleaner/`
- Loader/Transporter → `Level/Entity/Loader/`
- Toilet/Supply Station → `Level/Entity/Toilet/`
- Items → `Level/Entity/Item/`

### Modules
- `CashModule`, `EntityModule`, `ReceptionModule`, `ToiletModule`, `UtilityModule` → `Modules/`

### Core Framework
- DI Container → `Core/DI/` (`Context.cs`, `Injector.cs`)
- State Machine → `Core/StateMachine/`
- Observer → `Core/Observer/`
- Timer → `Core/Timer.cs`

## Coding Conventions
- Namespace: `GorodiskiGames.PerfectHotel.*`
- Custom DI: use `[Inject]` attribute, register via `Context.Install<T>()`
- Config values go in ScriptableObjects, not hardcoded
- UI follows `BehaviourWithModel<T>` pattern with `Mediator` base
- Fields: `_lowerCamelCase` for private, `PascalCase` for public
- Events: `ON_ACTION_VERB` naming (e.g., `ON_PURCHASED`, `ON_UPGRADED`)
- New entities: create Config → View → Model → Controller stack
- New modules: create `Module<T>` subclass, add to initialization order in `GamePlayState`

## Third-Party Dependencies
- **DOTween** — tweening/animation
- **Google AdMob** — ads
- **Unity IAP** — in-app purchases
- **TextMesh Pro** — text rendering
- **EDM4U** — native dependency management

## Game Design Reference
See `Docs/GAME_DESIGN.md` for the full game design document including: expedition sites, artifact rarity system, branching system, character designs, art direction, and monetization strategy.

## Git
- Remote: https://github.com/idlerelics/idlerelics.git
- Branch: master
