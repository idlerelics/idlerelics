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
        }

        public override void Dispose()
        {
            base.Dispose();

            _timer.TICK -= OnTick;
        }

        private void OnTick()
        {
            _item.Model.Duration -= Time.deltaTime;
            _item.Model.SetChanged();

            if (_item.Model.Duration > 0f) return;

            _item.FireItemFinished();

            _cleaner.SwitchToState(new CleanerWalkHomeState(_cleaner.InitialPosition));
        }
    }
}

