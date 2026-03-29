using System;
using Game.Level.Cash.States;
using Game.Core;
using Injection;
using UnityEngine;
using Utilities;

namespace Game.Level.Cash
{
    /// <summary>
    /// Controls a single cash object (a coin/bill) in the game world.
    /// Each piece of cash can fly to a pile, fly to the player, or be removed.
    /// Implements IDisposable so it can clean up resources when destroyed.
    /// Uses a State Machine to manage its current behavior (idle, flying, etc.).
    /// </summary>
    public sealed class CashController : IDisposable
    {
        // Action delegate that fires when this cash should be removed from the scene.
        public Action<CashController> REMOVE_CASH;

        public readonly CashView View; // The visual representation (GameObject) of this cash
        private readonly StateManager<CashState> _stateManager; // Manages which state the cash is in

        // Shortcut properties to access the View's Transform (position/rotation/scale).
        public Transform Transform => View.transform;
        public Transform Rotation => View.Rotation;

        /// <summary>
        /// Constructor: creates a cash object at the given position.
        /// Sets up dependency injection and the state machine.
        /// </summary>
        public CashController(CashView view, Vector3 startPosition, Context context)
        {
            View = view;
            Transform.position = startPosition; // Place the cash at the start position

            // Create a child context for dependency injection (DI).
            // DI lets this object and its states automatically receive dependencies they need.
            var subContext = new Context(context);
            var injector = new Injector(subContext);

            subContext.Install(this);
            subContext.Install(injector);

            _stateManager = new StateManager<CashState>();
            _stateManager.IsSendLogs = false; // Disable debug logging for performance

            injector.Inject(_stateManager); // Give the state manager access to all dependencies
        }

        /// <summary>Cleans up the state machine when this cash is no longer needed.</summary>
        public void Dispose()
        {
            _stateManager.Dispose();
        }

        /// <summary>Switches the cash to idle state (just sitting in place).</summary>
        public void Idle()
        {
            _stateManager.SwitchToState(new CashIdleState());
        }

        /// <summary>Makes the cash fly toward the player (e.g., when collected).</summary>
        public void FlyToPlayer()
        {
            _stateManager.SwitchToState(typeof(CashFlyToPlayerState));
        }

        /// <summary>Fires the REMOVE_CASH event to tell the system to destroy this cash.</summary>
        public void FireRemoveCash()
        {
            REMOVE_CASH.SafeInvoke(this); // SafeInvoke = only call if someone is listening
        }

        /// <summary>Makes the cash fly to a pile at the given position (e.g., spawning from a room).</summary>
        internal void FlyToPile(Vector3 endPosition)
        {
            _stateManager.SwitchToState(new CashFlyToPileState(endPosition));
        }

        /// <summary>Makes the cash fly to a position and then get removed (e.g., spending cash).</summary>
        internal void FlyToRemove(Vector3 endPosition)
        {
            _stateManager.SwitchToState(new CashFlyToRemoveState(endPosition));
        }
    }
}