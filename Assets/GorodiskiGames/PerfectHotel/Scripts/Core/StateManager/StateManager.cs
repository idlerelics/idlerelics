using System;
using System.Collections.Generic;
using Injection;

namespace Game.Core
{
    /// <summary>
    /// Manages game states (e.g., MainMenu, Playing, Paused, GameOver) using the State Pattern.
    /// Only one state is active at a time. When you switch states, the old one is disposed
    /// (cleaned up) and the new one is initialized.
    ///
    /// GENERICS EXPLAINED:
    /// The &lt;T&gt; makes this class work with ANY type of State. "where T : State" is a
    /// "constraint" that says T must be the State class or something that extends it.
    /// This gives you type safety: the compiler ensures you only use valid State subclasses.
    ///
    /// IDisposable means this class supports cleanup via Dispose().
    /// </summary>
    public class StateManager<T> : IDisposable where T : State
    {
        // OneListener<T> is a safe event system (see OneListener.cs). It stores callbacks
        // that get called when the state changes.
        protected readonly OneListener<T> _onChangeState = new OneListener<T>();

        /// <summary>
        /// Event that fires whenever the active state changes. Other code can subscribe:
        ///   stateManager.CHANGE_STATE += OnStateChanged;
        /// The custom add/remove use OneListener for safe event handling.
        /// </summary>
        public event Action<T> CHANGE_STATE
        {
            add { _onChangeState.Add(value); }
            remove { _onChangeState.Remove(value); }
        }

        // Controls whether state changes are logged to the console
        public bool IsSendLogs { get; internal set; }

        // [Inject] tells the dependency injection system to automatically fill this field
        // with the Injector instance from the Context (see Inject.cs and Injector.cs)
        [Inject]
        protected Injector _injector;

        // Cache of created states so we reuse them instead of creating new ones each time.
        // The key is the state's Type, and the value is the state instance.
        private readonly Dictionary<Type, T> _statesMap;

        // The currently active state
        protected T _state;

        public StateManager()
        {
            _statesMap = new Dictionary<Type, T>(10);
            _state = null;
            IsSendLogs = true;
        }

        /// <summary>
        /// Cleans up the current state and clears the state cache.
        /// </summary>
        public void Dispose()
        {
            // "null != _state" is the same as "_state != null" -- just a different style
            if (null != _state)
            {
                _state.Dispose();
            }

            _state = null;
            _statesMap.Clear();
        }

        /// <summary>
        /// Gets or sets the current state. "virtual" means subclasses can override this.
        /// The setter disposes the old state, sets the new one, initializes it,
        /// and fires the CHANGE_STATE event.
        /// "protected set" means only this class and its children can change the state.
        /// </summary>
        public virtual T Current
        {
            get { return _state; }
            protected set
            {
                // Clean up the old state before switching
                if (null != _state)
                {
                    _state.Dispose();
                }

                _state = value;

                if(IsSendLogs)
                    Log.Info("Change state " + _state);

                // Set up the new state
                _state.Initialize();

                // Notify all listeners that the state changed
                _onChangeState.Invoke(_state);
            }
        }

        /// <summary>
        /// Switches to a specific state instance. Injects its dependencies first,
        /// then sets it as the current state.
        /// </summary>
        public void SwitchToState(T state)
        {
            _injector.Inject(state);
            this.Current = state;
        }

        /// <summary>
        /// Switches to a state by its type using a generic parameter.
        /// Example: stateManager.SwitchToState&lt;PlayingState&gt;();
        /// T1 is a separate generic parameter from the class's T.
        /// </summary>
        public void SwitchToState<T1>()
        {
            SwitchToState(typeof(T1));
        }

        /// <summary>
        /// Switches to a state by its Type. Creates the state if it doesn't exist yet
        /// (using Activator.CreateInstance, which calls the type's default constructor).
        /// States are cached in _statesMap so they're reused on subsequent switches.
        /// </summary>
        public void SwitchToState(Type type)
        {
            if (!_statesMap.ContainsKey(type))
            {
                // Activator.CreateInstance dynamically creates an object of the given type at runtime.
                // (T) casts it to our state type.
                _statesMap[type] = (T)Activator.CreateInstance(type);
            }

            var state = _statesMap[type];
            _injector.Inject(state);
            this.Current = state;
        }
    }
}
