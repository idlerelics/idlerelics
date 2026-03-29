using UnityEngine;

namespace Game.Level.Cleaner
{
    public sealed class CleanerWalkHomeState : CleanerWalkState
    {
        public CleanerWalkHomeState(Vector3 position) : base(position)
        {
            _endPosition = position;
        }

        public override void Initialize()
        {
            base.Initialize();

            _gameManager.ITEM_ADDED += FindUsedItem;

            FindUsedItem(null);
        }

        public override void Dispose()
        {
            base.Dispose();

            _gameManager.ITEM_ADDED -= FindUsedItem;
        }

        public override void OnReachDistance()
        {
            _cleaner.SwitchToState(new CleanerIdleState());
        }
    }
}