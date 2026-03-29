using System.Collections.Generic;
using System.Linq;
using Core;
using Game.Core;
using Game.Level.Item;
using Game.Level.Room;
using Game.Level.Unit;
using Injection;
using UnityEngine;

namespace Game.Level.Reception
{
    public sealed class ReceptionModule : Module<ReceptionModuleView>, IObserver
    {
        [Inject] private Timer _timer;
        [Inject] private Context _context;
        [Inject] private GameManager _gameManager;
        [Inject] private LevelView _levelView;

        private readonly Dictionary<ItemController, RoomController> _itemsMap;
        private readonly Dictionary<ItemController, UnitController> _receptionistMap;
        private List<UnitController> _customers;

        private ReceptionController _reception;
        private int _receptionLvl;

        public ReceptionModule(ReceptionModuleView view) : base(view)
        {
            _itemsMap = new Dictionary<ItemController, RoomController>();
            _receptionistMap = new Dictionary<ItemController, UnitController>();
            _customers = new List<UnitController>();
        }

        public override void Initialize()
        {
            _reception = new ReceptionController(_view.ReceptionView, _context);
            _gameManager.Reception = _reception;
            _receptionLvl = _reception.Model.Lvl;

            UpdateReceptionistsCount();
            AddCustomerToLine();

            _reception.Model.AddObserver(this);
            _timer.TICK += OnTick;
        }

        public override void Dispose()
        {
            _reception.Model.RemoveObserver(this);
            _timer.TICK -= OnTick;

            foreach (var receptionist in _receptionistMap.Values.ToList())
            {
                receptionist.Dispose();
            }
            _view.Receptionist.ReleaseAllInstances();

            foreach (var customer in _customers)
            {
                customer.Dispose();
            }
            _customers.Clear();

            foreach (var customersPool in _view.Customers)
            {
                customersPool.ReleaseAllInstances();
            }

            _reception.Dispose();
        }

        private void UpdateReceptionistsCount()
        {
            int receptionistCount = _reception.Model.ReceptionistCount;

            if (receptionistCount > 0 && receptionistCount <= _reception.Items.Count)
                _reception.Model.ItemsToShow = receptionistCount;

            else if (receptionistCount > _reception.Items.Count)
            {
                receptionistCount = _reception.Items.Count;

                _reception.Model.ReceptionistCount = receptionistCount;
                _reception.Model.ItemsToShow = _reception.Items.Count;
            }

            for (int i = 0; i < _reception.Model.ItemsToShow; i++)
            {
                var item = _reception.Items[i];
                if (!_itemsMap.ContainsKey(item))
                    _itemsMap.Add(item, null);
            }

            for (int i = 0; i < _reception.Model.ReceptionistCount; i++)
            {
                var item = _reception.Items[i];
                if (!_receptionistMap.ContainsKey(item))
                {
                    _gameManager.RemoveItem(item);
                    CreateReceptionist(item, _reception.Items[i].Transform.position);
                }
            }
        }

        private void OnTick()
        {
            foreach (var item in _itemsMap.Keys.ToList())
            {
                var existingRoom = _itemsMap[item];
                var availableRoom = _gameManager.FindAvailableRoom();

                if (existingRoom == null && availableRoom != null)
                {
                    availableRoom.IsAvailable = false;

                    _itemsMap[item] = availableRoom;

                    item.Model.Duration = _reception.Model.UnitProceedTime;
                    item.Model.DurationNominal = _reception.Model.UnitProceedTime;
                    item.Model.SetChanged();

                    item.ITEM_FINISHED += OnItemFinished;

                    if(!_receptionistMap.ContainsKey(item))
                        _gameManager.AddItem(item);
                }
            }

            foreach (var item in _receptionistMap.Keys.ToList())
            {
                var existingRoom = _itemsMap[item];
                if (existingRoom != null)
                {
                    item.Model.Duration -= Time.deltaTime;
                    item.Model.SetChanged();

                    if (item.Model.Duration <= 0f)
                        item.FireItemFinished();
                }
            }
        }

        void OnItemFinished(ItemController item)
        {
            item.ITEM_FINISHED -= OnItemFinished;

            var room = _itemsMap[item];
            _itemsMap[item] = null;

            OnCustomerWalkIn(room);
        }

        private void AddCustomerToLine()
        {
            foreach (var place in _reception.Line.PlaceUnitMap.Keys.ToList())
            {
                var customer = _reception.Line.PlaceUnitMap[place];
                if (customer == null)
                    CreateCustomer(place);
            }
        }

        private void OnCustomerWalkIn(RoomController room)
        {
            var unit = _reception.Line.GetFirstCustomer();
            if (unit == null) return;

            unit.Area = room.Model.Area;

            _gameManager.CustomerRoomMap.Add(unit, room);
            unit.SwitchToState(new UnitWalkToRoomState());

            _reception.Line.RearrangeCustomersLine();

            int fee = room.Model.EntranceFee;
            _reception.Model.Cash += fee;
            _reception.Model.SetChanged();

            _gameManager.Model.SavePlaceCash(_reception.Model.ID, _reception.Model.Cash);

            AddCustomerToLine();
        }

        private void CreateCustomer(Transform place)
        {
            Vector3 start = _levelView.UnitSpawnPoint.position;
            int index = Random.Range(0, _view.Customers.Length);
            var view = _view.Customers[index].Get<UnitView>();
            var unit = new UnitController(view, index, _context);
            unit.View.transform.position = start;
            unit.SwitchToState(new UnitWalkState(place.transform.position));
            unit.ON_REMOVE += OnCustomerRemove;

            _reception.Line.PlaceUnitMap[place] = unit;
            _customers.Add(unit);
        }

        private void OnCustomerRemove(UnitController customer)
        {
            customer.ON_REMOVE -= OnCustomerRemove;

            customer.Dispose();
            _view.Customers[customer.View.Index].Release(customer.View);
            _customers.Remove(customer);
        }

        private void CreateReceptionist(ItemController item, Vector3 startPosition)
        {
            var view = _view.Receptionist.Get<UnitView>();
            var unit = new UnitController(view, 0, _context);
            unit.View.transform.position = startPosition;
            unit.SwitchToState(new UnitReceptionState());
            _receptionistMap.Add(item, unit);
        }

        public void OnObjectChanged(Observable observable)
        {
            if (_receptionLvl == _reception.Model.Lvl) return;
            _receptionLvl = _reception.Model.Lvl;

            UpdateReceptionistsCount();
        }
    }
}

