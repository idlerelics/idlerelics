using System;
using Core;
using Game.Config;
using Game.Core;
using Injection;

namespace Game.Level.Unit
{
    public sealed class UnitModel : Observable
    {
        public float WalkSpeed;
        public float RotateSpeed;

        public UnitModel(float walkSpeed, float rotateSpeed)
        {
            WalkSpeed = walkSpeed;
            RotateSpeed = rotateSpeed;
        }
    }

    public sealed class UnitController
    {
        public Action<UnitController> ON_REMOVE;
        public Action<UnitController> ON_REMOVE_FROM_LINE;

        private readonly StateManager<UnitState> _stateManager;

        private UnitView _view;
        private UnitModel _model;

        public UnitView View => _view;
        public UnitModel Model => _model;

        public int Area;

        public UnitController(UnitView view, int index, Context context)
        {
            _view = view;

            _view.Index = index;

            var subContext = new Context(context);
            var injector = new Injector(subContext);

            subContext.Install(this);
            subContext.Install(injector);

            _stateManager = new StateManager<UnitState>();
            _stateManager.IsSendLogs = false;

            injector.Inject(_stateManager);

            var config = context.Get<GameConfig>();
            _model = new UnitModel(config.CustomerWalkSpeed, config.CustomerRotationSpeed);

            _view.NavMeshAgent.speed = _model.WalkSpeed;
        }

        public void SwitchToState<T>(T instance) where T : UnitState
        {
            _stateManager.SwitchToState(instance);
        }

        public void Dispose()
        {
            _stateManager.Dispose();
        }

        public void FireUnitRemove()
        {
            ON_REMOVE?.Invoke(this);
        }

        public void FireUnitRemoveFromLine()
        {
            ON_REMOVE_FROM_LINE?.Invoke(this);
        }
    }
}

