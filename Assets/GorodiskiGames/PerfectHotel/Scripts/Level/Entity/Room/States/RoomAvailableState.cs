namespace Game.Level.Room
{
    /// <summary>
    /// State for when a room is clean and ready to accept a guest.
    /// Inherits from RoomUpdateState which handles upgrade logic.
    ///
    /// When entering this state:
    /// - Marks the room as "not used" in save data (clean)
    /// - Sets IsAvailable = true so the reception can assign guests here
    /// - Turns off the dark lighting (rooms appear darker when occupied)
    /// - Shows the room items in their clean visual state
    /// </summary>
    public sealed class RoomAvailableState : RoomUpdateState
    {
        public override void Initialize()
        {
            base.Initialize();

            // Save state: 0 = not used (clean)
            _gameManager.Model.SavePlaceIsUsed(_room.Model.ID, 0);

            _room.ResetSlots(); // Wipe any leaked reservations/workers from a previous dig cycle
            _room.AcceptingWorkers = true; // Allow workers to be assigned here (slot reservation gates the rest)

            _room.View.SetDarkLight(false); // Normal/bright lighting

            // Show all room items (bed, desk, etc.) in their clean visual
            foreach (var item in _room.View.Items)
            {
                item.SetVisual(true, _room.Model.VisualIndex);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        /// <summary>Updates room visuals when the room is upgraded to a new level.</summary>
        public override void UpdateRoomVisual(int visualIndex)
        {
            base.UpdateRoomVisual(visualIndex);

            foreach (var item in _room.Items)
            {
                item.View.SetVisual(true, visualIndex); // Show items with new visual level
            }
        }
    }
}
