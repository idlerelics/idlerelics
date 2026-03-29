using Game.Level.Area;
using Injection;
using UnityEngine.SceneManagement;

namespace Game.Level.Elevator
{
    public sealed class ElevatorInitializeState : ElevatorState
    {
        [Inject] private GameManager _gameManager;

        private AreaController _area;

        public override void Initialize()
        {
            _area = _gameManager.FindArea(_elevator.Model.Area);

            int nextHotelSceneIndex = _elevator.Model.NextHotel;
            if (nextHotelSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                if (_elevator.Model.IsPurchased)
                    _elevator.SwitchToState(new ElevatorPurchasedState());
                else
                {
                    if (_area.Model.IsPurchased)
                    {
                        if (_elevator.IsPurchasable(_gameManager.Model.LoadLvl(), _elevator.Model.TargetPurchaseValue))
                            _elevator.SwitchToState(new ElevatorReadyToPurchaseState());
                        else _elevator.SwitchToState(new ElevatorWaitForLevelState());
                    }
                    else _elevator.SwitchToState(new ElevatorHiddenState());
                }
            }
            else
            {
                Log.Warning("Warning. Hotel Scene Index " + nextHotelSceneIndex + " not found in Build Settings");
                _elevator.SwitchToState(new ElevatorNoHotelSceneState());
            } 
        }

        public override void Dispose()
        {
        }
    }
}