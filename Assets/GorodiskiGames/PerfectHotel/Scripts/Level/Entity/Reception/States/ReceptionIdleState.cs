using Game.Level.Item;
using Game.Level.Player;
using Game.UI;
using Injection;

namespace Game.Level.Reception
{
    /// <summary>
    /// The idle state for the reception desk. While in this state, the reception
    /// listens for player interaction and checks whether it can be upgraded.
    ///
    /// STATE PATTERN: This is one of several states the reception can be in.
    /// "sealed" means no other class can inherit from this -- it's the final version.
    /// The reception transitions between states (e.g., Hidden -> ReadyToPurchase -> Idle)
    /// as the player progresses through the game.
    /// </summary>
    public sealed class ReceptionIdleState : ReceptionState
    {
        // Injected dependencies -- the DI container fills these in automatically
        [Inject] private GameManager _gameManager;
        [Inject] private GameView _gameView;

        /// <summary>
        /// Called when the reception enters this idle state.
        /// Checks if the reception is eligible for an upgrade and subscribes to events.
        /// </summary>
        public override void Initialize()
        {
            // Check upgrade eligibility based on current game progress
            CheckIsUpdatable(_gameManager.Model.LoadProgress());

            // Subscribe to events:
            // PLAYER_ON_ITEM fires when the player steps onto the buy/update trigger zone
            // PROGRESS_CHANGED fires when game progress changes (e.g., rooms purchased)
            _reception.ItemBuyUpdate.PLAYER_ON_ITEM += PlayerOnItem;
            _gameManager.PROGRESS_CHANGED += CheckIsUpdatable;
        }

        /// <summary>
        /// Called when leaving this state. Unsubscribes from all events to prevent
        /// memory leaks and stale callbacks (always mirror your += with -= in Dispose).
        /// </summary>
        public override void Dispose()
        {
            _reception.ItemBuyUpdate.PLAYER_ON_ITEM -= PlayerOnItem;
            _gameManager.PROGRESS_CHANGED -= CheckIsUpdatable;
        }

        /// <summary>
        /// Determines whether the reception can be upgraded at the current progress level.
        /// Shows or hides the upgrade HUD accordingly, and registers/unregisters the
        /// buy item with the game manager so the player can interact with it.
        /// </summary>
        /// <param name="progress">The current game progress value.</param>
        private void CheckIsUpdatable(int progress)
        {
            // IsUpdatable checks: not already maxed, progress >= target, etc.
            bool isUpdatable = _reception.IsUpdatable(_reception.Model.IsMaxed, progress, _reception.Model.TargetUpdateProgress);
            _reception.View.HudView.gameObject.SetActive(isUpdatable);

            if (isUpdatable)
            {
                // Set the item's interaction duration and register it so the player can step on it
                _reception.ItemBuyUpdate.Model.Duration = 1f;
                _gameManager.AddItem(_reception.ItemBuyUpdate);
                _reception.View.HudView.ReadyToUpdate();
            }
            else _gameManager.RemoveItem(_reception.ItemBuyUpdate);
        }

        /// <summary>
        /// Called when the player steps onto the reception's upgrade trigger zone.
        /// Deducts cash from the player and applies it toward the upgrade price.
        /// When fully paid, the reception levels up and the camera zooms in for a celebration.
        /// </summary>
        /// <param name="item">The item controller the player is standing on.</param>
        void PlayerOnItem(ItemController item)
        {
            // Guard: exit early if the player has no cash or the reception can't be upgraded
            if (_gameManager.Model.Cash <= 0 || !_reception.IsUpdatable(_reception.Model.IsMaxed, _gameManager.Model.LoadProgress(), _reception.Model.TargetUpdateProgress)) return;

            // GetAmount calculates how much cash to deduct per tick (based on item's rate)
            var amount = item.GetAmount(_gameManager.Model.Cash);

            // Deduct cash from the player's balance and persist the change
            _gameManager.Model.Cash -= amount;
            _gameManager.Model.Save();
            _gameManager.Model.SetChanged(); // Notify observers (e.g., UI) that cash changed

            // Apply payment toward the upgrade cost
            _reception.Model.PriceUpdate -= amount;

            // Trigger the flying cash VFX toward the HUD
            _gameManager.FireFlyToRemoveCash(_reception.View.HudView.transform.position);

            // Check if the upgrade is fully paid for
            if (_reception.Model.PriceUpdate <= 0)
            {
                // Stop the purchase interaction
                _reception.ItemBuyUpdate.Model.Duration = 0f;

                // Level up the reception and persist the new level
                _reception.Model.Lvl++;
                _gameManager.Model.SavePlaceLvl(_reception.Model.ID, _reception.Model.Lvl);
                _reception.Model.UpdateModel(); // Recalculate stats for the new level

                // Re-check upgrade eligibility at the new level
                CheckIsUpdatable(_gameManager.Model.LoadProgress());

                // Camera celebration: zoom into the upgraded reception
                _gameView.CameraController.SetTarget(_reception.View.transform);
                _gameView.CameraController.ZoomIn(true);

                // Pause the player during the zoom animation
                _gameManager.Player.SwitchToState(new PlayerPauseState());
            }

            // Notify observers that the reception model changed (updates the price HUD)
            _reception.Model.SetChanged();
        }
    }
}
