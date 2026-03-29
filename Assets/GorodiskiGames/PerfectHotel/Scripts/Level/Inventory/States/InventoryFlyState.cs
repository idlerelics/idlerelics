using Injection;
using Game.Core;
using UnityEngine;

namespace Game.Level.Inventory.InventoryStates
{
    /// <summary>
    /// State for when an inventory item is flying from the player to a destination
    /// (e.g., delivering toilet paper to a bathroom stall).
    ///
    /// Uses Vector3.Lerp (Linear Interpolation) to smoothly move the item from
    /// start to end position over a specified duration.
    /// Lerp(a, b, t) returns a point between 'a' and 'b' based on 't' (0 to 1).
    /// </summary>
    public sealed class InventoryFlyState : InventoryState
    {
        [Inject] private Timer _timer; // Provides per-frame TICK events

        private Vector3 _endPosition;    // Where the item is flying to
        private Vector3 _startPosition;  // Where the item started (captured on Initialize)

        private float _flyTime;          // Total time the flight should take (in seconds)
        private float _timeElapsed;      // How much time has passed since the flight started

        /// <summary>
        /// Constructor -- stores the destination and duration.
        /// The actual start position is captured in Initialize() because the item
        /// might move between construction and initialization.
        /// </summary>
        public InventoryFlyState(Vector3 endPosition, float flyTime)
        {
            _endPosition = endPosition;
            _flyTime = flyTime;
        }

        public override void Initialize()
        {
            _startPosition = _inventory.View.transform.position; // Capture current position as start

            _timer.TICK += OnTICK; // Subscribe to per-frame updates
        }

        public override void Dispose()
        {
            _timer.TICK -= OnTICK; // Always unsubscribe to prevent memory leaks
        }

        /// <summary>
        /// Called every frame. Moves the item toward the destination using Lerp.
        /// When the item is close enough (within 0.05 units), snaps it to the
        /// exact position and fires the fly-end event.
        /// </summary>
        private void OnTICK()
        {
            // Move the item: _timeElapsed / _flyTime gives a 0-to-1 ratio for Lerp
            _inventory.View.Position = Vector3.Lerp(_startPosition, _endPosition, _timeElapsed / _flyTime);
            _timeElapsed += Time.deltaTime; // Accumulate elapsed time

            if ((_inventory.View.transform.position - _endPosition).sqrMagnitude > 0.0025f) return; // Not there yet, keep flying

            // Snap to exact position and notify that the flight is complete
            _inventory.View.transform.position = _endPosition;

            _inventory.FireFlyEnd();
        }
    }
}
