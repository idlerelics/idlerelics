using UnityEngine;

namespace Game.Level.Unit
{
    public sealed class UnitReceptionState : UnitState
    {
        public override void Initialize()
        {
            _unit.View.NavMeshAgent.enabled = false;
            _unit.View.Reception();
            _unit.View.transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }

        public override void Dispose()
        {
        }
    }
}