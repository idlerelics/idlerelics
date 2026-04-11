using Game.Level.Item;
using UnityEngine;

namespace Game.Level.Cleaner
{
    /// <summary>
    /// State where the cleaner walks toward a specific item that needs servicing
    /// (e.g., a dirty bed that needs cleaning). When the cleaner reaches the item,
    /// it transitions to CleanerCleaningState to perform the cleaning action.
    ///
    /// "sealed" means no other class can inherit from this one.
    /// Extends CleanerWalkState, which handles the actual NavMesh navigation.
    /// </summary>
    public sealed class CleanerWalkToItemState : CleanerWalkState
    {
        private ItemController _item;          // The item the cleaner is walking toward
        private static Vector3 position;       // Placeholder required by the base constructor

        /// <summary>
        /// Constructor: stores the target item and overrides the walk destination
        /// to the item's world position.
        /// The "base(position)" call passes a default Vector3 to CleanerWalkState's
        /// constructor, but _endPosition is immediately overwritten with the item's
        /// actual position.
        /// </summary>
        public CleanerWalkToItemState(ItemController item) : base(position)
        {
            _item = item;
            _endPosition = item.Transform.position;
        }

        /// <summary>Calls base to start NavMesh walking and frame-by-frame distance checks.</summary>
        public override void Initialize()
        {
            base.Initialize();

            _item.CLAIM_REVOKED += OnClaimRevoked;
        }

        /// <summary>Calls base to unsubscribe from timer events.</summary>
        public override void Dispose()
        {
            base.Dispose();

            _item.CLAIM_REVOKED -= OnClaimRevoked;
        }

        /// <summary>
        /// Called when the cleaner arrives at the item's position.
        /// Transitions to CleanerCleaningState, passing along the item reference
        /// so the cleaning logic knows which item to work on.
        /// </summary>
        public override void OnReachDistance()
        {
            _cleaner.SwitchToState(new CleanerCleaningState(_item));
        }

        /// <summary>
        /// The player walked up and stole this item. Abandon the trip and go idle —
        /// the idle state will rescan the registry and pick a new target.
        /// </summary>
        private void OnClaimRevoked(ItemController _)
        {
            _cleaner.SwitchToState(new CleanerIdleState());
        }
    }
}
