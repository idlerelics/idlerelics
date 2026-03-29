using Game.Level.Item;

namespace Game.Level.Room
{
    /// <summary>
    /// State for when a room is dirty (after a guest leaves) and needs cleaning.
    /// Each room item (bed, desk, etc.) becomes a cleaning task with a duration.
    /// The player (or a cleaner NPC) must interact with each item to clean it.
    ///
    /// When all items are cleaned, the room transitions back to RoomAvailableState.
    /// </summary>
    public sealed class RoomUsedState : RoomUpdateState
    {
        public override void Initialize()
        {
            base.Initialize();

            // Save state: 1 = used/dirty
            _gameManager.Model.SavePlaceIsUsed(_room.Model.ID, 1);

            _room.View.SetDarkLight(false); // Room is visible (not occupied)

            // Set up each room item as a cleaning task
            foreach (var item in _room.Items)
            {
                item.View.SetVisual(false, _room.Model.VisualIndex); // Show "dirty" visual

                // Set the cleaning duration based on room level
                item.Model.Duration = _room.Model.CleaningTime;
                item.Model.DurationNominal = item.Model.Duration; // Store nominal for progress bar
                item.Model.SetChanged(); // Notify UI to show the progress bar

                // Add to GameManager's item list so the player can interact with it
                _gameManager.AddItem(item);

                // Listen for when the player finishes cleaning this item
                item.ITEM_FINISHED += OnItemFinished;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            // Unsubscribe from all items
            foreach (var item in _room.Items)
            {
                item.ITEM_FINISHED -= OnItemFinished;
            }
        }

        /// <summary>
        /// Called when a single item in the room is cleaned.
        /// Checks if ALL items are clean -- if so, the room becomes available again.
        ///
        /// 'as' is a safe cast -- returns null if the cast fails (unlike (Type) which throws).
        /// </summary>
        void OnItemFinished(ItemController item)
        {
            ItemRoomController itemRoom = item as ItemRoomController;

            item.ITEM_FINISHED -= OnItemFinished; // Unsubscribe from this specific item

            itemRoom.View.SetVisual(true, _room.Model.VisualIndex); // Show "clean" visual

            if (RoomCleaned()) // Check if all items are now clean
                _room.SwitchToState(new RoomAvailableState());
        }

        /// <summary>Returns true if all items in the room have been cleaned (Duration <= 0).</summary>
        private bool RoomCleaned()
        {
            foreach (var item in _room.Items)
            {
                if (item.Model.Duration > 0f)
                    return false; // This item still needs cleaning
            }
            return true; // All clean!
        }

        public override void UpdateRoomVisual(int visualIndex)
        {
            base.UpdateRoomVisual(visualIndex);
            foreach (var item in _room.Items)
            {
                // Items that are already cleaned show clean visual; dirty ones show dirty
                bool isAvailable = item.Model.Duration <= 0f;
                item.View.SetVisual(isAvailable, visualIndex);
            }
        }
    }
}
