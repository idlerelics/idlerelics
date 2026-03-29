using Game.Level.Item;
using Game.Level.Player;
using Injection;

namespace Game.Level.Elevator
{
    /// <summary>
    /// State for an elevator that is ready to be purchased by the player.
    /// The elevator is visible with its meshes and HUD showing the purchase price.
    /// When the player stands on the buy zone, cash is deducted incrementally
    /// until the full price is paid and the elevator transitions to the purchased state.
    ///
    /// This follows the same purchase pattern used across entities (rooms, areas, etc.):
    /// player steps on trigger -> cash deducted per tick -> purchase completes when price reaches 0.
    /// </summary>
    public sealed class ElevatorReadyToPurchaseState : ElevatorState
    {
        [Inject] private GameManager _gameManager;

        /// <summary>
        /// Sets up the purchase-ready visual state and subscribes to purchase events.
        /// Shows the elevator meshes with the door closed and the purchase HUD active.
        /// </summary>
        public override void Initialize()
        {
            // Set the item's interaction duration (non-zero means the item is active)
            _elevator.BuyUpdateItem.Model.Duration = 1f;

            // Mark the elevator as unlocked so the player can interact with it
            _elevator.Model.IsLocked = false;
            _elevator.Model.SetChanged(); // Notify observers (updates any bound UI)

            // Show the HUD with the purchase price
            _elevator.View.HudView.gameObject.SetActive(true);
            _elevator.View.HudView.ReadyToPuchase();

            // Show the elevator meshes (the physical 3D model)
            _elevator.View.MeshesHolder.SetActive(true);

            // Hide walls that belong to other states (purchased or hidden)
            _elevator.View.HideWallsPurchasedState();
            _elevator.View.HideWallsHiddenState();

            // Close the elevator door (it opens only when purchased and in use)
            _elevator.View.CloseDoor();

            // Update wall visibility based on the player's current level
            OnLvlChanged(_gameManager.Model.LoadLvl());

            // Subscribe to the player stepping on the buy zone
            _elevator.BuyUpdateItem.PLAYER_ON_ITEM += PlayerOnItem;

            // Register the buy item with the game manager so the player can interact with it
            _gameManager.AddItem(_elevator.BuyUpdateItem);

            // Listen for level changes to update outside wall visibility
            _gameManager.LEVEL_CHANGED += OnLvlChanged;
        }

        /// <summary>
        /// Cleans up event subscriptions and unregisters the buy item.
        /// </summary>
        public override void Dispose()
        {
            _gameManager.LEVEL_CHANGED -= OnLvlChanged;

            _elevator.BuyUpdateItem.PLAYER_ON_ITEM -= PlayerOnItem;

            _gameManager.RemoveItem(_elevator.BuyUpdateItem);
        }

        /// <summary>
        /// Updates the visibility of outside walls based on the current level.
        /// Some walls only appear at certain levels to reveal or hide parts of the building.
        /// </summary>
        /// <param name="lvl">The player's current hotel level.</param>
        private void OnLvlChanged(int lvl)
        {
            _elevator.View.OutsideWalls.MeshesVisibilityLvl(lvl);
        }

        /// <summary>
        /// Called each tick while the player stands on the elevator's buy zone.
        /// Deducts cash from the player and applies it toward the purchase price.
        /// When fully paid, the elevator is marked as purchased, the state transitions,
        /// and the camera focuses on the newly purchased elevator.
        /// </summary>
        /// <param name="item">The item controller representing the buy trigger zone.</param>
        private void PlayerOnItem(ItemController item)
        {
            // Guard: can't purchase without cash
            if (_gameManager.Model.Cash <= 0)
                return;

            // Calculate how much cash to deduct this tick
            var amount = item.GetAmount(_gameManager.Model.Cash);

            // Deduct from the player's balance and persist
            _gameManager.Model.Cash -= amount;
            _gameManager.Model.SetChanged();
            _gameManager.Model.Save();

            // Apply the payment toward the elevator's purchase price
            _elevator.Model.PricePurchase -= amount;
            _elevator.Model.SetChanged(); // Update the price display on the HUD

            // Show the flying cash VFX heading toward the HUD
            _gameManager.FireFlyToRemoveCash(_elevator.View.HudView.transform.position);

            // If there's still money owed, wait for more ticks
            if (_elevator.Model.PricePurchase > 0)
                return;

            // === Purchase complete! ===

            // Disable further interaction with the buy item
            _elevator.BuyUpdateItem.Model.Duration = 0f;

            // Persist the purchase and update the model
            _gameManager.Model.SavePlaceIsPurchased(_elevator.Model.ID);
            _elevator.Model.IsPurchased = _gameManager.Model.LoadPlaceIsPurchased(_elevator.Model.ID);
            _elevator.Model.SetChanged();

            // Notify the game that an elevator was purchased (may trigger other systems)
            _gameManager.FireElevatorPurchased();

            // Transition to the purchased state and pause the player for the celebration
            _elevator.SwitchToState(new ElevatorPurchasedState());
            _gameManager.Player.SwitchToState(new PlayerPauseState());

            // Try to show an interstitial ad after a major purchase
            _gameManager.FireTryShowInterstitial();
        }
    }
}
