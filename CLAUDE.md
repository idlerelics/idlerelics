# Lost Chambers: Idle Relics

## IMPORTANT: Theme Change
This project was originally "WizDorm - Idle Magic School" (wizard school dorm) and before that a "Perfect Hotel" clone. It is now being re-themed to **Lost Chambers: Idle Relics** — an archaeology exploration idle management game (Indiana Jones-style). The "PerfectHotel" folder naming is legacy. All new work should use archaeology terminology.

## Game Concept
An idle management game where you play as a lead archaeologist exploring ancient sites (pyramids, jungle temples, etc.). You manage expedition workers, deliver supplies, excavate sealed tomb chambers, and collect valuable artifacts. Inspired by My Perfect Hotel but with a narrative reason for room unlocking: archaeological discovery.

- **Engine:** Unity 6 (6000.3.11f1)
- **Render Pipeline:** URP 2D
- **Target Platform:** Mobile (iOS & Android), portrait orientation
- **Art Style:** Cartoony & colorful 3D (Supercell-quality), warm golden lighting, chunky stylized proportions

## Theme Mapping (Hotel → Archaeology)

When working on this codebase, always think in archaeology terms. The code still uses hotel naming but the GAME is archaeology:

| Code Name (Legacy) | Game Concept | Description |
|---|---|---|
| Hotel | Expedition Site | Each "hotel" is a dig site (Pyramid, Jungle Temple, etc.) |
| Room | Tomb Chamber | Excavated one by one, contain artifacts |
| Guest/Customer | Expedition Worker | Arrive at base camp, get assigned to chambers, dig |
| Reception | Base Camp | Where workers register and get assigned |
| Cleaning | Supply Delivery | Workers need water, tools, torches — player delivers |
| Cash Pile | Artifact Discovery | Randomized rarity loot rolls replace flat cash |
| Elevator | Tunnel/Passage | Transitions between expedition sites |
| Toilet | Rest/Supply Station | Workers need rest/resupply, facility depletes and needs restocking |
| Cleaner NPC | Excavation Crew | Automated workers clearing debris |
| Loader NPC | Artifact Transporter | Automated workers moving relics to base camp |
| Room Cleaning Time | Supply Need Duration | How often workers need new supplies |
| Stay Duration | Excavation Time | How long a worker digs before finding something |
| Stay Fee | Artifact Value | Cash earned per excavation (now randomized by rarity) |
| Area/Floor | Dig Zone | Sections of the site unlocked progressively |

## New Systems to Build

### 1. Multiple Supply Types (extends existing Inventory system)
- Add `InventoryType` entries: `WaterCanteen`, `Pickaxe`, `Torch`
- Chambers specify `RequiredSupplyType` — player must deliver the correct type
- Extends `UtilityModule` delivery routing with type-checking
- Key files: `Level/Inventory/`, `Modules/UtilityModule/`, `Level/Entity/Item/ItemToiletController.cs`

### 2. Artifact Loot System (new ArtifactModule)
- Hooks into room state transition: `RoomOccupiedState` → `RoomUsedState`
- Instead of flat `StayFee`, rolls on a rarity table (70% common, 20% rare, 8% epic, 2% legendary)
- Multiplier per tier applied to base value
- New `ArtifactConfig` ScriptableObject per site with weighted tiers
- Creates UI popup showing artifact name, rarity, value
- Key files: new `Modules/ArtifactModule/`, hooks into `Level/Entity/Room/States/RoomOccupiedState.cs`

### 3. Discovery Reveal Sequence (new feature)
- Triggers on first-time chamber unlock (`RoomReadyToPurchaseState` → `RoomAvailableState`)
- DOTween sequence: camera zoom, particle burst, UI popup
- First excavation in new chamber could guarantee higher rarity
- Uses existing DOTween dependency
- Key files: new `Modules/DiscoveryModule/` or extend `RoomController`

### 4. Branching System (extends existing room/area unlock)
- Add `BranchId` (int) and `BranchDependency` (int, -1 for none) to room configs
- Group chambers into branches
- When all chambers in a branch are completed, dependent branch transitions from `RoomHiddenState` to `RoomReadyToPurchaseState`
- 1-2 branching points per site maximum
- Key files: extend `Config/RoomConfig.cs`, `Modules/EntityModule.cs`

### 5. Second Playable Character
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

### Phase 2: Multiple Supply Types
- Extend `InventoryType` enum (WaterCanteen, Pickaxe, Torch)
- Modify `UtilityController`/`UtilityView` for multiple supply stations
- Add `RequiredSupplyType` to item controllers
- Add type-checking in `UtilityModule` delivery routing
- Make player pickup type-aware
- THIS IS THE HARDEST PHASE — touches inventory, utility, items, player states

### Phase 3: Artifact Loot System
- Create `ArtifactModule` with rarity tables
- Create `ArtifactConfig` ScriptableObject
- Hook into `RoomOccupiedState` → `RoomUsedState` transition
- Replace flat `StayFee` with randomized loot roll
- Create UI popup for artifact reveal

### Phase 4: Discovery Reveal
- DOTween sequence on first chamber unlock
- Particle effects, camera interaction, UI popup
- Guarantee higher rarity on first excavation per chamber

### Phase 5: Branching System
- Add BranchId/BranchDependency to RoomConfig
- Branch completion checking in EntityModule
- Design first branching point in Pyramid site

### Phase 6: Second Character
- Add second PlayerConfig entry
- Use duplicate model with different material for now

### Phase 7: Site 2 — Jungle Temple
- New Unity scene with different chamber layout
- New RoomConfig values (harder, higher rewards)
- ElevatorController transitions from Site 1 → Site 2

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
- Remote: https://github.com/wizdorm/wizdorm.git
- Branch: master
