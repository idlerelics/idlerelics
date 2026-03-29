using System;
using Game.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Game.UI.Hud
{
    public sealed class ShopProductView : BaseHud
    {
        private const string _cashPattern = "{0} {1}";

        public Action<string> ON_CLICK;

        [SerializeField] private ShopProductConfig _config;
        public ShopProductConfig Config => _config;

        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _amountText;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private Image _imageRim;
        [SerializeField] private Image _imageBG;
        [SerializeField] private GameObject _imageFade;
        [SerializeField] private Color _inactiveColor;
        [SerializeField] private Color _inactiveColorRim;

        private Color _imageRimActiveColor;
        private Color _imageBGActiveColor;

        public TMP_Text PriceText => _priceText;

        private void Awake()
        {
            _imageRimActiveColor = _imageRim.color;
            _imageBGActiveColor = _imageBG.color;
        }

        public void Initialize(string price)
        {
            _titleText.text = _config.Title;

            _priceText.text = price;

            if (_config.Reward == ShopProductReward.NoAds) return;

            var config = _config as ShopProductWithScenarioConfig;
            var amount = string.Format(_cashPattern, GameConstants.CashIcon, config.Amount);
            var result = ColorUtil.ColorString(amount, Color.green);
            _amountText.text = result;
        }

        protected override void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClick);
        }

        protected override void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            ON_CLICK?.Invoke(_config.ID);
        }

        internal void SetInteractable(bool isInteractable)
        {
            _button.interactable = isInteractable;

            var imageRimColor = _imageRimActiveColor;
            var imageBGColor = _imageBGActiveColor;

            if (isInteractable == false)
            {
                imageRimColor = _inactiveColorRim;
                imageBGColor = _inactiveColor;
            }

            _imageRim.color = imageRimColor;
            _imageBG.color = imageBGColor;
            _imageFade.SetActive(isInteractable);
        }
    }
}


