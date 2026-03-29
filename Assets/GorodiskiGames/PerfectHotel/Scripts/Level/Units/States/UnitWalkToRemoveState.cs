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
        }

        public override void OnReachDistance()
        {
            _unit.FireUnitRemove();
        }
    }
}