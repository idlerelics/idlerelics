namespace Game.Level.Cleaner
{
    public sealed class CleanerInitializeState : CleanerState
    {
        public override void Initialize()
        {
            var area = _gameManager.FindArea(_cleaner.Model.Area);
            if (_cleaner.Model.IsPurchased && area.Model.IsPurchased)
                _cleaner.SwitchToState(new CleanerIdleState());
            else
            {
                if (_cleaner.IsPurchasable(area.Model.IsPurchased, _gameManager.Model.LoadProgress(), _cleaner.Model.TargetPurchaseValue))
                    _cleaner.SwitchToState(new CleanerReadyToPurchaseState());
                else
                    _cleaner.SwitchToState(new CleanerHiddenState());
            }
        }

        public override void Dispose()
        {
        }
    }
}