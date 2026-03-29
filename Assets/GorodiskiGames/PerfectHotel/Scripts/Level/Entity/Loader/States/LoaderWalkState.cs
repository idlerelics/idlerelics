using UnityEngine;

namespace Game.Level.Loader.LoaderStates
{
    public abstract class LoaderWalkState : LoaderUpdateState
    {
        public Vector3 _endPosition;

        public LoaderWalkState(Vector3 position)
        {
            _endPosition = position;
        }

        public override void Initialize()
        {
            base.Initialize();

            _loader.View.UnitView.Walk(_loader.Inventories);

            _loader.View.UnitView.NavMeshAgent.enabled = true;
            _loader.View.UnitView.NavMeshAgent.SetDestination(_endPosition);
            _loader.View.UnitView.NavMeshAgent.speed = _loader.Model.Speed;

            _timer.TICK += OnTick;
        }

        private void OnTick()
        {
            if (Vector3.Distance(_loader.View.UnitView.transform.position, _endPosition) > 0.05f) return;

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