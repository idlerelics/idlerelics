using Game.Core;
using Injection;

namespace Game.Level.Room
{
    /// <summary>
    /// Abstract base class for all room states.
    /// Provides DI-injected access to the RoomController and Timer.
    ///
    /// Room state lifecycle:
    /// RoomInitializeState -> (RoomHiddenState or RoomReadyToPurchaseState or RoomAvailableState or RoomUsedState)
    /// RoomAvailableState -> RoomOccupiedState (guest checks in) -> RoomUsedState (guest leaves, room dirty)
    /// RoomUsedState -> RoomAvailableState (room cleaned)
    /// </summary>
    public abstract class RoomState : State
    {
        [Inject] protected RoomController _room;  // The room this state belongs to
        [Inject] protected Timer _timer;           // Per-frame and per-second tick events
    }
}
