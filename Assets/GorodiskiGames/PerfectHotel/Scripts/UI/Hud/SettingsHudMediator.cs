using Game.Config;
using Game.Core.UI;
using Game.Managers;
using Injection;

namespace Game.UI.Hud
{
    public sealed class SettingsHudMediator : Mediator<SettingsHudView>
    {
        [Inject] private HudManager _hudManager;
        [Inject] private GameConfig _config;
        [Inject] private GameManager _gameManager;
        [Inject] private AdsManager _adsManager;
        [Inject] private IAPManager _IAPManager;
        [Inject] private GameView _gameView;

        protected override void Show()
        {
            _view.DeveloperButton.gameObject.SetActive(GameConstants.IsDebugBuild());

#if UNITY_ANDROID
            _view.RestorePurchasesButton.gameObject.SetActive(false);
#endif

            JoystickVisibilityToggleVisibility();

            _view.CloseButton.onClick.AddListener(OnCloseButtonClick);
            _view.DeveloperButton.onClick.AddListener(OnDeveloperButtonClick);
            _view.RestorePurchasesButton.onClick.AddListener(OnRestoreButtonClick);

            _view.JoystickVisibilityToggle.onValueChanged.AddListener(OnJoystickVisibilityToggleClick);

            _IAPManager.ON_PRODUCT_PURCHASED += OnProductPurchased;
        }

        protected override void Hide()
        {
            _view.CloseButton.onClick.RemoveListener(OnCloseButtonClick);
            _view.DeveloperButton.onClick.RemoveListener(OnDeveloperButtonClick);
            _view.RestorePurchasesButton.onClick.RemoveListener(OnRestoreButtonClick);

            _view.JoystickVisibilityToggle.onValueChanged.RemoveListener(OnJoystickVisibilityToggleClick);

            _IAPManager.ON_PRODUCT_PURCHASED -= OnProductPurchased;
        }

        private void OnCloseButtonClick()
        {
            InternalHide();
        }

        private void OnDeveloperButtonClick()
        {
            _hudManager.ShowAdditional<DeveloperHudMediator>();
        }

        private void OnRestoreButtonClick()
        {
            _IAPManager.RestorePurchases();
        }


        private void OnProductPurchased(string productID)
        {
            var config = _config.ShopProductIAPMap[productID];
            var reward = config.Reward;

            if (reward == ShopProductReward.NoAds)
            {
                _gameManager.Model.IsNoAds = true;
                _gameManager.Model.Save();
                _adsManager.SetNoAds();
            }
        }

        private void OnJoystickVisibilityToggleClick(bool value)
        {
            _gameManager.Model.JoystickVisibility = value;
            _gameManager.Model.Save();

            _gameView.Joystick.Visibility(value);

            JoystickVisibilityToggleVisibility();
        }

        private void JoystickVisibilityToggleVisibility()
        {
            _view.JoystickVisibilityToggle.isOn = _gameManager.Model.JoystickVisibility;
        }
    }
}