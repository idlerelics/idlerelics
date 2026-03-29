using UnityEngine;

namespace Game.Level.Cleaner
{
    public abstract class CleanerWalkState : CleanerUpdateState
    {
        public Vector3 _endPosition;

        public CleanerWalkState(Vector3 position)
        {
            _endPosition = position;
        }

        public override void Initialize()
        {
            base.Initialize();

            _cleaner.View.UnitView.Walk(0);

            _cleaner.View.UnitView.NavMeshAgent.enabled = true;
            _cleaner.View.UnitView.NavMeshAgent.SetDestination(_endPosition);
            _cleaner.View.UnitView.NavMeshAgent.speed = _cleaner.Model.Speed;

            _timer.TICK += OnTick;
        }

        private void OnTick()
        {
            if (Vector3.Distance(_cleaner.View.UnitView.transform.position, _endPosition) > 0.05f) return;

            OnReachDistance();
        }

        public abstract void OnReachDistance();

        public override void Dispose()
        {
            base.Dispose();

            _timer.TICK -= OnTick;
        }
    }
}