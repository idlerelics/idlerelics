using Game.Core.UI;
using Game.Level;
using Game.Level.Place;
using Game.Managers;
using Injection;

namespace Game.UI.Hud
{
    public sealed class GamePlayHudMediator : Mediator<GamePlayHudView>
    {
        [Inject] private LevelView _levelView;
        [Inject] private GameManager _gameManager;
        [Inject] private HudManager _hudManager;

        protected override void Show()
        {
            _view.MaxLevels = _levelView.MaxLevels;

            UpdateMaxProgress(_gameManager.Model.LoadLvl());
            HotelsButtonVisibility();

            _view.Model = _gameManager.Model;

            _view.HotelsButton.onClick.AddListener(OnHotelsButtonClick);
            _view.PlayersButton.onClick.AddListener(OnPlayersButtonClick);
            _view.SettingsButton.onClick.AddListener(OnSettingsButtonClick);
            _view.ShopButton.onClick.AddListener(OnShopButtonClick);

            _gameManager.LEVEL_CHANGED += UpdateMaxProgress;
            _gameManager.ELEVATOR_PURCHASED += HotelsButtonVisibility;
        }

        protected override void Hide()
        {
            _view.HotelsButton.onClick.RemoveListener(OnHotelsButtonClick);
            _view.PlayersButton.onClick.RemoveListener(OnPlayersButtonClick);
            _view.SettingsButton.onClick.RemoveListener(OnSettingsButtonClick);
            _view.ShopButton.onClick.RemoveListener(OnShopButtonClick);

            _gameManager.LEVEL_CHANGED -= UpdateMaxProgress;
            _gameManager.ELEVATOR_PURCHASED -= HotelsButtonVisibility;
        }

        private void HotelsButtonVisibility()
        {
            string elevatorID = _gameManager.Model.GenerateEntityID(1, EntityType.Elevator, 2);
            bool isPurchased = _gameManager.Model.LoadPlaceIsPurchased(elevatorID);

            if (GameConstants.IsDebugBuild())
                isPurchased = true;

            _view.HotelsButton.gameObject.SetActive(isPurchased);
        }

        private void UpdateMaxProgress(int lvl)
        {
            _view.MaxProgress = _levelView.MaxProgress(lvl);
        }

        private void OnHotelsButtonClick()
        {
            _hudManager.ShowAdditional<HotelsHudMediator>();
        }

        private void OnPlayersButtonClick()
        {
            _hudManager.ShowAdditional<PlayersHudMediator>();
        }

        private void OnSettingsButtonClick()
        {
            _hudManager.ShowSingle<SettingsHudMediator>();
        }

        private void OnShopButtonClick()
        {
            _hudManager.ShowSingle<ShopHudMediator>();
        }
    }
}