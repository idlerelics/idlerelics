using Injection;
using Game.Core;
using UnityEngine;

namespace Game.Level.Cash.States
{
    /// <summary>
    /// State for when a cash/coin object is flying toward the player (being collected).
    /// Smoothly moves the cash from its current position to the player's aim position
    /// using Lerp over a short duration, then fires the removal event.
    /// </summary>
    public sealed class CashFlyToPlayerState : CashState
    {
        private const float _flyTime = .2f; // Very fast flight -- 0.2 seconds to reach the player

        [Inject] private GameManager _gameManager; // Needed to get the player's current position
        [Inject] private Timer _timer;             // Per-frame tick events

        private float _timeElapsed;       // Time accumulated since flight started
        private Vector3 _startPosition;   // Where the cash started flying from

        public override void Initialize()
        {
            _startPosition = _cash.View.transform.position; // Capture starting position

            _timer.TICK += OnTICK;
        }

        /// <summary>
        /// Each frame, moves the cash toward the player using Lerp.
        /// Note: the target position is read fresh each frame (not cached) because
        /// the player might be moving while the cash flies toward them.
        /// </summary>
        private void OnTICK()
        {
            // Get the player's current aim position (updated each frame in case player moves)
            Vector3 targetPosition = _gameManager.Player.View.AimPosition;
            _cash.View.transform.position = Vector3.Lerp(_startPosition, targetPosition, _timeElapsed / _flyTime);
            _timeElapsed += Time.deltaTime;

            // When close enough, trigger removal (cash is collected)
            float distance = Vector3.Distance(_cash.View.transform.position, targetPosition);
            if (distance > 0.05f) return;

            _cash.FireRemoveCash(); // Notify the system to add the cash value and destroy this object
        }

        public override void Dispose()
        {
            _timer.TICK -= OnTICK;
        }
    }
}
