using System;
using System.Collections.Generic;
using Game.Config;
using Game.Level;
using Game.Managers;
using Game.UI;
using Injection;
using UnityEngine;
using Game.Level.Player;
using Game.UI.Hud;
using Game.Level.Reception;
using Game.Modules.CashModule;
using Game.Level.Entity;
using Game.Modules.UISpritesModule;
using Game.Modules.UINotificationModule;
using Game.Modules.Utility;
using Game.Modules.ToiletModule;

namespace Game.States
{
    /// <summary>
    /// The main gameplay state -- this is where the player actually plays the game!
    /// It sets up the player character, camera, UI, joystick, and all the
    /// gameplay modules (reception, rooms, cash, etc.).
    /// "sealed" means no class can inherit from this one.
    /// </summary>
    public sealed class GamePlayState : GameState
    {
        // All [Inject] fields are automatically filled by the dependency injection system
        [Inject] private Injector _injector;       // Used to inject dependencies into new objects
        [Inject] private Context _context;          // The central service container
        [Inject] private GameView _gameView;        // References to shared UI/game objects in the scene
        [Inject] private HudManager _hudManager;    // Manages showing/hiding UI panels
        [Inject] private GameConfig _config;        // Game settings and configuration data
        [Inject] private LevelView _levelView;      // References to the current level's objects
        [Inject] private AdsManager _adsManager;    // Handles ad display

        // The GameManager handles core game logic (not injected -- created here)
        private GameManager _gameManager;

        // A List<Module> is a GENERIC COLLECTION -- like an array but resizable.
        // "readonly" means the variable itself can't be reassigned after construction,
        // but you can still add/remove items from the list.
        private readonly List<Module> _levelModules;

        /// <summary>
        /// Constructor -- called when "new GamePlayState()" is used.
        /// Initializes the list that will hold all gameplay modules.
        /// </summary>
        public GamePlayState()
        {
            _levelModules = new List<Module>();
        }

        /// <summary>
        /// Called when entering this state. Sets up everything needed for gameplay:
        /// the game manager, player character, camera, UI, joystick, and modules.
        /// </summary>
        public override void Initialize()
        {
            // Create the GameManager which holds the game model (saved data) and logic
            _gameManager = new GameManager(_config);
            _context.Install(_gameManager);
            _context.Install(_gameManager.ItemRegistry);
            _context.Install(_gameManager.EventBus);

            // Look up which player skin/character is selected
            var player = _gameManager.Model.Player;
            // ContainsKey() checks if a dictionary has an entry for the given key.
            // A DICTIONARY is like a lookup table: you give it a key, it returns a value.
            if (!_config.PlayersMap.ContainsKey(player))
                player = 0; // Fall back to default player if the saved one doesn't exist

            // Create the player model (data) and controller (behavior)
            var playerConfig = _config.PlayersMap[player];
            var model = new PlayerModel(playerConfig, _config, _gameManager);

            _gameManager.Player = new PlayerController(_gameView.PlayerView, model, _context);
            // Set the player's initial rotation (facing the camera) and position (center)
            // Vector3 holds three floats: (x, y, z)
            _gameManager.Player.View.Euler = new Vector3(0f, 180f, 0f);
            _gameManager.Player.View.Position = Vector3.zero; // (0, 0, 0)

            // Point the camera at the player and enable camera following
            _gameView.CameraController.SetPlayer(_gameManager.Player.View.transform);
            _gameView.CameraController.enabled = true;

            // Initialize all gameplay modules (reception desk, rooms, cash, etc.)
            InitLevelModules();

            // Show the gameplay UI (HUD) and purchase UI
            _hudManager.ShowAdditional<GamePlayHudMediator>();
            _hudManager.ShowAdditional<PurchaseHudMediator>();

            // Set up the on-screen joystick for player movement
            _gameView.Joystick.Visibility(_gameManager.Model.JoystickVisibility);
            _gameView.Joystick.gameObject.SetActive(true);

            // Set the player's initial state to idle (standing still)
            _gameManager.Player.SwitchToState(new PlayerIdleState());

            // Subscribe to events using "+=" (see GameLoadLevelState for more on events)
            _gameManager.ON_TRY_SHOW_INTERSTITIAL += OnTryShowInterstitial;
            _adsManager.ON_REWARDED_WATCHED += OnRewardedWatched;
        }

        /// <summary>
        /// Called when leaving this state (e.g., going back to menu or reloading).
        /// Cleans up everything that was created in Initialize().
        /// Always clean up in the reverse order of setup to avoid issues!
        /// </summary>
        public override void Dispose()
        {
            _gameView.CameraController.enabled = false;

            DisposeLevelModules();

            _hudManager.HideAdditional<GamePlayHudMediator>();
            _hudManager.HideAdditional<PurchaseHudMediator>();

            _gameView.Joystick.gameObject.SetActive(false);

            // Unsubscribe from events with "-=" to prevent memory leaks
            _gameManager.ON_TRY_SHOW_INTERSTITIAL -= OnTryShowInterstitial;
            _adsManager.ON_REWARDED_WATCHED -= OnRewardedWatched;

            _gameManager.Player.Dispose();
            _gameManager.Dispose();

            // Remove the GameManager and sub-systems from the context
            _context.Uninstall(_gameManager.ItemRegistry);
            _context.Uninstall(_gameManager.EventBus);
            _context.Uninstall(_gameManager);
        }

        /// <summary>
        /// Creates and initializes all gameplay modules.
        /// Each module handles a specific part of the game (reception, rooms, etc.).
        /// </summary>
        private void InitLevelModules()
        {
            // Each call adds a module of type T with a view of type T1.
            // The view is a Unity component that lives on a GameObject in the scene.
            AddModule<ReceptionModule, ReceptionModuleView>(_levelView);
            AddModule<EntityModule, EntityModuleView>(_levelView);
            AddModule<ToiletModule, ToiletModuleView>(_levelView);
            AddModule<UtilityModule, UtilityModuleView>(_levelView);
            AddModule<CashModule, CashModuleView>(_gameView);
            AddModule<UISpritesModule, UISpritesModuleView>(_gameView);
            AddModule<UINotificationModule, UINotificationModuleView>(_gameView);
        }

        /// <summary>
        /// A GENERIC METHOD that creates a module and its view, then initializes it.
        /// T and T1 are TYPE PARAMETERS -- placeholders for actual types.
        /// "where T : Module" is a CONSTRAINT -- it means T must be a Module or subclass.
        /// This lets us write one method that works for many different module types!
        /// </summary>
        private void AddModule<T, T1>(Component component) where T : Module
        {
            // GetComponent<T1>() finds a component of type T1 on the GameObject
            var view = component.GetComponent<T1>();
            // Activator.CreateInstance uses REFLECTION to create a new object of type T
            // at runtime, passing 'view' to its constructor. Reflection lets you
            // create objects when you don't know the exact type until the code runs.
            var result = (T)Activator.CreateInstance(typeof(T), new object[] { view });
            _levelModules.Add(result);
            // Inject dependencies into the newly created module
            _injector.Inject(result);
            result.Initialize();
        }

        /// <summary>
        /// Cleans up all gameplay modules by calling Dispose() on each one,
        /// then empties the list.
        /// </summary>
        private void DisposeLevelModules()
        {
            foreach (var levelModule in _levelModules)
            {
                levelModule.Dispose();
            }
            _levelModules.Clear();
        }

        /// <summary>
        /// Event handler: called when the game wants to show an interstitial ad.
        /// Event handlers are regular methods that get called when an event fires.
        /// </summary>
        private void OnTryShowInterstitial()
        {
            _adsManager.ShowInterstitial();
        }

        /// <summary>
        /// Event handler: called after the player finishes watching a rewarded ad.
        /// Records how many ads the player has watched (for rewards/analytics).
        /// </summary>
        private void OnRewardedWatched()
        {
            _gameManager.Model.SaveWatchAdsTimes();
        }
    }
}
