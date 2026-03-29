using Game.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Game.UI.Hud
{
    /// <summary>
    /// The main gameplay HUD that is always visible during play.
    /// Displays the player's current cash, level, and progress toward the next level.
    /// Also holds references to navigation buttons (hotels, players, settings, shop).
    ///
    /// Extends BaseHudWithModel&lt;GameModel&gt; so it automatically updates whenever
    /// the GameModel changes (e.g., when the player earns cash or gains progress).
    ///
    /// "sealed" means no other class can inherit from GamePlayHudView.
    /// </summary>
    public sealed class GamePlayHudView : BaseHudWithModel<GameModel>
    {
        // UI elements assigned in the Inspector via [SerializeField]
        [SerializeField] private TMP_Text _cashText;       // Displays current cash amount
        [SerializeField] private TMP_Text _lvlText;        // Displays current level number
        [SerializeField] private TMP_Text _progressText;   // Displays progress as "current/max"
        [SerializeField] private Image _progressFillImage; // Fill bar image (0..1 fillAmount)

        // Navigation buttons that open different HUD screens
        [SerializeField] private Button _hotelsButton;
        [SerializeField] private Button _playersButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _shopButton;

        // Public read-only properties for the buttons, allowing mediators to subscribe to clicks
        public Button HotelsButton => _hotelsButton;
        public Button PlayersButton => _playersButton;
        public Button SettingsButton => _settingsButton;
        public Button ShopButton => _shopButton;

        /// <summary>
        /// The maximum progress value needed to advance to the next level.
        /// Set by the mediator based on the current level configuration.
        /// "internal set" means only code within the same assembly can set this.
        /// </summary>
        public int MaxProgress { get; internal set; }

        /// <summary>
        /// The total number of levels available in the current hotel.
        /// Used to determine when the player has reached the maximum level.
        /// </summary>
        public int MaxLevels { get; internal set; }

        /// <summary>
        /// Intentionally empty -- event subscriptions are handled by the mediator.
        /// Required by the abstract BaseHud class.
        /// </summary>
        protected override void OnEnable()
        {
        }

        /// <summary>
        /// Intentionally empty -- event unsubscriptions are handled by the mediator.
        /// Required by the abstract BaseHud class.
        /// </summary>
        protected override void OnDisable()
        {
        }

        /// <summary>
        /// Called automatically whenever the GameModel changes (via Observer pattern).
        /// Updates all displayed values: cash, level, and progress bar.
        ///
        /// MathUtil.NiceCash formats large numbers into readable strings (e.g., "1.2M").
        /// The progress fill bar uses Image.fillAmount (0 to 1) for smooth visual feedback.
        /// When the player reaches the max level, the progress shows "MAX" with a full bar.
        /// </summary>
        /// <param name="model">The updated GameModel containing current cash, level, and progress.</param>
        protected override void OnModelChanged(GameModel model)
        {
            // Update cash display with formatted number
            _cashText.text = MathUtil.NiceCash(model.Cash);

            // Load and display current level
            int lvl = model.LoadLvl();
            _lvlText.text = lvl.ToString();

            // Calculate and display progress toward the next level
            int progress = model.LoadProgress();
            string progressText = progress.ToString() + "/" + MaxProgress;
            float fillAmount = (float)progress / MaxProgress;

            // If the player has reached the maximum level, show "MAX" with a full bar
            if (lvl >= MaxLevels)
            {
                progressText = "MAX";
                fillAmount = 1f;
            }

            _progressText.text = progressText;
            _progressFillImage.fillAmount = fillAmount;
        }
    }
}
