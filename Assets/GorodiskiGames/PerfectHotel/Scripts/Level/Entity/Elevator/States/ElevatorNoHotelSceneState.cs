namespace Game.Level.Elevator
{
    /// <summary>
    /// State for an elevator when the hotel scene it connects to has not been loaded.
    /// This typically happens when the elevator leads to a higher hotel level (e.g., Hotel2)
    /// that the player hasn't unlocked yet or that isn't part of the current scene.
    ///
    /// In this state, the elevator is completely hidden -- no meshes, no HUD, no walls.
    /// It becomes a "ghost" in the scene until the connected hotel scene is loaded.
    /// </summary>
    public sealed class ElevatorNoHotelSceneState : ElevatorState
    {
        /// <summary>
        /// Hides all elevator visuals. The elevator effectively doesn't exist in
        /// the scene until its hotel scene is loaded and it transitions to another state.
        /// </summary>
        public override void Initialize()
        {
            _elevator.View.HudView.gameObject.SetActive(false);    // Hide the HUD (price/level display)
            _elevator.View.MeshesHolder.SetActive(false);          // Hide the 3D model

            // Hide both sets of contextual walls -- neither purchased nor hidden walls should show
            _elevator.View.HideWallsPurchasedState();
            _elevator.View.HideWallsHiddenState();
        }

        /// <summary>
        /// No cleanup needed -- this state doesn't subscribe to any events or
        /// allocate any resources. The elevator simply remains invisible.
        /// </summary>
        public override void Dispose()
        {
        }
    }
}
