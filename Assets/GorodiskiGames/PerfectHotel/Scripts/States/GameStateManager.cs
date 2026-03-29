using Game.Core;

namespace Game.States
{
    /// <summary>
    /// Manages game states (e.g., menu, gameplay, loading) using a state machine pattern.
    /// 'sealed' means no other class can inherit from GameStateManager.
    ///
    /// This class inherits from StateManager&lt;GameState&gt;, which is a "generic" base class.
    /// Generics (the &lt;GameState&gt; part) let you write reusable code that works with different types.
    /// Here, StateManager is reused specifically for GameState objects.
    ///
    /// The base class (StateManager) contains all the logic for switching between states,
    /// so this class is empty -- it only specifies WHICH type of state to manage.
    /// </summary>
    public sealed class GameStateManager : StateManager<GameState>
    {
    }
}