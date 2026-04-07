using Game.Level.Item;
using UnityEngine;

namespace Game.Level.Cleaner
{
    public sealed class CleanerCleaningState : CleanerUpdateState
    {
        private ItemController _item;

        public CleanerCleaningState(ItemController item)
        {
            _item = item;
        }

        public override void Initialize()
        {
            base.Initialize();

            _cleaner.View.UnitView.NavMeshAgent.enabled = false;
            _cleaner.View.UnitView.Clean();

            _timer.TICK += OnTick;
            _item.CLAIM_REVOKED += OnClaimRevoked;
        }

        public override void Dispose()
        {
            base.Dispose();

            _timer.TICK -= OnTick;
            _item.CLAIM_REVOKED -= OnClaimRevoked;
        }

        /// <summary>
        /// Player stole this item mid-clean. Stop the animation loop and go idle so the
        /// cleaner can pick a different target instead of standing here forever.
        /// </summary>
        private void OnClaimRevoked(ItemController _)
        {
            _cleaner.SwitchToState(new CleanerIdleState());
        }

        private void OnTick()
        {
            _item.Model.Duration -= Time.deltaTime;
            _item.Model.SetChanged();

            if (_item.Model.Duration > 0f) return;

            // Remove from the registry so the cleaner doesn't immediately re-find this same
            // already-cleaned torch on her next scan. The room re-adds items on the next
            // dig cycle via RoomUsedState.Initialize.
            _gameManager.RemoveItem(_item);
            _item.Release(_cleaner); // Clear the reservation so the next dig cycle starts clean
            _item.FireItemFinished();

            _cleaner.SwitchToState(new CleanerWalkHomeState(_cleaner.InitialPosition));
        }
    }
}

