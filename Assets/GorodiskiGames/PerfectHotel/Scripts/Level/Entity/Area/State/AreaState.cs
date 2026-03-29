using Game.Core;
using Injection;

namespace Game.Level.Area
{
    /// <summary>
    /// Abstract base state for all area states (e.g., AreaInitializeState,
    /// AreaWaitForLevelState, AreaReadyToPurchaseState, AreaPurchasedState).
    ///
    /// Provides shared injected dependencies that every area state needs:
    /// the area controller and a timer for delayed actions.
    ///
    /// Unlike UtilityState, this base class does NOT provide default empty
    /// implementations for Initialize/Dispose -- they remain abstract from
    /// the State base class, so every area state subclass MUST implement them.
    /// </summary>
    public abstract class AreaState : State
    {
        [Inject] protected AreaController _area;  // The area entity this state belongs to
        [Inject] protected Timer _timer;          // Shared timer for scheduling delayed callbacks
    }
}
