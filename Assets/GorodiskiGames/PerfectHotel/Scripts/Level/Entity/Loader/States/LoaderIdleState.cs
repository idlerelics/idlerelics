using UnityEngine;

namespace Game.Level.Loader.LoaderStates
{
    public sealed class LoaderIdleState : LoaderUpdateState
    {
        public override void Initialize()
        {
            base.Initialize();

            _loader.View.UnitView.transform.eulerAngles = new Vector3(0f, 180f, 0f);

            _loader.View.UnitView.Unhide();
            _loader.View.UnitView.NavMeshAgent.enabled = false;

            _loader.View.UnitView.Idle(_loader.UnitView.Sex, _loader.Inventories);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}

