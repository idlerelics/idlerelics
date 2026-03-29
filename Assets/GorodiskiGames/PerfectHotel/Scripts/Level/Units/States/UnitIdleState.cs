namespace Game.Level.Unit
{
    public sealed class UnitIdleState : UnitState
    {
        public override void Initialize()
        {
            _unit.View.NavMeshAgent.enabled = false;
            _unit.View.Idle(_unit.View.Sex, 0);
        }

        public override void Dispose()
        {
        }
    }
}