using Game.Core;
using Injection;

namespace Game.Level.Utility.UtilityStates
{
    /// <summary>
    /// Abstract base state for all utility states (e.g., UtilityInitializeState,
    /// UtilityPurchasedState, UtilityHiddenState).
    ///
    /// Provides shared injected dependencies that every utility state needs:
    /// the utility controller itself and the game manager. Subclasses can access
    /// these via the "protected" keyword (visible to child classes, hidden from outside).
    ///
    /// The Initialize() and Dispose() methods have empty default implementations,
    /// so subclasses only need to override the ones they actually use.
    /// </summary>
    public abstract class UtilityState : State
    {
        [Inject] protected UtilityController _utility;   // The utility entity this state belongs to
        [Inject] protected GameManager _gameManager;     // Central game manager for accessing game-wide data

        /// <summary>
        /// Default empty Dispose -- subclasses override this if they need cleanup.
        /// Having a default implementation avoids forcing every state to write
        /// an empty Dispose() method.
        /// </summary>
        public override void Dispose()
        {
        }

        /// <summary>
        /// Default empty Initialize -- subclasses override this if they need setup.
        /// </summary>
        public override void Initialize()
        {
        }
    }
}
