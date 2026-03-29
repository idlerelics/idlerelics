using Injection;

namespace Game.Level.Area
{
    public sealed class AreaInitializeState : AreaState
    {
        [Inject] private GameManager _gameManager;

        public override void Initialize()
        {
            if (_area.Model.IsPurchased)
                _area.SwitchToState(new AreaPurchasedState());
            else
            {
                if (_area.IsPurchasable(_gameManager.Model.LoadLvl(), _area.Model.TargetPurchaseValue))
                    _area.SwitchToState(new AreaReadyToPurchaseState());
                else _area.SwitchToState(new AreaWaitForLevelState());
            }
        }

        public override void Dispose()
        {
        }
    }
}