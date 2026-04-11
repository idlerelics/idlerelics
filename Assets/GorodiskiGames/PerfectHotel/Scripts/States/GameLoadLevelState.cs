using Game.Config;
using Game.Domain;
using Game.Level;
using Game.Managers;
using Game.UI.Hud;
using Injection;
using UnityEngine.SceneManagement;

namespace Game.States
{
    /// <summary>
    /// This state handles loading the correct hotel/level scene.
    /// It shows a splash screen, figures out which scene to load,
    /// unloads any old scenes, loads the new one, and then
    /// transitions to GamePlayState once the scene is ready.
    ///
    /// "protected" fields (instead of "private") can be accessed by
    /// classes that inherit from this one -- useful if you want to
    /// create a specialized version of this state later.
    ///
    /// "virtual" methods can be overridden by child classes to change behavior.
    /// </summary>
    public class GameLoadLevelState : GameState
    {
        // These dependencies are automatically filled in by the injection system
        [Inject] protected GameStateManager _gameStateManager;
        [Inject] protected GameConfig _config;
        [Inject] protected Context _context;
        [Inject] protected HudManager _hudManager;

        // Stores which hotel/level index to load
        private int _hotel;

        /// <summary>
        /// Called when this state becomes active.
        /// Shows a loading screen, determines the correct scene, and starts loading it.
        /// </summary>
        public override void Initialize()
        {
            // Show a splash/loading screen so the player sees something while we load
            _hudManager.ShowAdditional<SplashScreenHudMediator>();

            // Load the saved game data to find out which hotel the player is on
            var model = GameModel.Load(_config);
            _hotel = model.Hotel;

            // BOUNDS CHECKING: make sure the hotel index is valid.
            // Scene index 0 is usually the main/startup scene, so hotels start at 1.
            if (_hotel < 1) _hotel = 1;
            // SceneManager.sceneCountInBuildSettings tells us how many scenes exist.
            // We subtract 1 because indices are zero-based (0 to count-1).
            else if (_hotel >= SceneManager.sceneCountInBuildSettings)
            {
                _hotel = SceneManager.sceneCountInBuildSettings - 1;
            }

            // Save the validated hotel index back to the model
            model.Hotel = _hotel;
            model.Save();

            // DEBUG OVERRIDE: the Inspector-driven GameConfig.StartHotelOverride
            // lets us force a specific hotel without touching code or polluting
            // the save. Applied AFTER model.Save() so toggling the override off
            // returns the player to their real progress. Leave at 0 for normal
            // play. See Docs/DEVLOG.md for the history.
            if (_config.StartHotelOverride > 0)
            {
                _hotel = _config.StartHotelOverride;
                // Re-apply bounds so a bad Inspector value can't crash the load.
                if (_hotel >= SceneManager.sceneCountInBuildSettings)
                    _hotel = SceneManager.sceneCountInBuildSettings - 1;
            }

            // EVENT SUBSCRIPTION: "+=" subscribes our method to an event.
            // When Unity finishes loading a scene, it fires sceneLoaded,
            // and our OnSceneLoaded method will be called automatically.
            // Events are like a notification system -- "call me when this happens."
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Unload any previously loaded scenes (skip index 0, the main scene).
            // This loop starts at i=1 and goes up to the total scene count.
            for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                if (SceneManager.GetSceneByBuildIndex(i).isLoaded)
                {
                    // "Async" means this happens in the background without freezing the game
                    SceneManager.UnloadSceneAsync(i);
                }
            }
            LoadScene();
        }

        /// <summary>
        /// Called when leaving this state.
        /// "-=" UNSUBSCRIBES from the event so we stop receiving notifications.
        /// Always unsubscribe to prevent memory leaks and unexpected behavior!
        /// </summary>
        public override void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// Callback that fires when Unity finishes loading a scene.
        /// It finds the LevelView component in the newly loaded scene
        /// and transitions to the gameplay state.
        /// "virtual" means child classes can override this method to customize behavior.
        /// </summary>
        public virtual void OnSceneLoaded(Scene scene, LoadSceneMode arg)
        {
            LevelView level = null;
            // GetRootGameObjects() returns all top-level GameObjects in the scene
            var sceneObjects = scene.GetRootGameObjects();
            // "foreach" loops through every item in a collection, one at a time
            foreach (var sceneObject in sceneObjects)
            {
                // Try to find a LevelView component on each root object
                level = sceneObject.GetComponent<LevelView>();

                // "null" means "nothing" -- if we found a LevelView, stop searching
                // "!= null" checks that the variable actually holds a valid object
                if (null != level)
                    break;
            }

            // Register the level in the context so other systems can use it
            _context.Install(level);

            // Transition to the GamePlayState using a GENERIC method.
            // <GamePlayState> in angle brackets is a TYPE PARAMETER --
            // it tells the method which type to use without needing "new" or "typeof".
            _gameStateManager.SwitchToState<GamePlayState>();
        }

        /// <summary>
        /// Loads the hotel scene additively (on top of the current scene).
        /// LoadSceneMode.Additive means the new scene is added alongside the
        /// existing one, rather than replacing it.
        /// </summary>
        public virtual void LoadScene()
        {
            SceneManager.LoadScene(_hotel, LoadSceneMode.Additive);
        }
    }
}
