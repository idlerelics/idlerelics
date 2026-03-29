using System;
using System.Collections.Generic;
using Game.Config;
using Game.Core;
using Game.Level.Entity;
using Game.Level.Inventory;
using Game.Level.Item;
using Game.Level.Place;
using Game.Level.Utility.UtilityStates;
using Injection;
using UnityEngine;

namespace Game.Level.Utility
{
    public sealed class UtilityModel : EntityModel
    {
        public UtilityModel(string id, int lvl, EntityType type, UtilityConfig config) : base(id, lvl, type)
        {
            TargetPurchaseValue = config.TargetPurchaseProgress;
            Area = config.Area;
        }
        public override int GetLvlLength()
        {
            return 0;
        }

        public override void GetUpdatedValues()
        {
        }
    }

    public sealed class UtilityController : EntityController, IDisposable
    {
        private readonly UtilityModel _model;
        private readonly UtilityView _view;
        private readonly StateManager<UtilityState> _stateManager;
        private readonly Dictionary<InventoryType, ItemUtilityView> _itemsMap;

        public EntityModel Model => _model;
        public UtilityView View => _view;
        public Dictionary<InventoryType, ItemUtilityView> ItemsMap => _itemsMap;

        public override Transform Transform => _view.transform;

        public UtilityController(UtilityView view, Context context)
        {
            _view = view;

            CameraAngleSign = _view.Sign;

            var subContext = new Context(context);
            var injector = new Injector(subContext);

            subContext.Install(this);
            subContext.InstallByType(this, typeof(UtilityController));
            subContext.Install(injector);

            _stateManager = new StateManager<UtilityState>();
            _stateManager.IsSendLogs = false;

            injector.Inject(_stateManager);

            var gameManager = context.Get<GameManager>();

            string id = gameManager.Model.GenerateEntityID(gameManager.Model.Hotel, _view.Type, 0);
            _view.name = id;

            _model = new UtilityModel(id, 0, _view.Type, _view.Config);

            _itemsMap = new Dictionary<InventoryType, ItemUtilityView>();
            foreach (var itemView in _view.Items)
            {
                _itemsMap.Add(itemView.Inventory, itemView);
            }

            SwitchToState(new UtilityInitializeState());
        }

        public void SwitchToState<T>(T instance) where T : UtilityState
        {
            _stateManager.SwitchToState(instance);
        }

        public void Dispose()
        {
            _stateManager.Dispose();
        }
    }
}

