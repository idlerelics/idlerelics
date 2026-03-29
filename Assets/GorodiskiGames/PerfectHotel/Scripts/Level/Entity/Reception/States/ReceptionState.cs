using Game.Core;
using Injection;

namespace Game.Level.Reception
{
    /// <summary>
    /// Abstract base class for all reception states.
    /// Provides DI-injected access to the ReceptionController.
    ///
    /// The reception uses a simple state machine with states like:
    /// - ReceptionIdleState: waiting for guests/player interaction
    /// - Other states for processing guests through the queue
    /// </summary>
    public abstract class ReceptionState : State
    {
        [Inject] protected ReceptionController _reception;
    }
}
