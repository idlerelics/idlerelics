using UnityEngine;

namespace Game.Level.Unit
{
    public class UnitWalkState : UnitState
    {
        public Vector3 _endPosition;

        public UnitWalkState(Vector3 position)
        {
            _endPosition = position;
        }

        public override void Initialize()
        {
            _unit.View.Walk(0);

            _unit.View.NavMeshAgent.enabled = true;
            _unit.View.NavMeshAgent.SetDestination(_endPosition);

            _timer.TICK += OnTick;
        }

        public override void Dispose()
        {
            _timer.TICK -= OnTick;
        }

        private void OnTick()
        {
            if (Vector3.Distance(_unit.View.transform.position, _endPosition) > 0.05f) return;

            OnReachDistance();
        }

        public virtual void OnReachDistance()
        {
            _unit.SwitchToState(new UnitIdleState());
        }
    }
}