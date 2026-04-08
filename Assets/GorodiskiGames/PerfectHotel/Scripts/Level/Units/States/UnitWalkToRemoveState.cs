using UnityEngine;

namespace Game.Level.Unit
{
    public sealed class UnitWalkToRemoveState : UnitWalkState
    {
        public UnitWalkToRemoveState(Vector3 position) : base(position)
        {
            _endPosition = position;
        }

        public override void Initialize()
        {
            base.Initialize();

            _unit.Area = -1;

            // Safety: if the worker was carrying a relic placeholder but is now
            // being despawned (collector full, no office in area, etc.), clean
            // up the prop so they don't walk off-screen with a floating cube.
            _unit.View.DetachRelic();
        }

        public override void OnReachDistance()
        {
            _unit.FireUnitRemove();
        }
    }
}