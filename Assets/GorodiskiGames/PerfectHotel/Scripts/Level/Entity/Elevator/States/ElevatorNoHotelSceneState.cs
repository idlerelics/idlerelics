namespace Game.Level.Elevator
{
    public sealed class ElevatorNoHotelSceneState : ElevatorState
    {
        public override void Initialize()
        {
            _elevator.View.HudView.gameObject.SetActive(false);
            _elevator.View.MeshesHolder.SetActive(false);

            _elevator.View.HideWallsPurchasedState();
            _elevator.View.HideWallsHiddenState();
        }

        public override void Dispose()
        {
        }
    }
}

