using Game.Level.Item;
using Game.Level.Player;
using Injection;

namespace Game.Level.Area
{
    /// <summary>
    /// State for an area that is not yet purchasable because the player's level
    /// is too low. The area appears locked (hidden floors and walls) and displays
    /// a "locked" HUD indicator. When the player steps on the buy zone, a
    /// notification shows the required level instead of starting a purchase.
    ///
    /// This state listens for level changes and area purchases. When the player
    /// reaches the required level (or a prerequisite area is purchased), it
    /// automatically transitions to AreaReadyToPurchaseState.
    /// </summary>
    public sealed class AreaWaitForLevelState : AreaState
    {
        [Inject] private GameManager _gameManager;

        private AreaController _prevArea; // Reference to the area before this one (prerequisite)

        /// <summary>
        /// Sets up the locked state: hides visual elements, shows the locked HUD,
        /// and subscribes to events that could unlock this area.
        /// </summary>
        public override void Initialize()
        {
            // Find the previous area (prerequisite) -- areas are numbered sequentially
            int prevAreaNumber = _area.View.Config.Number - 1;
            if (prevAreaNumber > 1)
                _prevArea = _gameManager.FindArea(prevAreaNumber);

            // Mark the area as locked and notify observers (updates UI bindings)
            _area.Model.IsLocked = true;
            _area.Model.SetChanged();

            // Show the "locked" indicator on the HUD
            _area.View.HudView.Locked();

            // Hide all visual elements since the area isn't available yet
            _area.View.HideFloors();
            _area.View.HideHidingWalls();
            _area.View.HidePermanentWalls();

            // Check if we already meet the level requirement (e.g., after loading a save)
            OnLvlChanged(_gameManager.Model.LoadLvl());

            // Register the buy item so the player can walk to it (even if just to see the "need level" message)
            _gameManager.AddItem(_area.ItemBuyUpdate);

            // Subscribe to events that could unlock this area
            _area.ItemBuyUpdate.PLAYER_ON_ITEM += PlayerOnItem;     // Player stepped on buy zone
            _gameManager.LEVEL_CHANGED += OnLvlChanged;             // Player leveled up
            _gameManager.AREA_PURCHASED += OnAreaPurchased;         // Another area was purchased
        }

        /// <summary>
        /// Cleans up: unregisters the buy item and unsubscribes from all events.
        /// </summary>
        public override void Dispose()
        {
            _gameManager.RemoveItem(_area.ItemBuyUpdate);

            _area.ItemBuyUpdate.PLAYER_ON_ITEM -= PlayerOnItem;
            _gameManager.LEVEL_CHANGED -= OnLvlChanged;
            _gameManager.AREA_PURCHASED -= OnAreaPurchased;
        }

        /// <summary>
        /// Called when any area in the game is purchased. If the purchased area
        /// is our prerequisite, re-check level requirements since area purchase
        /// chains might unlock this area.
        /// </summary>
        /// <param name="area">The area that was just purchased.</param>
        private void OnAreaPurchased(AreaController area)
        {
            if (_prevArea == null) return;
            if (area.View.Config.Number != _prevArea.View.Config.Number) return;

            // Prerequisite area was purchased -- re-evaluate our unlock status
            OnLvlChanged(_gameManager.Model.LoadLvl());
        }

        /// <summary>
        /// Called when the player's level changes. If the new level meets the
        /// requirement, transitions to AreaReadyToPurchaseState so the player
        /// can buy this area.
        /// </summary>
        /// <param name="lvl">The player's current hotel level.</param>
        private void OnLvlChanged(int lvl)
        {
            if (_area.IsPurchasable(lvl, _area.Model.TargetPurchaseValue))
                _area.SwitchToState(new AreaReadyToPurchaseState());
        }

        /// <summary>
        /// Called when the player steps onto the buy zone while the area is locked.
        /// Instead of purchasing, shows a notification telling the player what level
        /// they need. Pauses the player briefly so the notification is visible.
        /// </summary>
        /// <param name="item">The item controller the player stepped on.</param>
        private void PlayerOnItem(ItemController item)
        {
            // Show a "Need Level X" notification at the item's position
            _gameManager.FireNotificationNeedLvl(item.Transform.position, _area.Model.TargetPurchaseValue);

            // Briefly pause the player so they see the notification
            _gameManager.Player.SwitchToState(new PlayerPauseState());

            // Re-add the item so it stays interactive for future interactions
            _gameManager.AddItem(item);
        }
    }
}
