using Injection;
using Game.Core;
using UnityEngine;

namespace Game.Level.Cash.States
{
    public sealed class CashFlyToPlayerState : CashState
    {
        private const float _flyTime = .2f;

        [Inject] private GameManager _gameManager;
        [Inject] private Timer _timer;

        private float _timeElapsed;
        private Vector3 _startPosition;

        public override void Initialize()
        {
            _startPosition = _cash.View.transform.position;

            _timer.TICK += OnTICK;
        }

        private void OnTICK()
        {
            Vector3 targetPosition = _gameManager.Player.View.AimPosition;
            _cash.View.transform.position = Vector3.Lerp(_startPosition, targetPosition, _timeElapsed / _flyTime);
            _timeElapsed += Time.deltaTime;

            float distance = Vector3.Distance(_cash.View.transform.position, targetPosition);
            if (distance > 0.05f) return;

            _cash.FireRemoveCash();
        }

        public override void Dispose()
        {
            _timer.TICK -= OnTICK;
        }
    }
}