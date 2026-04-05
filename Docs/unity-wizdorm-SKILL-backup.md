---
name: unity-wizdorm
description: >
  Unity game development skill for Lost Chambers: Idle Relics (formerly WizDorm), an archaeology exploration
  idle management game. Covers C# scripting using the project's custom DI container, Observable/Observer
  pattern, state machines, and module system. Also covers game design decisions for idle/management gameplay,
  Blender-to-Unity asset pipeline, and codebase navigation.
  Use this skill whenever the user mentions Unity, C#, game code, scripting, adding features or entities,
  game design, idle game mechanics, Blender export, or anything related to building Lost Chambers. Also trigger
  when the user asks to understand, explain, debug, or modify existing game code — even if they don't
  explicitly say "Unity". If they reference chambers, tunnels, areas, NPCs, artifacts, cash, upgrades, workers,
  excavation, supply delivery, branching, or any gameplay system, this skill applies.
---

# Unity Lost Chambers Development Skill

You're helping build **Lost Chambers: Idle Relics**, a mobile idle management game (think *My Perfect Hotel* meets *Indiana Jones*). The player is a lead archaeologist exploring ancient sites — managing expedition workers, delivering supplies, excavating sealed tomb chambers, and collecting artifacts. Built in **Unity 6** with **URP 2D**, targeting mobile in portrait orientation.

This skill helps you write code, design features, navigate the codebase, and make game design decisions that fit the project's architecture and genre.

## CRITICAL: Theme Context

The codebase uses legacy "PerfectHotel" / "Hotel" / "Room" / "Guest" naming throughout. The game is being re-themed to archaeology. When writing new code or modifying existing code, understand these mappings:

| Code Name (Legacy) | Game Concept | Notes |
|---|---|---|
| Hotel | Expedition Site | Pyramid, Jungle Temple, etc. |
| Room | Tomb Chamber | Sealed, then excavated |
| Guest/Customer | Expedition Worker | Arrives, digs, needs supplies |
| Reception | Base Camp | Worker registration/assignment |
| Cleaning | Supply Delivery | Water, tools, torches |
| Cash | Artifact Value | Randomized by rarity tier |
| Elevator | Tunnel/Passage | Site transitions |
| Toilet | Rest/Supply Station | Depletes, needs restocking |
| Cleaner NPC | Excavation Crew | Auto-clears debris |
| Loader NPC | Artifact Transporter | Auto-carries relics |

When the user says "chamber" they mean a Room in code. When they say "worker" they mean a Customer/Unit. When they say "base camp" they mean Reception. Always translate between game concepts and code names.

## Project Location

```
Assets/GorodiskiGames/PerfectHotel/Scripts/
```
The "PerfectHotel" naming is legacy. Follow the existing folder structure.

## Architecture Quick Reference

The codebase uses homegrown patterns (no Zenject, no third-party DI).

### Dependency Injection

```csharp
var context = new Context();
context.Install(timer, config, gameManager);
context.ApplyInstall(); // Triggers reflection-based field injection

[Inject] private Timer _timer;
[Inject] private GameConfig _config;
```

Key behaviors:
- `Install()` registers singletons in a `Dictionary<Type, object>`.
- `ApplyInstall()` scans for `[Inject]` fields via reflection (`BindingFlags.NonPublic | Instance`).
- Child contexts inherit from parents: `new Context(parentContext)`.
- Injection happens *after* construction — `[Inject]` fields are null in constructors. Use `Initialize()`.

### Observable / Observer

```csharp
model.SetChanged(); // Calls OnObjectChanged() on all observers

public class RoomHudView : BehaviourWithModel<RoomModel>, IObserver
{
    protected override void OnModelChanged(RoomModel model) { /* Update visuals */ }
}
```

`BehaviourWithModel<T>` handles subscribe/unsubscribe automatically. Defers `OnModelChanged()` until after `Start()`.

### State Machines

```csharp
public sealed class MyFeatureState : State
{
    [Inject] private GameManager _gameManager;
    public override void Initialize() { /* Enter state */ }
    public override void Dispose() { /* Exit state */ }
}
_stateManager.SwitchToState<MyFeatureState>(); // Uses cache
```

States need **public parameterless constructor**. Use `Initialize()` for setup, not constructor. States are cached.

### Module System

```csharp
public sealed class MyFeatureModule : Module<MyFeatureView>
{
    [Inject] private GameManager _gameManager;
    public override void Initialize() { /* Setup */ }
    public override void Dispose() { /* Cleanup */ }
}
```

Module initialization order in `GamePlayState`:
1. `ReceptionModule` (Base Camp)
2. `EntityModule` (Areas, chambers, tunnel, crew)
3. `ToiletModule` (Supply stations)
4. `UtilityModule` (Supply distribution)
5. `CashModule` (Artifact value collection)
6. `UISpritesModule`
7. `UINotificationModule`

### MVC Entity Pattern

| Layer | Role | Example |
|-------|------|---------|
| **Config** | ScriptableObject | `RoomConfig` |
| **View** | MonoBehaviour in scene | `RoomView` |
| **Model** | `EntityModel` (extends `Observable`) | `RoomModel` |
| **Controller** | Logic + state machine | `RoomController` |

### Player Interaction System

`PlayerInteractionFactory` maps `ItemType` → `PlayerState`:
```csharp
_interactionFactory.Register(ItemType.Clean, item => new PlayerCleaningState(item));
_interactionFactory.Register(ItemType.ReceptionDesk, item => new PlayerReceptionState(item));
_interactionFactory.Register(ItemType.BuyUpdate, item => new PlayerOnItemState(item));
_interactionFactory.Register(ItemType.ShowHud, item => new PlayerElevatorState(item));
```

### Chamber (Room) State Machine — Core Gameplay Loop

```
RoomInitializeState
  ├→ RoomHiddenState (locked: not enough progress OR branch dependency)
  ├→ RoomReadyToPurchaseState (unlocked, awaiting purchase)
  ├→ RoomUsedState (needs supplies — "dirty" in code)
  └→ RoomAvailableState (supplied, ready for worker)

RoomAvailableState → RoomOccupiedState (worker excavating)
RoomOccupiedState → RoomUsedState (artifact found, needs resupply)
RoomUsedState → RoomAvailableState (supplies delivered)
```

Key fields: `StayDuration` (excavation time), `StayFee` (artifact base value), `CleaningTime` (supply time), `IsPurchased`, `Cash`.

## Current Development Plan (Mechanics First)

### Phase 1: Core Re-theme (config only)
Adjust GameConfig timing for archaeology feel. No code changes.

### Phase 2: Multiple Supply Types (HARDEST PHASE)
- Extend `InventoryType`: `WaterCanteen`, `Pickaxe`, `Torch`
- Add `RequiredSupplyType` to items. Type-checking in `UtilityModule`.
- Files: `Level/Inventory/`, `Modules/UtilityModule/`, `Level/Entity/Item/`, `Level/Entity/Toilet/`

### Phase 3: Artifact Loot System
- New `ArtifactModule` + `ArtifactConfig` ScriptableObject
- Hook `RoomOccupiedState` → `RoomUsedState`, replace flat `StayFee` with rarity roll
- Tiers: Common 70%, Rare 20%, Epic 8%, Legendary 2%

### Phase 4: Discovery Reveal
- DOTween sequence on first chamber unlock. Particles + camera + UI popup.

### Phase 5: Branching System
- `BranchId` + `BranchDependency` in `RoomConfig`. Branch completion → unhide next group.

### Phase 6: Second Character
- Second `PlayerConfig` in `GameConfig.PlayersMap`. Existing `PlayerSelectionState`.

### Phase 7: Site 2 (Jungle Temple)
- New scene, different layout, harder configs.

## Naming Conventions

| Type | Pattern | Example |
|------|---------|---------|
| Controller | `{Feature}Controller.cs` | `ArtifactController.cs` |
| View | `{Feature}View.cs` | `ArtifactView.cs` |
| Model | `{Feature}Model.cs` | `ArtifactModel.cs` |
| State | `{Feature}{Action}State.cs` | `ChamberExcavatingState.cs` |
| Config | `{Feature}Config.cs` | `ArtifactConfig.cs` |
| Module | `{Feature}Module.cs` | `ArtifactModule.cs` |

**Fields:** `_lowerCamelCase` private, `PascalCase` public.
**Events:** `ON_ACTION_VERB` (e.g., `ON_ARTIFACT_DISCOVERED`).
**Namespaces:** `Game.Level.{Feature}` for entities, `Game.Modules.{Feature}` for modules.

## Persistence

PlayerPrefs + JSON (`JsonUtility`). Entity keys: `"Hotel" + hotelIndex + entityType + number`. Per-entity: `"IsUsed"`, `"IsPurchased"`, `"Lvl"`, `"Cash"` + ID. Large numbers as strings.

## Game Design Principles

- **Core loop:** Workers arrive → assigned to chambers → excavate → need supplies → player delivers → artifacts found → collect → unlock new chambers
- **Discovery is the emotional core.** Every chamber unlock should feel exciting.
- **Three supply types** (water, tools, torches) > MPH's single cleaning mechanic
- **No tourists.** Revenue = artifact collection only. Explicit design decision.
- **Branching paths:** Choice of order, not exclusion. 1-2 per site.
- **Idle + active balance.** Reward both efficient active play and passive accumulation.

## Blender Pipeline

FBX export, -Z Forward / Y Up, 1 unit = 1m. Reassign URP materials. Power-of-2 textures. Art style: chunky cartoony 3D, warm golden lighting. Static → `ResourcesStatic/`, dynamic → `Resources/`.

## Debugging

- Missing dependency → check `Install()` before `ApplyInstall()`
- Observer not firing → check `SetChanged()` and `AddObserver()`
- State not initializing → needs parameterless constructor, use `Initialize()`
- Module order bugs → check `GamePlayState` initialization order
- Save corruption → `PlayerPrefs.DeleteAll()`

## Key Files

| What | Where |
|------|-------|
| Bootstrap | `GameStartBehaviour.cs` |
| Gameplay state | `States/GamePlayState.cs` |
| Central logic | `Managers/GameManager.cs` |
| Events | `Managers/GameEventBus.cs` |
| Items | `Managers/ItemRegistry.cs` |
| Save data | `Domain/GameModel.cs` |
| Config | `Config/GameConfig.cs` |
| Player | `Level/Player/PlayerController.cs` |
| Interactions | `Level/Player/PlayerInteractionFactory.cs` |
| Chambers | `Level/Entity/Room/` + `States/` |
| Cash/artifacts | `Modules/CashModule/` |
| Base camp | `Modules/ReceptionModule/` |
| Supply delivery | `Modules/UtilityModule/` |
| Supply stations | `Modules/ToiletModule/` |
| Entity setup | `Modules/EntityModule/` |
| DI container | `Core/Inject/Context.cs` |
| Game design | `Docs/GAME_DESIGN.md` |
