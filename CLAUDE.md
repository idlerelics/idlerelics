# WizDorm - Idle Magic School

## Project Overview
An idle management game inspired by **My Perfect Hotel**, set in a **Wizard School** (WizDorm). Players manage a magical dormitory — assigning rooms to wizard students, collecting currency, upgrading facilities, and expanding the school.

- **Engine:** Unity 6 (6000.3.11f1)
- **Render Pipeline:** URP 2D
- **Target Platform:** Mobile (iOS & Android), portrait orientation
- **Bundle Version:** 0.0.1 (early development)

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

### Key Managers (installed in Context)
- `GameStateManager` — state machine
- `HudManager` — UI management
- `ResourcesManager` — asset loading
- `AdsManager` — ad provider abstraction (GoogleAdMob / Fake)
- `IAPManager` — Unity IAP integration
- `LoginManager` — daily login tracking

### Persistence
- **PlayerPrefs** with JSON-serialized `GameModel`
- Keys: `"model"`, `HotelLvl{n}`, `HotelProgress{n}`, `watch_ads_times`, `login_days`, `login_date`

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
│   ├── Hotel1.unity   # Hotel/dorm level 1
│   └── Hotel2.unity   # Hotel/dorm level 2
├── Resources/         # Dynamic-loaded assets (prefabs, sprites, configs)
└── ResourcesStatic/   # Static assets (animations, materials, models, shaders, textures, fonts)
```

## Key Class Locations

All paths relative to `Assets/GorodiskiGames/PerfectHotel/Scripts/`.

### Managers & Config
- `GameManager` → `Managers/GameManager.cs` (thin wrapper, delegates to ItemRegistry & GameEventBus)
- `ItemRegistry` → `Managers/ItemRegistry.cs` (item add/remove/find, ITEM_ADDED event)
- `GameEventBus` → `Managers/GameEventBus.cs` (all game-wide events and Fire methods)
- `GameConfig` → `Config/GameConfig.cs`
- `GameModel` → `Domain/GameModel.cs`
- `HudManager` → `Managers/HudManager.cs`
- `ResourcesManager` → `Managers/ResourcesManager.cs`
- `AdsManager` → `Managers/AdsManager.cs`

### Core Framework
- `BehaviourWithModel<T>` → `Core/UI/BehaviourWithModel.cs`
- `Observable`, `IObserver` → `Core/Observer/Observable.cs`, `Core/Observer/IObserver.cs`
- `Timer` → `Core/Timer.cs`
- `Context`, `Injector` → `Core/DI/`
- `State` (base) → `Core/StateMachine/`

### Player States
- `PlayerIdleState`, `PlayerWalkState`, `PlayerFindEntityState` → `Level/Player/PlayerStates/`
- `PlayerItemState`, `PlayerCleaningState`, `PlayerReceptionState`, `PlayerOnItemState`, `PlayerElevatorState`, `PlayerPauseState` → `Level/Player/PlayerStates/`
- `PlayerController` → `Level/Player/PlayerController.cs`

### Items
- `ItemController`, `ItemRoomController`, `ItemToiletController`, `ItemReceptionController`, `ItemModel` → `Level/Entity/Item/ItemController.cs`
- `ItemView`, `ItemType` enum → `Level/Entity/Item/ItemView.cs`
- `ItemFillBarView` → `Level/Entity/Item/ItemFillBarView.cs`
- `ItemReusableView` → `Level/Entity/Item/ItemReusableView.cs`

### Room States
- `RoomInitializeState` → `Level/Entity/Room/States/RoomInitializeState.cs`
- `RoomAvailableState` → `Level/Entity/Room/States/RoomAvailableState.cs`
- `RoomOccupiedState` → `Level/Entity/Room/States/RoomOccupiedState.cs`
- `RoomUsedState` → `Level/Entity/Room/States/RoomUsedState.cs`
- `RoomReadyToPurchaseState`, `RoomHiddenState` → `Level/Entity/Room/States/`
- `RoomController` → `Level/Entity/Room/RoomController.cs`

### Game States
- `GameInitializeState` → `States/GameInitializeState.cs`
- `GameLoadLevelState` → `States/GameLoadLevelState.cs`
- `GamePlayState` → `States/GamePlayState.cs`

### Modules
- `CashModule`, `EntityModule`, `ReceptionModule`, `ToiletModule`, `UtilityModule` → `Modules/`

### UI
- `GameView` (joystick, camera ref) → `UI/GameView.cs`
- `FillBarView` → `UI/Hud/FillBarView.cs`
- `CameraController` → `Camera/CameraController.cs`

## Configuration
- `GameConfig` (ScriptableObject) — central game balance: cash defaults, entity radii, walk speeds, inventory limits, shop products, hotel configs
- Entity-specific configs loaded from `Resources/{Entity}Configs/`

## Third-Party Dependencies
- **DOTween** — tweening/animation (`Assets/Plugins/Demigiant/DOTween/`)
- **Google AdMob** — banner, interstitial, rewarded ads (`Assets/GoogleMobileAds/`)
- **Unity IAP** — in-app purchases (`com.unity.purchasing`)
- **TextMesh Pro** — text rendering
- **EDM4U** — native dependency management

## Coding Conventions
- Namespace: `GorodiskiGames.PerfectHotel.*`
- Custom DI: use `[Inject]` attribute, register via `Context.Install<T>()`
- Config values go in ScriptableObjects, not hardcoded
- UI follows `BehaviourWithModel<T>` pattern with `Mediator` base

## Build
- Build scenes defined in `ProjectSettings/EditorBuildSettings.asset`:
  1. Gameplay.unity
  2. Hotel1.unity
  3. Hotel2.unity
- Portrait-only orientation on mobile
- 60 FPS target, VSync disabled

## Git
- Remote: https://github.com/wizdorm/wizdorm.git
- Branch: master
- `.gitignore` excludes Library/, Temp/, Obj/, Build/, Logs/, UserSettings/, .csproj, .sln files
