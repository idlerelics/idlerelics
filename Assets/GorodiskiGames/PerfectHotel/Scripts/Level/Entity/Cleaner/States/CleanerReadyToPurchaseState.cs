using Game.Level.Item;
using Game.Level.Player;

namespace Game.Level.Cleaner
{
    public sealed class CleanerReadyToPurchaseState : CleanerState
    {
        public override void Initialize()
        {
            _cleaner.ItemBuyUpdate.Model.Duration = 1f;

            _cleaner.View.HudView.gameObject.SetActive(true);
            _cleaner.View.HudView.ReadyToPuchase();

            _cleaner.View.UnitView.Hide();

            _cleaner.ItemBuyUpdate.PLAYER_ON_ITEM += PlayerOnItem;

            _gameManager.AddItem(_cleaner.ItemBuyUpdate);
        }

        public override void Dispose()
        {
            _cleaner.ItemBuyUpdate.PLAYER_ON_ITEM -= PlayerOnItem;

            _gameManager.RemoveItem(_cleaner.ItemBuyUpdate);
        }

        private void PlayerOnItem(ItemController item)
        {
            if (_gameManager.Model.Cash <= 0)
                return;

            int amount = item.GetAmount(_gameManager.Model.Cash);

            _gameManager.Model.Cash -= amount;
            _gameManager.Model.SetChanged();
            _gameManager.Model.Save();

            _cleaner.Model.PricePurchase -= amount;
            _cleaner.Model.SetChanged();

            _gameManager.FireFlyToRemoveCash(_cleaner.View.HudView.transform.position);

            if (_cleaner.Model.PricePurchase > 0)
                return;

            _cleaner.ItemBuyUpdate.Model.Duration = 0f;

            _gameManager.Model.SavePlaceIsPurchased(_cleaner.Model.ID);
            _cleaner.Model.IsPurchased = _gameManager.Model.LoadPlaceIsPurchased(_cleaner.Model.ID);
            _cleaner.Model.SetChanged();

            _cleaner.UnitView.PlayUnitParticles();

            _gameView.CameraController.SetTarget(_cleaner.UnitView.transform);
            _gameView.CameraController.ZoomIn(true);

            _cleaner.SwitchToState(new CleanerIdleState());
            _gameManager.Player.SwitchToState(new PlayerPauseState());

            _gameManager.FireTryShowInterstitial();
        }
    }
}