using Game.Level.Item;
using UnityEngine;

namespace Game.Level.Cleaner
{
    public sealed class CleanerWalkToItemState : CleanerWalkState
    {
        private ItemController _item;
        private static Vector3 position;

        public CleanerWalkToItemState(ItemController item) : base(position)
        {
            _item = item;
            _endPosition = item.Transform.position;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void OnReachDistance()
        {
            _cleaner.SwitchToState(new CleanerCleaningState(_item));
        }
    }
}