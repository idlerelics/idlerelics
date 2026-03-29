namespace Game.Level.Loader.LoaderStates
{
    public sealed class LoaderInitializeState : LoaderState
    {
        public override void Initialize()
        {
            var area = _gameManager.FindArea(_loader.Model.Area);

            if (_loader.Model.IsPurchased && area.Model.IsPurchased)
            {
                _loader.Idle();
                _loader.FireStaffPurchased();
            }
            else
            {
                if (_loader.IsPurchasable(area.Model.IsPurchased, _gameManager.Model.LoadProgress(), _loader.Model.TargetPurchaseValue))
                    _loader.ReadyToPurchase();
                else
                    _loader.Hidden();
            }
        }

        public override void Dispose()
        {
        }
    }
}