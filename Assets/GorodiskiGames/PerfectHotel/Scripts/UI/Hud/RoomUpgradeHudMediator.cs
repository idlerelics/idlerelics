using System.Collections.Generic;
using Game.Core.UI;
using Game.Level;
using Game.Level.Room;
using Injection;
using UnityEngine;

namespace Game.UI.Hud
{
    /// <summary>
    /// Mediator for the Room Upgrade HUD that appears when a player upgrades a room.
    /// Presents a selection of visual styles (wall designs) the player can choose from.
    /// After selection, the room's appearance changes and the player earns progress.
    ///
    /// This mediator also handles camera zoom to focus on the room being upgraded,
    /// and awards progress points when the upgrade is complete.
    ///
    /// Follows the Mediator pattern: the view (RoomUpgradeHudView) holds UI references,
    /// while this class manages the business logic.
    /// </summary>
    public class RoomUpgradeHudMediator : Mediator<RoomUpgradeHudView>
    {
        // Injected dependencies resolved by the custom DI container
        [Inject] private GameManager _gameManager;   // Manages game state, player, and model
        [Inject] private GameView _gameView;         // Root UI view with joystick and camera access
        [Inject] private LevelView _levelView;       // Level-specific data (max progress, max levels)

        // The specific room being upgraded, passed in via the constructor
        private RoomController _room;

        /// <summary>
        /// List of instantiated room visual style slots. Tracked for cleanup on hide.
        /// "protected" allows subclasses to access this list.
        /// </summary>
        protected List<RoomSlotView> _slots;

        /// <summary>
        /// Constructor receives the room that the player is upgrading.
        /// In the Mediator pattern, the constructor is where context-specific
        /// data is provided before the HUD is shown.
        /// </summary>
        /// <param name="room">The room controller being upgraded.</param>
        public RoomUpgradeHudMediator(RoomController room)
        {
            _room = room;
            _slots = new List<RoomSlotView>();
        }

        /// <summary>
        /// Called when the HUD is shown. Sets up the upgrade selection screen:
        /// 1. Hides the joystick (player cannot move during upgrade selection)
        /// 2. Creates visual style slot buttons from the room's available designs
        /// 3. Displays the room's current level
        /// 4. Zooms the camera in on the room being upgraded
        /// 5. Subscribes to app quit to save progress if the user leaves
        /// </summary>
        protected override void Show()
        {
            // Disable player movement while selecting a room style
            _gameView.Joystick.gameObject.SetActive(false);

            InsantiateSlots();

            _view.SetLvl(_room.Model.Lvl);

            // Focus the camera on the room being upgraded
            _gameView.CameraController.SetTarget(_room.View.transform);
            _gameView.CameraController.ZoomIn(false);

            // Save progress if the app is closed during selection
            _view.ON_APP_QUIT += OnApplicationQuit;
        }

        /// <summary>
        /// Called when the HUD is hidden. Cleans up all event subscriptions
        /// and destroys all instantiated slot GameObjects.
        /// Always destroy instantiated prefabs and unsubscribe events to prevent memory leaks.
        /// </summary>
        protected override void Hide()
        {
            _view.ON_APP_QUIT -= OnApplicationQuit;

            // Unsubscribe click events from all slots
            foreach (var slot in _slots)
            {
                slot.ON_SLOT_CLICK -= OnSlotClick;
            }

            // Destroy all instantiated slot GameObjects
            foreach (var slot in _slots)
            {
                GameObject.Destroy(slot.gameObject);
            }
            _slots.Clear();
        }

        /// <summary>
        /// Called when the player selects a visual style for the room.
        /// Saves the chosen style index, re-enables the joystick, and closes the HUD.
        ///
        /// SetChanged() notifies all observers of the room model that data has changed,
        /// triggering the room's visual update to reflect the new style.
        /// </summary>
        /// <param name="visualIndex">The index of the selected visual style.</param>
        private void OnSlotClick(int visualIndex)
        {
            // Re-enable player movement
            _gameView.Joystick.gameObject.SetActive(true);

            // Apply and persist the selected visual style
            _room.Model.VisualIndex = visualIndex;
            _gameManager.Model.SavePlaceVisualIndex(_room.Model.ID, _room.Model.VisualIndex);
            _room.Model.SetChanged();

            OnCloseButtonClick();
        }

        /// <summary>
        /// Handles closing the upgrade HUD. Awards progress points, returns the camera
        /// to follow the player, and zooms back out to the normal view.
        ///
        /// FireAddGameProgress triggers a visual progress animation at the room's position.
        /// </summary>
        private void OnCloseButtonClick()
        {
            // Award progress points with a visual effect at the room's position
            _gameManager.FireAddGameProgress(_room.View.transform.position, _room.Model.UpdateProgressReward);

            // Return camera to following the player
            _gameView.CameraController.SetTarget(_gameManager.Player.View.transform);
            _gameView.CameraController.ZoomOut();

            InternalHide();
        }

        /// <summary>
        /// Creates one slot button for each available visual style (wall design).
        /// Each slot shows a preview icon and its index.
        ///
        /// _room.View.InsideWalls.Icons contains the sprite previews for each style option.
        /// Slots are instantiated as children of the view's container for proper layout.
        /// </summary>
        private void InsantiateSlots()
        {
            for (int i = 0; i < _room.View.InsideWalls.Icons.Length; i++)
            {
                RoomSlotView slot = GameObject.Instantiate(_view.RoomSlotPrefab, _view.Container).GetComponent<RoomSlotView>();
                slot.Initialize(_room.View.InsideWalls.Icons[i], i);
                _slots.Add(slot);
                slot.ON_SLOT_CLICK += OnSlotClick;
            }
        }

        /// <summary>
        /// Called when the application is about to quit while the upgrade HUD is open.
        /// Ensures progress is saved before the app closes.
        /// </summary>
        private void OnApplicationQuit()
        {
            AddRewardProgress();
        }

        /// <summary>
        /// Adds the room's upgrade progress reward to the player's total progress,
        /// checks if the player has reached a new level, and saves everything.
        ///
        /// This method handles the persistence side of progress -- the visual
        /// feedback (floating numbers, etc.) is handled by FireAddGameProgress.
        /// </summary>
        private void AddRewardProgress()
        {
            int progress = _gameManager.Model.LoadProgress();
            var progressReward = _room.Model.UpdateProgressReward;

            progress += progressReward;

            _gameManager.Model.SaveProgress(progress);

            CheckIfReachNewLvl(progress);

            // Save all changes and notify observers
            _gameManager.Model.Save();
            _gameManager.Model.SetChanged();
        }

        /// <summary>
        /// Checks if the accumulated progress has reached the threshold for a new level.
        /// If so, increments the level (up to the maximum allowed).
        ///
        /// _levelView.MaxProgress(lvl) returns the progress needed for the given level.
        /// _levelView.MaxLevels returns the total number of levels available.
        /// </summary>
        /// <param name="progress">The player's current total progress.</param>
        private void CheckIfReachNewLvl(int progress)
        {
            int lvl = _gameManager.Model.LoadLvl();
            if (progress >= _levelView.MaxProgress(lvl))
            {
                lvl++;

                // Only save the new level if it does not exceed the maximum
                if (lvl <= _levelView.MaxLevels)
                    _gameManager.Model.SaveLvl(lvl);
            }
        }
    }
}
