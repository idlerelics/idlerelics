using System;
using Game.Core;
using Game.Level.Inventory.InventoryStates;
using Injection;
using UnityEngine;
using Utilities;

namespace Game.Level.Inventory
{
    /// <summary>
    /// Controls a single inventory item that the player is carrying (e.g., toilet paper, soda can).
    /// Each inventory item has its own state machine (idle or flying to a destination).
    ///
    /// IDisposable is a C# interface that provides a Dispose() method for cleanup.
    /// It's commonly used to unsubscribe from events and free resources when an object is no longer needed.
    ///
    /// 'event Action' is C#'s built-in event system:
    /// - Other classes subscribe with += and unsubscribe with -=
    /// - The event is "fired" (invoked) to notify all subscribers
    /// </summary>
    public sealed class InventoryController : IDisposable
    {
        // Events notify other systems when something happens to this inventory item
        public event Action<InventoryController> ON_FLY_END;    // Fired when the item finishes flying to destination
        public event Action<InventoryController> ON_REMOVE;     // Fired when this item should be removed

        // 'readonly' means these can only be set in the constructor -- they never change after that
        private readonly InventoryType _type;                   // What kind of item (ToiletPaper, SodaCan, etc.)
        private readonly InventoryView _view;                   // The visual representation in the scene
        private readonly StateManager<InventoryState> _stateManager; // Manages idle/fly states

        // Public read-only properties (using "expression body" syntax: =>)
        public InventoryView View => _view;
        public InventoryType Type => _type;

        /// <summary>
        /// Creates a new inventory controller with its own sub-context for DI.
        /// A sub-context inherits all bindings from the parent but can add its own
        /// (like 'this' InventoryController) without polluting the parent context.
        /// </summary>
        public InventoryController(InventoryView view, InventoryType type, Context context)
        {
            _view = view;
            _type = type;

            // Create a child DI context so this controller's dependencies are scoped
            var subContext = new Context(context);
            var injector = new Injector(subContext);

            subContext.Install(this);       // Register this controller in the sub-context
            subContext.Install(injector);    // Register the injector itself

            // Create and inject the state machine so states can access DI dependencies
            _stateManager = new StateManager<InventoryState>();
            injector.Inject(_stateManager);
        }

        /// <summary>Cleans up the state machine when this inventory item is destroyed.</summary>
        public void Dispose()
        {
            _stateManager.Dispose();
        }

        /// <summary>
        /// Fires the ON_REMOVE event. 'internal' means accessible within this assembly only.
        /// SafeInvoke is an extension method that checks for null before invoking (prevents crashes
        /// if nobody is listening to the event).
        /// </summary>
        internal void FireRemove()
        {
            ON_REMOVE.SafeInvoke(this);
        }

        /// <summary>Notifies listeners that the fly animation has completed.</summary>
        internal void FireFlyEnd()
        {
            ON_FLY_END.SafeInvoke(this);
        }

        /// <summary>Switches the inventory item to the idle state (just sitting on the player).</summary>
        internal void Idle()
        {
            _stateManager.SwitchToState(new InventoryIdleState());
        }

        /// <summary>
        /// Makes the inventory item fly to the given position over the specified time.
        /// Used when delivering items (e.g., dropping toilet paper at a toilet).
        /// </summary>
        internal void Fly(Vector3 endPosition, float flyTime)
        {
            _stateManager.SwitchToState(new InventoryFlyState(endPosition, flyTime));
        }
    }
}
