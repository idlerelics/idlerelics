using System;
using Game.Level.Item;

namespace Game.Level.Player
{
    public sealed class PlayerIdleState : PlayerFindEntityState
    {
        public override void Initialize()
        {
            base.Initialize();

            _player.View.NavMeshAgent.enabled = false;
            _player.View.Idle(_player.Model.Sex, _gameManager.Model.InventoryTypes.Count);

            _player.FireIdle();

            _gameManager.ITEM_ADDED += OnItemAdded;
            _timer.TICK += OnTICK;

            FindClosestUsedItem();
        }

        private void OnItemAdded(ItemController item)
        {
            FindClosestUsedItem();
        }

        public override void Dispose()
        {
            base.Dispose();

            _gameManager.ITEM_ADDED -= OnItemAdded;
            _timer.TICK -= OnTICK;
        }

        internal void FindClosestUsedItem()
        {
            var item = _gameManager.FindClosestUsedItem();
            if (item == null) return;

            var type = item.Type;

            if (type == ItemType.Clean)
                _player.SwitchToState(new PlayerCleaningState(item));

            else if (type == ItemType.ReceptionDesk)
                _player.SwitchToState(new PlayerReceptionState(item));

            else if (type == ItemType.BuyUpdate)
                _player.SwitchToState(new PlayerOnItemState(item));

            else if (type == ItemType.ShowHud)
                _player.SwitchToState(new PlayerElevatorState(item));
        }

        private void OnTICK()
        {
            if (!_gameView.Joystick.HasInput)
                return;

            _player.SwitchToState(new PlayerWalkState());
        }
    }
}