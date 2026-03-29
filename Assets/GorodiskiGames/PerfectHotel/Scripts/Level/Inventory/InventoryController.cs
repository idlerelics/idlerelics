using System;
using Game.Core;
using Game.Level.Inventory.InventoryStates;
using Injection;
using UnityEngine;
using Utilities;

namespace Game.Level.Inventory
{
    public sealed class InventoryController : IDisposable
    {
        public event Action<InventoryController> ON_FLY_END;
        public event Action<InventoryController> ON_REMOVE;

        private readonly InventoryType _type;
        private readonly InventoryView _view;
        private readonly StateManager<InventoryState> _stateManager;

        public InventoryView View => _view;
        public InventoryType Type => _type;

        public InventoryController(InventoryView view, InventoryType type, Context context)
        {
            _view = view;
            _type = type;

            var subContext = new Context(context);
            var injector = new Injector(subContext);

            subContext.Install(this);
            subContext.Install(injector);

            _stateManager = new StateManager<InventoryState>();
            injector.Inject(_stateManager);
        }

        public void Dispose()
        {
            _stateManager.Dispose();
        }

        internal void FireRemove()
        {
            ON_REMOVE.SafeInvoke(this);
        }

        internal void FireFlyEnd()
        {
            ON_FLY_END.SafeInvoke(this);
        }

        internal void Idle()
        {
            _stateManager.SwitchToState(new InventoryIdleState());
        }

        internal void Fly(Vector3 endPosition, float flyTime)
        {
            _stateManager.SwitchToState(new InventoryFlyState(endPosition, flyTime));
        }
    }
}

