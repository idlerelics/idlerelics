namespace Game.Level.Cleaner
{
    /// <summary>
    /// The first state a cleaner enters when the game loads.
    /// Checks whether the cleaner has been purchased and its area is unlocked,
    /// then immediately transitions to the appropriate state:
    /// - CleanerIdleState if already purchased and area is open
    /// - CleanerReadyToPurchaseState if it can be bought (area open + enough progress)
    /// - CleanerHiddenState if the player hasn't reached the requirements yet
    ///
    /// "sealed" means no other class can inherit from this one.
    /// </summary>
    public sealed class CleanerInitializeState : CleanerState
    {
        /// <summary>
        /// Runs once when this state becomes active. Determines the cleaner's
        /// initial status and routes it to the correct state.
        /// </summary>
        public override void Initialize()
        {
            // Find the area (floor/section) this cleaner belongs to
            var area = _gameManager.FindArea(_cleaner.Model.Area);

            // If both the cleaner and its area have been purchased, go straight to idle
            if (_cleaner.Model.IsPurchased && area.Model.IsPurchased)
                _cleaner.SwitchToState(new CleanerIdleState());
            else
            {
                // Check if the player has enough progress to unlock the purchase option
                if (_cleaner.IsPurchasable(area.Model.IsPurchased, _gameManager.Model.LoadProgress(), _cleaner.Model.TargetPurchaseValue))
                    _cleaner.SwitchToState(new CleanerReadyToPurchaseState());
                else
                    // Not enough progress yet -- hide the cleaner from the scene
                    _cleaner.SwitchToState(new CleanerHiddenState());
            }
        }

        /// <summary>No cleanup needed for this transient initialization state.</summary>
        public override void Dispose()
        {
        }
    }
}
