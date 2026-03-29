using Game.Level.Area;
using Injection;

namespace Game.Level.Elevator
{
    public sealed class ElevatorHiddenState : ElevatorState
    {
        [Inject] private GameManager _gameManager;

        private AreaController _area;

        public override void Initialize()
        {
            _area = _gameManager.FindArea(_elevator.Model.Area);

            _elevator.View.HudView.gameObject.SetActive(false);
            _elevator.View.MeshesHolder.SetActive(false);

            _elevator.View.HideWallsPurchasedState();
            _elevator.View.HideWallsHiddenState();

            _gameManager.AREA_PURCHASED += OnAreaPurchased;
        }

        public override void Dispose()
        {
            _gameManager.AREA_PURCHASED -= OnAreaPurchased;
        }

        private void OnAreaPurchased(AreaController area)
        {
            Log.Info("ElevatorHiddenState. OnAreaPurchased.");

            if (area.View.Config.Number != _area.View.Config.Number) return;

            Log.Info("ElevatorHiddenState. OnAreaPurchased. Area.");

            CheckIsPurchasable(_gameManager.Model.LoadLvl());
        }

        private void CheckIsPurchasable(int lvl)
        {
            if (_elevator.IsPurchasable(_gameManager.Model.LoadLvl(), _elevator.Model.TargetPurchaseValue))
                _elevator.SwitchToState(new ElevatorReadyToPurchaseState());
            else _elevator.SwitchToState(new ElevatorWaitForLevelState());
        }
    }
}

