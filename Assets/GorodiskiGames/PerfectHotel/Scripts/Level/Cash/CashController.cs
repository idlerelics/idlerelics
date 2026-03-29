using System;
using Game.Level.Cash.States;
using Game.Core;
using Injection;
using UnityEngine;
using Utilities;

namespace Game.Level.Cash
{
    public sealed class CashController : IDisposable
    {
        public Action<CashController> REMOVE_CASH;

        public readonly CashView View;
        private readonly StateManager<CashState> _stateManager;

        public Transform Transform => View.transform;
        public Transform Rotation => View.Rotation;


        public CashController(CashView view, Vector3 startPosition, Context context)
        {
            View = view;
            Transform.position = startPosition;

            var subContext = new Context(context);
            var injector = new Injector(subContext);

            subContext.Install(this);
            subContext.Install(injector);

            _stateManager = new StateManager<CashState>();
            _stateManager.IsSendLogs = false;

            injector.Inject(_stateManager);
        }

        public void Dispose()
        {
            _stateManager.Dispose();
        }

        public void Idle()
        {
            _stateManager.SwitchToState(new CashIdleState());
        }

        public void FlyToPlayer()
        {
            _stateManager.SwitchToState(typeof(CashFlyToPlayerState));
        }

        public void FireRemoveCash()
        {
            REMOVE_CASH.SafeInvoke(this);
        }

        internal void FlyToPile(Vector3 endPosition)
        {
            _stateManager.SwitchToState(new CashFlyToPileState(endPosition));
        }

        internal void FlyToRemove(Vector3 endPosition)
        {
            _stateManager.SwitchToState(new CashFlyToRemoveState(endPosition));
        }
    }
}