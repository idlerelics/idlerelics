using Game.Core;
using Game.Level.Item;
using Game.UI;
using Injection;
using UnityEngine;

namespace Game.Level.Player
{
    public class PlayerItemState : PlayerState
    {
        [Inject] protected Timer _timer;
        [Inject] protected GameView _gameView;
        [Inject] protected GameManager _gameManager;

        protected ItemController _item;

        public PlayerItemState(ItemController item)
        {
            _item = item;
        }

        public override void Initialize()
        {
            _gameManager.RemoveItem(_item);

            _timer.TICK += OnTick;
        }

        public override void Dispose()
        {
            _timer.TICK -= OnTick;

            if (_item.Model.Duration > 0f)
                _gameManager.AddItem(_item);
        }

        private void OnTick()
        {
            if (_gameView.Joystick.HasInput)
                _player.SwitchToState(new PlayerWalkState());

            PlayerOnItem();
        }

        public virtual void PlayerOnItem()
        {
            _item.Model.Duration -= Time.deltaTime;
            _item.Model.SetChanged();

            if (_item.Model.Duration > 0f) return;
            OnItemFinished();
        }

        public virtual void OnItemFinished()
        {
            _item.FireItemFinished();
            _player.SwitchToState(new PlayerIdleState());
        }
    }
}

