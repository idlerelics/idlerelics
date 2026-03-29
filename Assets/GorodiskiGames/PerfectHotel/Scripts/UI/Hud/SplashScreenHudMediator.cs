using Game.Config;
using Game.Core;
using Game.Core.UI;
using Game.Domain;
using Game.Managers;
using Injection;
using UnityEngine;

namespace Game.UI.Hud
{
    /// <summary>
    /// Mediator for the splash/loading screen HUD shown when the game starts.
    /// Displays a progress bar that fills over a configurable duration,
    /// then automatically hides itself to transition to gameplay.
    ///
    /// Also shows the icon of the currently selected hotel and the app version.
    ///
    /// Uses the Timer system to update the progress bar each frame.
    /// "sealed" means no other class can inherit from SplashScreenHudMediator.
    /// </summary>
    public sealed class SplashScreenHudMediator : Mediator<SplashScreenHudView>
    {
        // Injected dependencies resolved by the custom DI container
        [Inject] private GameConfig _config;   // Provides splash screen duration settings
        [Inject] private Timer _timer;         // Frame-based timer for per-frame updates

        // How long the splash screen should display (in seconds)
        private float _duration;

        // How much time has elapsed since the splash screen was shown
        private float _elapsed;

        // The scene index of the current hotel (used to find its icon)
        private int _hotel;

        /// <summary>
        /// Called when the splash screen is shown. Sets up the version text,
        /// determines the splash duration based on platform, loads the hotel icon,
        /// and starts listening for frame ticks to update the progress bar.
        ///
        /// Application.version returns the version string set in Player Settings.
        ///
        /// The #if UNITY_EDITOR preprocessor directive allows different behavior
        /// in the Unity Editor vs. on a real device (shorter duration for faster testing).
        /// </summary>
        protected override void Show()
        {
            _view.AppVersionText.text = "v" + Application.version;

            // Use mobile duration by default, but override with editor duration when in Unity Editor
            _duration = _config.SplashScreenDurationMobile;
#if UNITY_EDITOR
            _duration = _config.SplashScreenDurationEditor;
#endif

            SetIcon();
            UpdateBar();

            // Subscribe to the Timer's per-frame TICK event
            _timer.TICK += OnTICK;
        }

        /// <summary>
        /// Called when the splash screen is hidden. Unsubscribes from the timer
        /// to stop receiving frame updates.
        /// </summary>
        protected override void Hide()
        {
            _timer.TICK -= OnTICK;
        }

        /// <summary>
        /// Called every frame via the Timer. Updates the progress bar and checks
        /// if the splash duration has elapsed. When time is up, hides the splash screen.
        ///
        /// Time.deltaTime is Unity's built-in time since the last frame (in seconds).
        /// </summary>
        private void OnTICK()
        {
            UpdateBar();

            _elapsed += Time.deltaTime;

            // Auto-hide when the duration has been reached
            if (_elapsed >= _duration)
                InternalHide();
        }

        /// <summary>
        /// Updates the fill bar to show loading progress.
        /// fillAmount ranges from 0 (empty) to 1 (full), calculated as elapsed/duration.
        /// </summary>
        private void UpdateBar()
        {
            float value = _elapsed / _duration;
            _view.FillBarImage.fillAmount = value;
        }

        /// <summary>
        /// Loads the saved GameModel to find the currently selected hotel,
        /// then looks up that hotel's icon sprite from the config and displays it.
        ///
        /// GameModel.Load reads the saved JSON from PlayerPrefs.
        /// The foreach loop searches the HotelConfigMap for the matching scene index.
        /// </summary>
        private void SetIcon()
        {
            var model = GameModel.Load(_config);
            foreach (var config in _config.HotelConfigMap.Values)
            {
                if (config.SceneIndex == model.Hotel)
                {
                    _view.Icon.sprite = config.Icon;
                    break;
                }
            }
        }
    }
}
