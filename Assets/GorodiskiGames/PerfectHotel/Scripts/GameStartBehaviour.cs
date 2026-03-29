// These "using" statements import code from other parts of the project,
// like #include in C++ or import in Python. They let us use classes
// defined in those namespaces without typing the full path every time.
using Game.Core;
using Game.Domain;
using Game.Managers;
using Game.States;
using Injection;
using UnityEngine;

// A "namespace" groups related classes together to avoid naming conflicts.
// Think of it like a folder for your code.
namespace Game
{
    /// <summary>
    /// This is the ENTRY POINT of the entire game. It is the first script that runs.
    /// It inherits from MonoBehaviour, which is Unity's base class for any script
    /// you attach to a GameObject in a scene.
    /// "sealed" means no other class can inherit from this one.
    /// </summary>
    public sealed class GameStartBehaviour : MonoBehaviour
    {
        // A private field -- only this class can access it.
        // The underscore prefix (_) is a common C# naming convention for private fields.
        // Timer is a helper that forwards Unity's update ticks to other systems.
        private Timer _timer;
        private float _saveTimer;
        private const float SaveInterval = 5f;
        private GameModel _gameModel;

        // "{ get; private set; }" is a C# AUTO-PROPERTY.
        // Other classes can READ Context (get), but only this class can WRITE it (private set).
        public Context Context { get; private set; }

        /// <summary>
        /// Start() is a special Unity method called once when the GameObject
        /// this script is attached to first becomes active in the scene.
        /// Here it sets up everything the game needs to run.
        /// </summary>
        private void Start()
        {
            _timer = new Timer();

            // Cap the game at 60 frames per second for consistent performance.
            Application.targetFrameRate = 60;
            // Disable vertical sync so targetFrameRate takes effect.
            QualitySettings.vSyncCount = 0;
            // Keep the game running even when the app window loses focus.
            Application.runInBackground = true;

            // Context is a DEPENDENCY INJECTION CONTAINER.
            // It acts as a central "box" that holds shared services and managers.
            // Any part of the game can ask the Context for a service it needs,
            // instead of creating one itself. This keeps code loosely coupled.
            var context = new Context();

            // Install() registers objects into the container so other parts
            // of the game can retrieve them later with context.Get<T>().
            context.Install(
                new Injector(context),      // Handles injecting dependencies into objects
                new GameStateManager(),     // Manages which game state is active (init, loading, play, etc.)
                new HudManager(),           // Manages UI screens (HUD = Heads-Up Display)
                new ResourcesManager(),     // Loads assets like prefabs, textures, etc.
                new AdsManager(),           // Handles showing advertisements
                new IAPManager(),           // Handles In-App Purchases (buying items with real money)
                new LoginManager()          // Tracks player login / daily rewards
            );

            // GetComponents<Component>() grabs ALL Unity components on this same
            // GameObject and registers them in the context too.
            context.Install(GetComponents<Component>());
            context.Install(_timer);
            // ApplyInstall() finalizes all registrations and injects dependencies.
            context.ApplyInstall();

            // Tell the GameStateManager to move into the GameInitializeState,
            // which is the first state in the game's lifecycle.
            // "typeof()" returns the Type object for a class -- used here to
            // identify which state to switch to.
            context.Get<GameStateManager>().SwitchToState(typeof(GameInitializeState));

            Context = context;
        }

        /// <summary>
        /// Reload() tears down the entire game and restarts it from scratch.
        /// Dispose() is a common C# pattern for cleaning up resources.
        /// After disposing, it simply calls Start() again to rebuild everything.
        /// </summary>
        public void Reload()
        {
            Context.Get<GameStateManager>().Dispose();
            Context.Dispose();

            Start();
        }

        /// <summary>
        /// Update() is called by Unity every single frame (up to 60 times/sec here).
        /// It forwards the tick to the Timer so other systems can run per-frame logic.
        /// </summary>
        private void Update()
        {
            _timer.Update();

            _saveTimer += Time.deltaTime;
            if (_saveTimer >= SaveInterval)
            {
                _saveTimer = 0f;
                if (_gameModel == null)
                    _gameModel = Context?.Get<GameManager>()?.Model;
                _gameModel?.FlushIfDirty();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                _gameModel?.FlushIfDirty();
        }

        private void OnApplicationQuit()
        {
            _gameModel?.FlushIfDirty();
        }

        /// <summary>
        /// LateUpdate() is called every frame AFTER all Update() calls finish.
        /// Useful for things like camera follow (so the camera moves after the player).
        /// </summary>
        private void LateUpdate()
        {
            _timer.LateUpdate();
        }

        /// <summary>
        /// FixedUpdate() is called at a fixed time interval (default 50 times/sec),
        /// independent of frame rate. Used for physics calculations.
        /// </summary>
        private void FixedUpdate()
        {
            _timer.FixedUpdate();
        }
    }
}
