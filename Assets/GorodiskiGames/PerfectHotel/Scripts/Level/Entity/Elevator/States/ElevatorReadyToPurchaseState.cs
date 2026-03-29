using Game.Level.Item;
using Game.Level.Player;
using Injection;

namespace Game.Level.Elevator
{
    public sealed class ElevatorReadyToPurchaseState : ElevatorState
    {
        [Inject] private GameManager _gameManager;

        public override void Initialize()
        {
            _elevator.BuyUpdateItem.Model.Duration = 1f;

            _elevator.Model.IsLocked = false;
            _elevator.Model.SetChanged();

            _elevator.View.HudView.gameObject.SetActive(true);
            _elevator.View.HudView.ReadyToPuchase();

            _elevator.View.MeshesHolder.SetActive(true);

            _elevator.View.HideWallsPurchasedState();
            _elevator.View.HideWallsHiddenState();

            _elevator.View.CloseDoor();

            OnLvlChanged(_gameManager.Model.LoadLvl());

            _elevator.BuyUpdateItem.PLAYER_ON_ITEM += PlayerOnItem;

            _gameManager.AddItem(_elevator.BuyUpdateItem);

            _gameManager.LEVEL_CHANGED += OnLvlChanged;
        }

        public override void Dispose()
        {
            _gameManager.LEVEL_CHANGED -= OnLvlChanged;

            _elevator.BuyUpdateItem.PLAYER_ON_ITEM -= PlayerOnItem;

            _gameManager.RemoveItem(_elevator.BuyUpdateItem);
        }

        private void OnLvlChanged(int lvl)
        {
            _elevator.View.OutsideWalls.MeshesVisibilityLvl(lvl);
        }

        private void PlayerOnItem(ItemController item)
        {
            if (_gameManager.Model.Cash <= 0)
                return;

            var amount = item.GetAmount(_gameManager.Model.Cash);

            _gameManager.Model.Cash -= amount;
            _gameManager.Model.SetChanged();
            _gameManager.Model.Save();

            _elevator.Model.PricePurchase -= amount;
            _elevator.Model.SetChanged();

            _gameManager.FireFlyToRemoveCash(_elevator.View.HudView.transform.position);

            if (_elevator.Model.PricePurchase > 0)
                return;

            _elevator.BuyUpdateItem.Model.Duration = 0f;

            _gameManager.Model.SavePlaceIsPurchased(_elevator.Model.ID);
            _elevator.Model.IsPurchased = _gameManager.Model.LoadPlaceIsPurchased(_elevator.Model.ID);
            _elevator.Model.SetChanged();

            _gameManager.FireElevatorPurchased();

            _elevator.SwitchToState(new ElevatorPurchasedState());
            _gameManager.Player.SwitchToState(new PlayerPauseState());

            _gameManager.FireTryShowInterstitial();
        }
    }
}

