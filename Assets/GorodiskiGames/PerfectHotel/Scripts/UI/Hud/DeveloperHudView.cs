using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class DeveloperHudView : BaseHud
    {
        public Action SAVE;

        private const string _progressWord = "PROGRESS ";
        private const string _lvlWord = "LEVEL ";

        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _addCashButton;
        [SerializeField] private Button _loadGameplayButton;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _rewAdsButton;

        [SerializeField] private TMP_Text _progressText;
        [SerializeField] private TMP_Text _progressMinText;
        [SerializeField] private TMP_Text _progressMaxText;
        [SerializeField] private Slider _progressSlider;

        [SerializeField] private TMP_Text _lvlText;
        [SerializeField] private TMP_Text _lvlMinText;
        [SerializeField] private TMP_Text _lvlMaxText;
        [SerializeField] private Slider _lvlSlider;

        public Button CloseButton => _closeButton;
        public Button AddCashButton => _addCashButton;
        public Button LoadGameplayButton => _loadGameplayButton;
        public Button ResetButton => _resetButton;
        public Button RewAdsButton => _rewAdsButton;

        int _progress;
        int _lvl;

        public int Progress => _progress;
        public int Lvl => _lvl;

        public void OnPointerUp()
        {
            SAVE?.Invoke();
        }

        protected override void OnEnable()
        {
            _progressSlider.onValueChanged.AddListener(OnProgressSlider);
            _lvlSlider.onValueChanged.AddListener(OnLvlSlider);
        }
        protected override void OnDisable()
        {
            _progressSlider.onValueChanged.RemoveListener(OnProgressSlider);
            _lvlSlider.onValueChanged.RemoveListener(OnLvlSlider);
        }

        public void LoadProgress(int value)
        {
            _progressSlider.value = value;
            OnProgressSlider(value);
        }
        public void LoadLvl(int lvl)
        {
            _lvlSlider.value = lvl;
            OnLvlSlider(lvl);
        }

        public void SetProgressSliderLimits(int min, int max)
        {
            _progressSlider.minValue = min;
            _progressSlider.maxValue = max;

            _progressMinText.text = min.ToString();
            _progressMaxText.text = max.ToString();
        }

        public void SetLvlSliderLimits(int min, int max)
        {
            _lvlSlider.minValue = min;
            _lvlSlider.maxValue = max;

            _lvlMinText.text = min.ToString();
            _lvlMaxText.text = max.ToString();
        }

        private void OnProgressSlider(float value)
        {
            _progress = (int)value;
            _progressText.text = _progressWord + _progress;
        }

        private void OnLvlSlider(float value)
        {
            _lvl = (int)value;
            _lvlText.text = _lvlWord + _lvl;
        }
    }
}