using Game.Level.Item;
using Game.Level.Player;
using Injection;

namespace Game.Level.Elevator
{
    public sealed class ElevatorWaitForLevelState : ElevatorState
    {
        [Inject] private GameManager _gameManager;

        public override void Initialize()
        {
            _elevator.Model.IsLocked = true;
            _elevator.Model.SetChanged();

            _elevator.View.HudView.gameObject.SetActive(true);
            _elevator.View.HudView.Locked();

            _elevator.View.MeshesHolder.SetActive(true);
            _elevator.View.CloseDoor();

            UpdateOutsideWalls(_gameManager.Model.LoadLvl());

            _gameManager.AddItem(_elevator.BuyUpdateItem);

            _elevator.BuyUpdateItem.PLAYER_ON_ITEM += PlayerOnItem;
            _gameManager.LEVEL_CHANGED += OnLvlChanged;
        }

        public override void Dispose()
        {
            _gameManager.RemoveItem(_elevator.BuyUpdateItem);

            _elevator.BuyUpdateItem.PLAYER_ON_ITEM -= PlayerOnItem;
            _gameManager.LEVEL_CHANGED -= OnLvlChanged;
        }

        private void OnLvlChanged(int lvl)
        {
            UpdateOutsideWalls(lvl);
            CheckIsPurchasable(lvl);
        }

        private void UpdateOutsideWalls(int lvl)
        {
            _elevator.View.OutsideWalls.MeshesVisibilityLvl(lvl);
        }

        private void CheckIsPurchasable(int lvl)
        {
            if (_elevator.IsPurchasable(lvl, _elevator.Model.TargetPurchaseValue))
                _elevator.SwitchToState(new ElevatorReadyToPurchaseState());
        }

        private void PlayerOnItem(ItemController item)
        {
            _gameManager.FireNotificationNeedLvl(item.Transform.position, _elevator.Model.TargetPurchaseValue);
            _gameManager.Player.SwitchToState(new PlayerPauseState());
            _gameManager.AddItem(item);
        }
    }
}

