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
    /// <summary>
    /// Module that manages the reception desk: spawning receptionists, processing guest
    /// check-ins, managing the customer queue line, and assigning guests to available rooms.
    ///
    /// Implements IObserver to react when the reception's data model changes (e.g., after
    /// an upgrade that adds more receptionist slots).
    ///
    /// Flow:
    /// 1. Customers spawn and walk to the reception line
    /// 2. When a room becomes available, a receptionist processes the first customer in line
    /// 3. After processing, the customer walks to their assigned room
    /// 4. The line shifts forward and a new customer spawns at the back
    ///
    /// "sealed" means no other class can inherit from this one.
    /// </summary>
    public sealed class ReceptionModule : Module<ReceptionModuleView>, IObserver
    {
        // Dependencies injected by the custom DI system
        [Inject] private Timer _timer;               // Global frame timer for per-frame updates
        [Inject] private Context _context;           // DI container for creating child objects
        [Inject] private GameManager _gameManager;   // Central game state and entity management
        [Inject] private LevelView _levelView;       // Scene references (spawn points, etc.)

        // Maps each reception desk item to the room it's currently processing a guest for (null = idle)
        private readonly Dictionary<ItemController, RoomController> _itemsMap;
        // Maps each reception desk item to the receptionist unit standing behind it
        private readonly Dictionary<ItemController, UnitController> _receptionistMap;
        // All customer units currently in the scene (in line or walking)
        private List<UnitController> _customers;

        private ReceptionController _reception;   // The reception entity controller
        private int _receptionLvl;                // Cached level to detect upgrades
        private readonly List<ItemController> _tempItems = new List<ItemController>();

        /// <summary>
        /// Constructor: initializes the dictionaries that track desk-to-room
        /// and desk-to-receptionist mappings.
        /// </summary>
        public ReceptionModule(ReceptionModuleView view) : base(view)
        {
            _itemsMap = new Dictionary<ItemController, RoomController>();
            _receptionistMap = new Dictionary<ItemController, UnitController>();
            _customers = new List<UnitController>();
        }

        /// <summary>
        /// Sets up the reception: creates the controller, spawns receptionists
        /// based on the current level, fills the customer line, and subscribes
        /// to model changes and the frame timer.
        /// </summary>
        public override void Initialize()
        {
            // Create the reception controller from the scene view and register it globally
            _reception = new ReceptionController(_view.ReceptionView, _context);
            _gameManager.Reception = _reception;
            _receptionLvl = _reception.Model.Lvl;

            // Spawn the correct number of receptionists for the current level
            UpdateReceptionistsCount();
            // Fill empty spots in the customer queue line
            AddCustomerToLine();

            // Subscribe to reception model changes (Observer pattern) and frame ticks
            _reception.Model.AddObserver(this);
            _timer.TICK += OnTick;
        }

        /// <summary>
        /// Cleans up all receptionists, customers, pools, and subscriptions.
        /// Called when the level is unloaded or the module is destroyed.
        /// </summary>
        public override void Dispose()
        {
            _reception.Model.RemoveObserver(this);
            _timer.TICK -= OnTick;

            // Dispose and release all receptionist units back to their object pool
            foreach (var receptionist in _receptionistMap.Values.ToList())
            {
                receptionist.Dispose();
            }
            _view.Receptionist.ReleaseAllInstances();

            // Dispose and release all customer units
            foreach (var customer in _customers)
            {
                customer.Dispose();
            }
            _customers.Clear();

            // Release all instances from each customer prefab pool
            foreach (var customersPool in _view.Customers)
            {
                customersPool.ReleaseAllInstances();
            }

            _reception.Dispose();
        }

        /// <summary>
        /// Ensures the correct number of receptionists are spawned based on the
        /// reception's current level. If the level allows more receptionists than
        /// there are desk slots, it caps at the number of available desks.
        /// Also registers desk items in _itemsMap so they can be assigned rooms.
        /// </summary>
        private void UpdateReceptionistsCount()
        {
            int receptionistCount = _reception.Model.ReceptionistCount;

            // Cap receptionist count to available desk items
            if (receptionistCount > 0 && receptionistCount <= _reception.Items.Count)
                _reception.Model.ItemsToShow = receptionistCount;

            else if (receptionistCount > _reception.Items.Count)
            {
                receptionistCount = _reception.Items.Count;

                _reception.Model.ReceptionistCount = receptionistCount;
                _reception.Model.ItemsToShow = _reception.Items.Count;
            }

            // Register each active desk item in the items map (null = no room assigned yet)
            for (int i = 0; i < _reception.Model.ItemsToShow; i++)
            {
                var item = _reception.Items[i];
                if (!_itemsMap.ContainsKey(item))
                    _itemsMap.Add(item, null);
            }

            // Spawn a receptionist unit at each desk that doesn't have one yet
            for (int i = 0; i < _reception.Model.ReceptionistCount; i++)
            {
                var item = _reception.Items[i];
                if (!_receptionistMap.ContainsKey(item))
                {
                    // Remove this item from the player's interaction list
                    // (receptionists handle it automatically, no player interaction needed)
                    _gameManager.RemoveItem(item);
                    CreateReceptionist(item, _reception.Items[i].Transform.position);
                }
            }
        }

        /// <summary>
        /// Called every frame. Handles two responsibilities:
        /// 1. For non-receptionist desks: checks if a room is available and assigns it,
        ///    starting the processing timer and adding the item to the player's interaction list.
        /// 2. For receptionist desks: counts down the processing timer automatically
        ///    and fires the "finished" event when done (no player interaction required).
        /// </summary>
        private void OnTick()
        {
            // Check non-receptionist desk items for available rooms
            // Uses cached list because callbacks can modify _itemsMap
            _tempItems.Clear();
            _tempItems.AddRange(_itemsMap.Keys);
            foreach (var item in _tempItems)
            {
                var existingRoom = _itemsMap[item];
                var availableRoom = _gameManager.FindAvailableRoom();

                // If this desk has no room assigned but one has a free slot, claim it
                if (existingRoom == null && availableRoom != null && availableRoom.TryReserveSlot())
                {
                    _itemsMap[item] = availableRoom;

                    // Set the processing duration on the item's model (countdown timer)
                    item.Model.Duration = _reception.Model.UnitProceedTime;
                    item.Model.DurationNominal = _reception.Model.UnitProceedTime;
                    item.Model.SetChanged();  // Notify observers (updates UI progress bar)

                    // Listen for when processing is complete
                    item.ITEM_FINISHED += OnItemFinished;

                    // Only add to player interaction list if no receptionist handles this desk
                    if(!_receptionistMap.ContainsKey(item))
                        _gameManager.AddItem(item);
                }
            }

            // For receptionist-staffed desks, count down automatically each frame.
            // IMPORTANT: only one desk processes at a time, and only while a customer
            // is physically at the front of the line. With multi-worker chambers, all
            // 3 desks can reserve slots simultaneously — without this gate they all
            // tick to 0 in lockstep and fire FireItemFinished in the same frame, which
            // pulls customers out of line before they've even arrived (visually they
            // "skip" reception). Serial processing keeps the visuals coherent.
            if (!_reception.Line.IsFirstCustomerReady()) return;

            _tempItems.Clear();
            _tempItems.AddRange(_receptionistMap.Keys);
            foreach (var item in _tempItems)
            {
                var existingRoom = _itemsMap[item];
                if (existingRoom == null) continue;

                // Decrement timer by frame time (Time.deltaTime = seconds since last frame)
                item.Model.Duration -= Time.deltaTime;
                item.Model.SetChanged();  // Update UI each frame

                // When timer reaches zero, the guest is processed — fire and stop so
                // only one customer is pulled per frame (line needs to shift first).
                if (item.Model.Duration <= 0f)
                {
                    item.FireItemFinished();
                    break;
                }

                // Only the lead desk counts down per frame so processing stays serial.
                break;
            }
        }

        /// <summary>
        /// Called when a desk item finishes processing a guest.
        /// Clears the desk's room assignment and sends the guest to their room.
        /// </summary>
        void OnItemFinished(ItemController item)
        {
            item.ITEM_FINISHED -= OnItemFinished;

            // Get the assigned room, then clear the desk for the next guest
            var room = _itemsMap[item];
            _itemsMap[item] = null;

            OnCustomerWalkIn(room);
        }

        /// <summary>
        /// Fills any empty spots in the reception queue line by spawning new customer units.
        /// Each spot in the line is a Transform that defines where the customer stands.
        /// </summary>
        private void AddCustomerToLine()
        {
            foreach (var place in _reception.Line.PlaceUnitMap.Keys.ToList())
            {
                var customer = _reception.Line.PlaceUnitMap[place];
                if (customer == null)
                    CreateCustomer(place);
            }
        }

        /// <summary>
        /// Sends the first customer in the reception line to their assigned room.
        /// Charges the entrance fee, saves the updated cash, rearranges the remaining
        /// customers in line, and spawns a new customer at the back.
        /// </summary>
        private void OnCustomerWalkIn(RoomController room)
        {
            // Get the first customer from the reception line queue
            var unit = _reception.Line.GetFirstCustomer();
            if (unit == null)
            {
                // No customer to send — release the slot we reserved for this desk so it doesn't leak
                room.ReleaseReservation();
                return;
            }

            // Set the customer's area to match the room's area (for area-based systems)
            unit.Area = room.Model.Area;

            // Register the customer-to-room mapping so UnitWalkToRoomState knows where to go
            _gameManager.CustomerRoomMap.Add(unit, room);
            unit.SwitchToState(new UnitWalkToRoomState());

            // Shift remaining customers forward in the queue
            _reception.Line.RearrangeCustomersLine();

            // No entrance fee in the archaeology theme — workers don't pay to register at base camp.
            // All revenue comes from artifacts found during excavation (RoomOccupiedState trickle).

            // Spawn a new customer at the back of the line to replace the one that left
            AddCustomerToLine();
        }

        /// <summary>
        /// Spawns a new customer unit at the level's spawn point and sends it
        /// walking toward the given queue position. A random customer visual
        /// variant is chosen from the available prefab pools.
        /// </summary>
        private void CreateCustomer(Transform place)
        {
            Vector3 start = _levelView.UnitSpawnPoint.position;
            // Pick a random customer visual variant (different character models)
            int index = Random.Range(0, _view.Customers.Length);
            // Get a unit from the object pool (avoids expensive Instantiate/Destroy calls)
            var view = _view.Customers[index].Get<UnitView>();
            var unit = new UnitController(view, index, _context);
            unit.View.transform.position = start;
            // Walk from the spawn point to the assigned queue position
            unit.SwitchToState(new UnitWalkState(place.transform.position));
            // Listen for when this customer needs to be removed/despawned
            unit.ON_REMOVE += OnCustomerRemove;

            // Register the customer in the reception line at the given queue spot
            _reception.Line.PlaceUnitMap[place] = unit;
            _customers.Add(unit);
        }

        /// <summary>
        /// Handles customer removal: cleans up the unit controller, returns the
        /// visual to the object pool, and removes it from the active customers list.
        /// </summary>
        private void OnCustomerRemove(UnitController customer)
        {
            customer.ON_REMOVE -= OnCustomerRemove;

            customer.Dispose();
            // Return the unit view to its pool for reuse (object pooling pattern)
            _view.Customers[customer.View.Index].Release(customer.View);
            _customers.Remove(customer);
        }

        /// <summary>
        /// Creates a receptionist unit at the given desk position. The receptionist
        /// immediately enters UnitReceptionState (stands behind the desk, plays
        /// the greeting animation).
        /// </summary>
        private void CreateReceptionist(ItemController item, Vector3 startPosition)
        {
            var view = _view.Receptionist.Get<UnitView>();
            var unit = new UnitController(view, 0, _context);
            unit.View.transform.position = startPosition;
            unit.SwitchToState(new UnitReceptionState());
            _receptionistMap.Add(item, unit);
        }

        /// <summary>
        /// IObserver callback: called when the reception's Observable model changes.
        /// Checks if the reception level has increased (upgrade purchased) and,
        /// if so, updates the receptionist count to match the new level.
        /// </summary>
        public void OnObjectChanged(Observable observable)
        {
            // Only react if the reception level actually changed (ignore cash-only updates)
            if (_receptionLvl == _reception.Model.Lvl) return;
            _receptionLvl = _reception.Model.Lvl;

            UpdateReceptionistsCount();
        }
    }
}
