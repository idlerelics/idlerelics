namespace Game.Level.Utility.UtilityStates
{
    public sealed class UtilityInitializeState : UtilityState
    {
        public override void Initialize()
        {
            var area = _gameManager.FindArea(_utility.Model.Area);
            if (_utility.IsPurchasable(area.Model.IsPurchased, _gameManager.Model.LoadProgress(), _utility.Model.TargetPurchaseValue))
                _utility.SwitchToState(new UtilityPurchasedState());
            else _utility.SwitchToState(new UtilityHiddenState());
        }

        public override void Dispose()
        {
        }
    }
}