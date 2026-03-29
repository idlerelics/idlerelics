using Game.Level.Item;
using Game.Level.Player;

namespace Game.Level.Loader.LoaderStates
{
    public sealed class LoaderReadyToPurchaseState : LoaderState
    {
        public override void Initialize()
        {
            _loader.ItemBuyUpdate.Model.Duration = 1f;

            _loader.View.HudView.gameObject.SetActive(true);
            _loader.View.HudView.ReadyToPuchase();

            _loader.View.UnitView.Hide();

            _loader.ItemBuyUpdate.PLAYER_ON_ITEM += PlayerOnItem;

            _gameManager.AddItem(_loader.ItemBuyUpdate);
        }

        public override void Dispose()
        {
            _loader.ItemBuyUpdate.PLAYER_ON_ITEM -= PlayerOnItem;

            _gameManager.RemoveItem(_loader.ItemBuyUpdate);
        }

        void PlayerOnItem(ItemController item)
        {
            if (_gameManager.Model.Cash <= 0)
                return;

            var amount = item.GetAmount(_gameManager.Model.Cash);

            _gameManager.Model.Cash -= amount;
            _gameManager.Model.SetChanged();
            _gameManager.Model.Save();

            _loader.Model.PricePurchase -= amount;
            _loader.Model.SetChanged();

            _gameManager.FireFlyToRemoveCash(_loader.View.HudView.transform.position);

            if (_loader.Model.PricePurchase > 0)
                return;

            _loader.ItemBuyUpdate.Model.Duration = 0f;

            _gameManager.Model.SavePlaceIsPurchased(_loader.Model.ID);
            _loader.Model.IsPurchased = _gameManager.Model.LoadPlaceIsPurchased(_loader.Model.ID);
            _loader.Model.SetChanged();

            _loader.UnitView.PlayUnitParticles();

            _gameView.CameraController.SetTarget(_loader.UnitView.transform);
            _gameView.CameraController.ZoomIn(true);

            _loader.Idle();
            _loader.FireStaffPurchased();
            _gameManager.Player.SwitchToState(new PlayerPauseState());

            _gameManager.FireTryShowInterstitial();
        }
    }
}
