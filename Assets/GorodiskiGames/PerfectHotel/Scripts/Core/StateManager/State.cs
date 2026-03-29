using System;

namespace Game.Core
{
    /// <summary>
    /// Base class for all game states (e.g., MainMenuState, PlayingState, PausedState).
    ///
    /// ABSTRACT CLASS EXPLAINED:
    /// "abstract" means you CANNOT create a State directly (new State() won't work).
    /// You must create a subclass that extends it, like: "class PlayingState : State".
    /// Abstract methods (Initialize and Dispose) have NO body here -- each subclass
    /// MUST provide its own implementation. This ensures every state knows how to
    /// set itself up and clean itself up.
    ///
    /// IDisposable is a standard C# interface for objects that need cleanup.
    /// </summary>
    public abstract class State : IDisposable
    {
        /// <summary>
        /// Called when this state becomes the active state. Set up your state here
        /// (subscribe to events, create objects, start timers, etc.).
        /// Each subclass MUST implement this method.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Called when leaving this state (switching to another state). Clean up here
        /// (unsubscribe from events, release resources, stop timers, etc.).
        /// Each subclass MUST implement this method.
        /// </summary>
        public abstract void Dispose();
    }
}
