namespace Game.Level.Loader.LoaderStates
{
    public sealed class LoaderArrivedToUtilityState : LoaderState
    {
        public override void Initialize()
        {
            _loader.UnitView.Idle(_loader.UnitView.Sex, _loader.Inventories);

            _loader.FireStaffArrivedToUtility();
        }

        public override void Dispose()
        {
        }
    }
}

