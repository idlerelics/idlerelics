using Game.Level;
using Game.Managers;
using Game.UI;
using Game.UI.Hud;
using Injection;
using UnityEngine;

namespace Game.Modules.UISpritesModule
{
    /// <summary>
    /// Module that handles the flying sprite/star animation when the player earns
    /// game progress (e.g., after purchasing or upgrading an entity).
    ///
    /// When progress is earned:
    /// 1. Disables the joystick so the player can't move during the animation
    /// 2. Spawns particle/sprite effects that fly from the world position to the UI
    /// 3. Each sprite arrival increments the progress counter by 1
    /// 4. When all sprites finish, re-enables the joystick
    /// 5. If progress crosses a level threshold, shows the "Level Up" HUD
    ///
    /// "sealed" means no other class can inherit from this one.
    /// </summary>
    public sealed class UISpritesModule : Module<UISpritesModuleView>
    {
        // Dependencies injected by the custom DI system
        [Inject] private GameManager _gameManager;   // Central game state management
        [Inject] private GameView _gameView;         // Top-level view (camera, joystick, etc.)
        [Inject] private LevelView _levelView;       // Level-specific references and config
        [Inject] private HudManager _hudManager;     // Manages showing/hiding UI panels

        public UISpritesModule(UISpritesModuleView view) : base(view)
        {
        }

        /// <summary>Subscribes to the game progress event to trigger sprite animations.</summary>
        public override void Initialize()
        {
            _gameManager.ADD_GAME_PROGRESS += AddGameProgress;
        }

        /// <summary>Unsubscribes from the game progress event on cleanup.</summary>
        public override void Dispose()
        {
            _gameManager.ADD_GAME_PROGRESS -= AddGameProgress;
        }

        /// <summary>
        /// Called when progress is earned somewhere in the game world.
        /// Converts the 3D world position to screen coordinates and spawns
        /// flying sprite particles that travel toward the UI progress indicator.
        /// </summary>
        /// <param name="startPosition">World-space position where the progress was earned.</param>
        /// <param name="progressDelta">Number of progress points earned (number of sprites to spawn).</param>
        private void AddGameProgress(Vector3 startPosition, int progressDelta)
        {
            // Ignore zero or negative progress
            if (progressDelta <= 0)
                return;

            // Disable the joystick to prevent player movement during the animation
            _gameView.Joystick.gameObject.SetActive(false);

            // Convert 3D world position to 2D screen position and spawn flying sprites
            _view.ShowParticles(progressDelta, _gameView.CameraController.Camera.WorldToScreenPoint(startPosition));

            // Subscribe to sprite completion events
            _view.ON_ALL_FINISHED += OnAllFinished;
            _view.ON_ONE_FINISHED += OnOneFinished;
        }

        /// <summary>
        /// Called each time one flying sprite reaches its destination.
        /// Increments the persistent progress counter by 1, checks if the player
        /// has reached a new level, and saves the updated state.
        /// </summary>
        private void OnOneFinished()
        {
            int progress = _gameManager.Model.LoadProgress();
            progress++;
            _gameManager.Model.SaveProgress(progress);
            // Notify other systems (UI, entities) that progress has changed
            _gameManager.FireProgressChanged(progress);

            // Check if this progress point pushed the player to a new level
            CheckIfReachNewLvl(progress);

            _gameManager.Model.Save();
            _gameManager.Model.SetChanged();  // Trigger Observer notifications
        }

        /// <summary>
        /// Checks if the current progress has reached or exceeded the threshold
        /// for the next level. If so, increments the level, saves it, fires
        /// a level-changed event, and shows the "Level Up" reward HUD.
        /// </summary>
        private void CheckIfReachNewLvl(int progress)
        {
            // Compare current progress against the max progress for the current level
            if (progress >= _levelView.MaxProgress(_gameManager.Model.LoadLvl()))
            {
                int lvl = _gameManager.Model.LoadLvl();
                // Get the reward amount configured for this area/level
                var reward = _gameManager.AreasMap[lvl].View.Config.Reward;

                lvl++;

                // Only level up if we haven't exceeded the maximum number of levels
                if (lvl <= _levelView.MaxLevels)
                {
                    _gameManager.Model.SaveLvl(lvl);
                    _gameManager.FireLevelChanged(_gameManager.Model.LoadLvl());

                    // Show the level-up celebration HUD with the reward amount
                    _hudManager.ShowAdditional<LevelUpHudMediator>(new object[] { reward });
                }
            }
        }

        /// <summary>
        /// Called when all flying sprites have finished their animations.
        /// Re-enables the joystick so the player can move again, and
        /// unsubscribes from the sprite completion events.
        /// </summary>
        private void OnAllFinished(int progressDelta)
        {
            _gameView.Joystick.gameObject.SetActive(true);

            // Unsubscribe to avoid duplicate callbacks on the next progress event
            _view.ON_ONE_FINISHED -= OnOneFinished;
            _view.ON_ALL_FINISHED -= OnAllFinished;
        }
    }
}
