namespace Game.Level.Utility.UtilityStates
{
    /// <summary>
    /// The initial state for a utility entity. Determines whether the utility
    /// should start in its "purchased" state or remain "hidden" based on the
    /// area's purchase status and the current game progress.
    ///
    /// This state is entered once during construction and immediately transitions
    /// to the appropriate long-lived state -- it acts as a router/decision point
    /// in the state machine.
    /// </summary>
    public sealed class UtilityInitializeState : UtilityState
    {
        /// <summary>
        /// Evaluates whether the utility is purchasable (its parent area is purchased
        /// and the player has enough progress). Routes to the correct follow-up state:
        /// - UtilityPurchasedState if the utility is already available
        /// - UtilityHiddenState if the player hasn't unlocked it yet
        /// </summary>
        public override void Initialize()
        {
            // Find the area this utility belongs to and check purchase eligibility
            var area = _gameManager.FindArea(_utility.Model.Area);
            if (_utility.IsPurchasable(area.Model.IsPurchased, _gameManager.Model.LoadProgress(), _utility.Model.TargetPurchaseValue))
                _utility.SwitchToState(new UtilityPurchasedState());
            else _utility.SwitchToState(new UtilityHiddenState());
        }

        /// <summary>
        /// No cleanup needed -- this state transitions immediately and doesn't
        /// subscribe to any events.
        /// </summary>
        public override void Dispose()
        {
        }
    }
}