using Game.Level.Item;
using Injection;

namespace Game.Level.Area
{
    public sealed class AreaReadyToPurchaseState : AreaState
    {
        [Inject] private GameManager _gameManager;

        public override void Initialize()
        {
            _area.ItemBuyUpdate.Model.Duration = 1f;

            _area.Model.IsLocked = false;
            _area.Model.SetChanged();

            _area.View.HudView.gameObject.SetActive(true);
            _area.View.HudView.ReadyToPuchase();

            _area.View.HideFloors();
            _area.View.HideHidingWalls();
            _area.View.HidePermanentWalls();

            _area.ItemBuyUpdate.PLAYER_ON_ITEM += PlayerOnItem;

            _gameManager.AddItem(_area.ItemBuyUpdate);
        }

        public override void Dispose()
        {
            _area.ItemBuyUpdate.PLAYER_ON_ITEM -= PlayerOnItem;

            _gameManager.RemoveItem(_area.ItemBuyUpdate);
        }

        private void PlayerOnItem(ItemController item)
        {
            if (_gameManager.Model.Cash <= 0)
                return;

            var amount = item.GetAmount(_gameManager.Model.Cash);

            _gameManager.Model.Cash -= amount;
            _gameManager.Model.SetChanged();
            _gameManager.Model.Save();

            _area.Model.PricePurchase -= amount;
            _area.Model.SetChanged();

            _gameManager.FireFlyToRemoveCash(_area.View.HudView.transform.position);

            if (_area.Model.PricePurchase > 0)
                return;

            _area.ItemBuyUpdate.Model.Duration = 0f;

            _gameManager.Model.SavePlaceIsPurchased(_area.Model.ID);
            _area.Model.IsPurchased = _gameManager.Model.LoadPlaceIsPurchased(_area.Model.ID);
            _area.Model.SetChanged();

            _gameManager.FireAreaPurchased(_area);

            _area.SwitchToState(new AreaPurchasedState());

            _gameManager.FireTryShowInterstitial();
        }
    }
}

