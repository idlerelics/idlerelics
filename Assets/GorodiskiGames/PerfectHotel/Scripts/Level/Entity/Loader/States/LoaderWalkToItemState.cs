using UnityEngine;

namespace Game.Level.Loader.LoaderStates
{
    public sealed class LoaderWalkToItemState : LoaderWalkState
    {
        public LoaderWalkToItemState(Vector3 position) : base(position)
        {
            _endPosition = position;
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
            _loader.FireArrivedToItem();
        }
    }
}

