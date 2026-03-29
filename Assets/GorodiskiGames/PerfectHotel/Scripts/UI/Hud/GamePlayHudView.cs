using Game.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Game.UI.Hud
{
    public sealed class GamePlayHudView : BaseHudWithModel<GameModel>
    {
        [SerializeField] private TMP_Text _cashText;
        [SerializeField] private TMP_Text _lvlText;
        [SerializeField] private TMP_Text _progressText;
        [SerializeField] private Image _progressFillImage;
        [SerializeField] private Button _hotelsButton;
        [SerializeField] private Button _playersButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _shopButton;

        public Button HotelsButton => _hotelsButton;
        public Button PlayersButton => _playersButton;
        public Button SettingsButton => _settingsButton;
        public Button ShopButton => _shopButton;

        public int MaxProgress { get; internal set; }
        public int MaxLevels { get; internal set; }

        protected override void OnEnable()
        {
        }

        protected override void OnDisable()
        {
        }

        protected override void OnModelChanged(GameModel model)
        {
            _cashText.text = MathUtil.NiceCash(model.Cash);

            int lvl = model.LoadLvl();
            _lvlText.text = lvl.ToString();

            int progress = model.LoadProgress();
            string progressText = progress.ToString() + "/" + MaxProgress;
            float fillAmount = (float)progress / MaxProgress;

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