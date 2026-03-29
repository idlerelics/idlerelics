using Game.Level.Item;
using Game.Level.Player;

namespace Game.Level.Loader.LoaderStates
{
    /// <summary>
    /// Base state that provides upgrade logic for the loader. This is not a standalone
    /// state -- it's meant to be extended by states where the loader can be upgraded
    /// (e.g., LoaderIdleState). When the player steps on the upgrade zone, cash is
    /// deducted until the upgrade price is paid, then the loader levels up.
    ///
    /// This pattern (upgrade logic in a base state) avoids duplicating the same
    /// purchase code across multiple states where the loader is upgradeable.
    /// </summary>
    public class LoaderUpdateState : LoaderState
    {
        /// <summary>
        /// Sets up upgrade monitoring: enables the buy item, updates the HUD,
        /// checks if the loader is eligible for an upgrade, and subscribes to events.
        /// </summary>
        public override void Initialize()
        {
            // Set the interaction duration (non-zero means the item is active)
            _loader.ItemBuyUpdate.Model.Duration = 1f;
            _loader.View.HudView.ReadyToUpdate();

            // Check upgrade eligibility based on current progress
            CheckIsUpdatable(_gameManager.Model.LoadProgress());

            // Subscribe to events for player interaction and progress changes
            _loader.ItemBuyUpdate.PLAYER_ON_ITEM += PlayerOnItem;
            _gameManager.PROGRESS_CHANGED += CheckIsUpdatable;
        }

        /// <summary>
        /// Cleans up event subscriptions. Always mirror += with -= to prevent
        /// memory leaks and stale callbacks when leaving this state.
        /// </summary>
        public override void Dispose()
        {
            _loader.ItemBuyUpdate.PLAYER_ON_ITEM -= PlayerOnItem;
            _gameManager.PROGRESS_CHANGED -= CheckIsUpdatable;
        }

        /// <summary>
        /// Determines whether the loader can be upgraded at the current progress level.
        /// Shows or hides the upgrade HUD and registers/unregisters the buy item
        /// with the game manager accordingly.
        /// </summary>
        /// <param name="progress">The current game progress value.</param>
        private void CheckIsUpdatable(int progress)
        {
            // IsUpdatable checks: not already maxed AND progress >= required target
            bool isUpdatable = _loader.IsUpdatable(_loader.Model.IsMaxed, progress, _loader.Model.TargetUpdateProgress);
            _loader.View.HudView.gameObject.SetActive(isUpdatable);

            if (isUpdatable)
                _gameManager.AddItem(_loader.ItemBuyUpdate);    // Register so player can interact
            else
                _gameManager.RemoveItem(_loader.ItemBuyUpdate); // Unregister to prevent interaction
        }

        /// <summary>
        /// Called each tick while the player stands on the loader's upgrade zone.
        /// Deducts cash and applies it toward the upgrade cost. When fully paid,
        /// the loader levels up, gets new stats, and plays a celebration effect.
        /// </summary>
        /// <param name="item">The item controller the player is standing on.</param>
        private void PlayerOnItem(ItemController item)
        {
            // Guard: exit early if the player has no cash or the loader can't be upgraded
            if (_gameManager.Model.Cash <= 0 || !_loader.IsUpdatable(_loader.Model.IsMaxed, _gameManager.Model.LoadProgress(), _loader.Model.TargetUpdateProgress)) return;

            // Calculate the amount to deduct this tick (based on the item's rate)
            var amount = item.GetAmount(_gameManager.Model.Cash);

            // Deduct from the player's cash and persist
            _gameManager.Model.Cash -= amount;
            _gameManager.Model.Save();
            _gameManager.Model.SetChanged(); // Notify observers (updates cash display)

            // Apply payment toward the upgrade cost
            _loader.Model.PriceUpdate -= amount;

            // Show the flying cash VFX toward the HUD
            _gameManager.FireFlyToRemoveCash(_loader.View.HudView.transform.position);

            // Check if the upgrade is fully paid
            if (_loader.Model.PriceUpdate <= 0)
            {
                // Disable further purchase interaction
                _loader.ItemBuyUpdate.Model.Duration = 0f;

                // Level up the loader and persist the new level
                _loader.Model.Lvl++;
                _gameManager.Model.SavePlaceLvl(_loader.Model.ID, _loader.Model.Lvl);
                _loader.Model.UpdateModel(); // Recalculate stats (speed, next price) for the new level

                // Re-check upgrade eligibility at the new level
                CheckIsUpdatable(_gameManager.Model.LoadProgress());

                // Play upgrade celebration particles on the loader character
                _loader.UnitView.PlayUnitParticles();

                // Camera zoom: focus on the upgraded loader for a celebration moment
                _gameView.CameraController.SetTarget(_loader.UnitView.transform);
                _gameView.CameraController.ZoomIn(true);

                // Pause the player during the camera animation
                _gameManager.Player.SwitchToState(new PlayerPauseState());

                // Try to show an interstitial ad after a major purchase
                _gameManager.FireTryShowInterstitial();
            }

            // Notify observers that the loader model changed (updates the price HUD)
            _loader.Model.SetChanged();
        }
    }
}
