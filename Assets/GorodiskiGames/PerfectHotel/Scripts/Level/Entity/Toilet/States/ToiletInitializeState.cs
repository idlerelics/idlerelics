namespace Game.Level.Toilet
{
    public sealed class ToiletInitializeState : ToiletState
    {
        public override void Initialize()
        {
            var area = _gameManager.FindArea(_toilet.Model.Area);
            if (_toilet.Model.IsPurchased && area.Model.IsPurchased)
                _toilet.SwitchToState(new ToiletPurchasedState());
            else
            {
                if (_toilet.IsPurchasable(area.Model.IsPurchased, _gameManager.Model.LoadProgress(), _toilet.Model.TargetPurchaseValue))
                    _toilet.SwitchToState(new ToiletReadyToPurchaseState());
                else _toilet.SwitchToState(new ToiletHiddenState());
            }
        }

        public override void Dispose()
        {
        }
    }
}