using Injection;
using Game.Core;
using UnityEngine;

namespace Game.Level.Cash.States
{
    /// <summary>
    /// Abstract base class for cash flying to a fixed position (not the player).
    /// Unlike CashFlyToPlayerState, the end position is fixed (doesn't track a moving target).
    ///
    /// Two concrete subclasses:
    /// - CashFlyToPileState: flies to a pile, then goes idle (stacks on the pile)
    /// - CashFlyToRemoveState: flies to a position, then gets removed (destroyed)
    /// </summary>
    public abstract class CashFlyToPositionState : CashState
    {
        private const float _flyTime = .3f; // 0.3 seconds to reach destination

        [Inject] protected Timer _timer;

        protected Vector3 _startPosition;  // Captured when Initialize runs
        protected Vector3 _endPosition;    // Set by the subclass constructor

        private float _timeElapsed;

        public override void Initialize()
        {
            _startPosition = _cash.View.transform.position;

            _timer.TICK += OnTICK;
        }

        public override void Dispose()
        {
            _timer.TICK -= OnTICK;
        }

        /// <summary>
        /// Each frame, Lerps the cash toward the fixed end position.
        /// When it arrives, calls the abstract OnEnd() method -- subclasses decide what happens next.
        /// </summary>
        private void OnTICK()
        {
            _cash.View.transform.position = Vector3.Lerp(_startPosition, _endPosition, _timeElapsed / _flyTime);
            _timeElapsed += Time.deltaTime;

            float distance = Vector3.Distance(_cash.View.transform.position, _endPosition);
            if (distance > 0.05f) return; // Still flying

            // Snap to exact position and let the subclass handle what happens next
            _cash.View.transform.position = _endPosition;

            OnEnd();
        }

        /// <summary>Called when the cash arrives at the destination. Subclasses define the behavior.</summary>
        public abstract void OnEnd();
    }

    /// <summary>
    /// Cash flies to a pile position and then becomes idle (stacks on the pile, waiting to be collected).
    /// </summary>
    public sealed class CashFlyToPileState : CashFlyToPositionState
    {
        public CashFlyToPileState(Vector3 endPosition)
        {
            _endPosition = endPosition;
        }

        /// <summary>When the cash reaches the pile, switch to idle (just sit there).</summary>
        public override void OnEnd()
        {
            _cash.Idle();
        }
    }

    /// <summary>
    /// Cash flies to a position and then gets removed/destroyed.
    /// Used for cash that's being spent or transferred elsewhere.
    /// </summary>
    public sealed class CashFlyToRemoveState : CashFlyToPositionState
    {
        public CashFlyToRemoveState(Vector3 endPosition)
        {
            _endPosition = endPosition;
        }

        /// <summary>When the cash arrives, fire the removal event to destroy it.</summary>
        public override void OnEnd()
        {
            _cash.FireRemoveCash();
        }
    }
}
