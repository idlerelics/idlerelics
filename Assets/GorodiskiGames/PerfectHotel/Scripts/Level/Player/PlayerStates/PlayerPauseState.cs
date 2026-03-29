using Game.Core;
using Game.UI;
using Injection;

namespace Game.Level.Player
{
    public sealed class PlayerPauseState : PlayerState
    {
        [Inject] private GameView _gameView;
        [Inject] private Timer _timer;
        [Inject] private GameManager _gameManager;

        public override void Initialize()
        {
            _player.View.Idle(_player.Model.Sex, _gameManager.Model.InventoryTypes.Count);

            _timer.TICK += OnTICK;
        }

        public override void Dispose()
        {
            _timer.TICK -= OnTICK;
        }

        private void OnTICK()
        {
            if (!_gameView.Joystick.HasInput)
                return;

            _player.SwitchToState(new PlayerWalkState());
        }
    }
}

