using Injection;

namespace Game.Level.Area
{
    /// <summary>
    /// The initial state for an area entity. Acts as a decision router that checks
    /// the area's current status and transitions to the appropriate long-lived state:
    ///
    /// - Already purchased  -> AreaPurchasedState (area is active and usable)
    /// - Ready to purchase  -> AreaReadyToPurchaseState (player can buy it now)
    /// - Not yet available  -> AreaWaitForLevelState (player needs a higher level)
    ///
    /// This three-way branching ensures the area always starts in the correct visual
    /// and interactive state, even after loading a saved game.
    /// </summary>
    public sealed class AreaInitializeState : AreaState
    {
        [Inject] private GameManager _gameManager;

        /// <summary>
        /// Checks the area's purchase/level status and routes to the correct state.
        /// IsPurchasable compares the player's current hotel level against the
        /// area's TargetPurchaseValue (the minimum level required to unlock it).
        /// </summary>
        public override void Initialize()
        {
            if (_area.Model.IsPurchased)
                _area.SwitchToState(new AreaPurchasedState());
            else
            {
                // Check if the player's level meets the requirement to purchase this area
                if (_area.IsPurchasable(_gameManager.Model.LoadLvl(), _area.Model.TargetPurchaseValue))
                    _area.SwitchToState(new AreaReadyToPurchaseState());
                else _area.SwitchToState(new AreaWaitForLevelState());
            }
        }

        /// <summary>
        /// No cleanup needed -- this state transitions immediately during Initialize
        /// and does not subscribe to any events or allocate resources.
        /// </summary>
        public override void Dispose()
        {
        }
    }
}