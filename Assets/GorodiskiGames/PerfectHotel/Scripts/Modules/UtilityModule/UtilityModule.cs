using System.Collections.Generic;
using Game.Level;
using Game.Level.Inventory;
using Game.Level.Loader;
using Game.Level.Utility;
using Injection;
using UnityEngine;
using Game.Core;
using System.Linq;
using Game.Config;
using Game.Level.Item;
using Game.Level.Player;

namespace Game.Modules.Utility
{
    public sealed class UtilityModule : Module<UtilityModuleView>
    {
        private const float _inventoryFlyTime = 0.3f;
        private const float _inventoryLerpSpeed = 0.95f;

        [Inject] private GameManager _gameManager;
        [Inject] private Context _context;
        [Inject] private Timer _timer;
        [Inject] private GameConfig _config;

        private readonly List<StaffController> _staff;
        private readonly List<InventoryController> _inventories;
        private readonly List<StaffController> _staffAvailable;
        private readonly Dictionary<StaffController, InventoryController> _staffAvailableWithInventory;
        private readonly Dictionary<StaffController, InventoryController> _staffWalkWithInventory;
        private readonly List<StaffController> _staffWalkingToUtility;
        private readonly Dictionary<InventoryController, StaffController> _inventoryFlyingMap;
        private readonly Dictionary<StaffController, ItemController> _staffItemMap;
        private readonly Dictionary<InventoryController, ItemController> _inventoryItemMap;

        public UtilityModule(UtilityModuleView view) : base(view)
        {
            _staff = new List<StaffController>();
            _inventories = new List<InventoryController>();
            _staffAvailable = new List<StaffController>();
            _staffWalkingToUtility = new List<StaffController>();
            _inventoryFlyingMap = new Dictionary<InventoryController, StaffController>();
            _staffAvailableWithInventory = new Dictionary<StaffController, InventoryController>();
            _staffWalkWithInventory = new Dictionary<StaffController, InventoryController>();
            _staffItemMap = new Dictionary<StaffController, ItemController>();
            _inventoryItemMap = new Dictionary<InventoryController, ItemController>();
        }

        public override void Initialize()
        {
            _gameManager.Utility = new UtilityController(_view.UtilityView, _context);
            _gameManager.Entities.Add(_gameManager.Utility);

            CreatePlayerInventories();
            SetLoaders();
            CheckAllItems();

            _gameManager.ITEM_ADDED += OnItemAdded;
            _gameManager.Player.ON_IDLE += OnPlayerIdle;
            _gameManager.ON_PLAYER_TRY_DROP_INVENTORY += OnPlayerTryDropInventory;

            _gameManager.ON_PLAYERS_HUD_OPEN += OnPlayersHudOpen;

            _timer.TICK += OnTick;
        }

        public override void Dispose()
        {
            _timer.TICK -= OnTick;

            _gameManager.ITEM_ADDED -= OnItemAdded;
            _gameManager.Player.ON_IDLE -= OnPlayerIdle;
            _gameManager.ON_PLAYER_TRY_DROP_INVENTORY -= OnPlayerTryDropInventory;
            _gameManager.ON_PLAYERS_HUD_OPEN -= OnPlayersHudOpen;

            foreach (var staff in _staff)
            {
                staff.Dispose();
            }
            _staff.Clear();

            foreach (var inventory in _inventories)
            {
                _view.InventoryMap[inventory.Type].Release(inventory.View);
                inventory.Dispose();
            }
            _inventories.Clear();

            _gameManager.Utility.Dispose();
        }

        private void SetLoaders()
        {
            foreach (var view in _view.LoaderViews)
            {
                var loader = new LoaderController(view, _context);
                _staff.Add(loader);

                loader.ON_PURCHASED += OnStaffPurchased;
                loader.InitializeStaff();
            }
        }

        private void OnStaffPurchased(StaffController staff)
        {
            staff.ON_PURCHASED -= OnStaffPurchased;

            if (_staffAvailable.Contains(staff))
                Log.Warning("Trying to add existing Staff");
            else
            {
                _staffAvailable.Add(staff);

                var targetItem = ItemType.DropInventory;
                var item = _gameManager.FindUsedItem(staff.Area, targetItem);

                if (item != null)
                {
                    OnItemAdded(item);
                }
            }
        }

        private void CheckAllItems()
        {
            foreach (var item in _gameManager.Items)
            {
                OnItemAdded(item);
            }
        }

        internal void OnItemAdded(ItemController item)
        {
            if (item.Type != ItemType.DropInventory) return;

            var cabine = item as ItemToiletController;

            var targetInventory = cabine.View.TargetInventory;
            var staffAvailableWithInventory = FindStaffAvailableWithInventory(targetInventory);

            if (staffAvailableWithInventory != null)
            {
                _gameManager.RemoveItem(item);

                var inventory = _staffAvailableWithInventory[staffAvailableWithInventory];
                _staffWalkWithInventory.Add(staffAvailableWithInventory, inventory);

                _staffAvailableWithInventory.Remove(staffAvailableWithInventory);

                staffAvailableWithInventory.WalkToItem(item.Transform.position);
                staffAvailableWithInventory.ON_ARRIVED_TO_ITEM += OnStaffArrivedToItem;

                _staffItemMap.Add(staffAvailableWithInventory, item);

                return;
            }

            var staff = FindStaffAvailable(targetInventory);

            if (staff == null) return;

            Vector3 position = _gameManager.Utility.ItemsMap[targetInventory].Position;

            _staffAvailable.Remove(staff);

            staff.WalkToUtility(position);

            _staffWalkingToUtility.Add(staff);

            staff.ON_ARRIVED_TO_UTILITY += OnStaffArrivedToUtility;
        }

        private StaffController FindStaffAvailableWithInventory(InventoryType targetInventory)
        {
            foreach (var staff in _staffAvailableWithInventory.Keys.ToList())
            {
                var inventory = _staffAvailableWithInventory[staff];
                if (inventory != null && inventory.Type == targetInventory)
                {
                    return staff;
                }
            }
            return null;
        }

        private StaffController FindStaffAvailable(InventoryType targetInventory)
        {
            foreach (var staff in _staffAvailable)
            {
                if (staff.TargetInventory == targetInventory)
                {
                    return staff;
                }
            }
            return null;
        }

        private InventoryController GetInventory(InventoryType type)
        {
            var view = _view.InventoryMap[type].Get<InventoryView>();
            var inventory = new InventoryController(view, type, _context);

            _inventories.Add(inventory);

            inventory.ON_REMOVE += RemoveInventory;

            Vector3 startPosition = _gameManager.Utility.ItemsMap[type].AimPosition;
            inventory.View.Position = startPosition;

            return inventory;
        }

        private void OnStaffArrivedToUtility(StaffController staff)
        {
            staff.ON_ARRIVED_TO_UTILITY -= OnStaffArrivedToUtility;

            var inventory = GetInventory(staff.TargetInventory);

            _inventoryFlyingMap.Add(inventory, staff);

            inventory.Fly(staff.InventoryHolder.position, _inventoryFlyTime);

            inventory.ON_FLY_END += OnStaffGetInventory;
        }

        private void OnStaffGetInventory(InventoryController inventory)
        {
            inventory.ON_FLY_END -= OnStaffGetInventory;

            var staff = _inventoryFlyingMap[inventory];
            _inventoryFlyingMap.Remove(inventory);

            inventory.Idle();

            staff.Inventories++;

            var targetItem = ItemType.DropInventory;
            var item = _gameManager.FindUsedItem(staff.Area, targetItem);
            if (item != null)
            {
                _gameManager.RemoveItem(item);

                staff.WalkToItem(item.Transform.position);
                staff.ON_ARRIVED_TO_ITEM += OnStaffArrivedToItem;

                _staffWalkWithInventory.Add(staff, inventory);
                _staffItemMap.Add(staff, item);
            }
            else
            {
                _staffAvailableWithInventory.Add(staff, inventory);
                staff.WalkHome();
                staff.ON_ARRIVED_HOME += OnStaffArrivedHome;
            }
        }

        private void OnStaffArrivedHome(StaffController staff)
        {
            staff.ON_ARRIVED_HOME -= OnStaffArrivedHome;

            staff.Idle();
        }

        private void OnStaffArrivedToItem(StaffController staff)
        {
            staff.ON_ARRIVED_TO_ITEM -= OnStaffArrivedToItem;

            var item = _staffItemMap[staff] as ItemToiletController;
            _staffItemMap.Remove(staff);

            var inventory = _staffWalkWithInventory[staff];
            _staffWalkWithInventory.Remove(staff);

            if (!item.IsAvailable)
            {
                inventory.Fly(item.View.AimPosition, _inventoryFlyTime);
                inventory.ON_FLY_END += OnStaffDeliveredInventory;

                _inventoryItemMap.Add(inventory, item);

                staff.Inventories--;

                _staffAvailable.Add(staff);
            }
            else
            {
                _staffAvailableWithInventory.Add(staff, inventory);
            }

            var targetItem = ItemType.DropInventory;
            var newItem = _gameManager.FindUsedItem(staff.Area, targetItem);

            if (newItem != null)
            {
                OnItemAdded(newItem);
            }
            else
            {
                staff.WalkHome();
                staff.ON_ARRIVED_HOME += OnStaffArrivedHome;
            }
        }

        private void OnStaffDeliveredInventory(InventoryController inventory)
        {
            inventory.ON_FLY_END -= OnStaffDeliveredInventory;

            var item = _inventoryItemMap[inventory];
            _inventoryItemMap.Remove(inventory);

            inventory.FireRemove();

            item.Model.Duration = 0;
            item.Model.SetChanged();

            item.FireItemFinished();
        }

        private void RemoveInventory(InventoryController inventory)
        {
            inventory.ON_REMOVE -= RemoveInventory;

            _inventories.Remove(inventory);

            _view.InventoryMap[inventory.Type].Release(inventory.View);
            inventory.Dispose();
        }

        private void OnTick()
        {
            foreach (var staff in _staffAvailableWithInventory.Keys.ToList())
            {
                var inventory = _staffAvailableWithInventory[staff];
                inventory.View.Position = Vector3.Lerp(inventory.View.Position, staff.InventoryHolder.transform.position, _inventoryLerpSpeed);
            }

            foreach (var staff in _staffWalkWithInventory.Keys.ToList())
            {
                var inventory = _staffWalkWithInventory[staff];
                inventory.View.Position = Vector3.Lerp(inventory.View.Position, staff.InventoryHolder.transform.position, _inventoryLerpSpeed);
            }

            int index = 0;
            foreach (var inventory in _gameManager.Player.Model.Inventories.ToList())
            {
                inventory.View.Position = Vector3.Lerp(inventory.View.Position, _gameManager.Player.GetInventoryPosition(index), _inventoryLerpSpeed);
                index++;
            }
        }

        private void OnPlayerIdle()
        {
            var item = FindNearestItem();
            if (item == null) return;

            if (item.Type == ItemType.GetInventory && _gameManager.Player.Model.HasInventorySpace())
            {
                var inventory = GetInventory(item.Inventory);
                int inventories = _gameManager.Player.Model.Inventories.Count;
                int inventoriesResult = inventories + 1;

                _gameManager.Player.View.UpdateCurrentAnimation(inventoriesResult);

                Vector3 endPosition = _gameManager.Player.GetInventoryPosition(inventories);

                inventory.Fly(endPosition, _inventoryFlyTime);
                inventory.ON_FLY_END += OnPlayerGetInventory;
            }

            else if (item.Type == ItemType.DropInventory && _gameManager.Player.Model.HasInventory())
            {
                int index = _gameManager.Player.Model.Inventories.Count - 1;
                var inventory = _gameManager.Player.Model.Inventories[index];

                _gameManager.Model.InventoryTypes.Remove(inventory.Type);
                _gameManager.Model.Save();

                _gameManager.Player.Model.Inventories.Remove(inventory);

                int inventories = _gameManager.Model.InventoryTypes.Count;
                _gameManager.Player.View.UpdateCurrentAnimation(inventories);

                inventory.Fly(item.AimPosition, _inventoryFlyTime);
                inventory.ON_FLY_END += OnDisposeInventory;
            }
        }

        private void OnDisposeInventory(InventoryController inventory)
        {
            inventory.ON_FLY_END -= OnDisposeInventory;

            inventory.FireRemove();
        }

        private void OnPlayerGetInventory(InventoryController inventory)
        {
            inventory.ON_FLY_END -= OnPlayerGetInventory;

            inventory.Idle();

            _gameManager.Model.InventoryTypes.Add(inventory.Type);
            _gameManager.Model.Save();

            _gameManager.Player.Model.Inventories.Add(inventory);
        }

        public ItemUtilityView FindNearestItem()
        {
            ItemUtilityView result = null;
            float minDistance = float.MaxValue;
            var playerPosition = new Vector2(_gameManager.Player.View.Position.x, _gameManager.Player.View.Position.z);
            float itemRadius = _config.UtilityItemRadius;

            foreach (var item in _gameManager.Utility.ItemsMap.Values)
            {
                var itemPosition = new Vector2(item.Position.x, item.Position.z);
                var distance = Vector2.Distance(itemPosition, playerPosition);

                if (distance <= itemRadius && distance < minDistance)
                {
                    minDistance = distance;
                    result = item;
                }
            }
            return result;
        }

        private void CreatePlayerInventories()
        {
            foreach (var type in _gameManager.Model.InventoryTypes)
            {
                if (!_gameManager.Player.Model.HasInventorySpace()) continue;

                var inventory = GetInventory(type);
                _gameManager.Player.Model.Inventories.Add(inventory);
            }
        }

        private void DestroyPlayerInventories()
        {
            foreach (var inventory in _gameManager.Player.Model.Inventories.ToList())
            {
                _gameManager.Player.Model.Inventories.Remove(inventory);
                inventory.FireRemove();
            }
        }

        private void OnPlayerTryDropInventory(ItemToiletController item)
        {
            foreach (var inventory in _gameManager.Player.Model.Inventories.ToList())
            {
                if (inventory.Type == item.View.TargetInventory)
                {
                    _gameManager.Model.InventoryTypes.Remove(inventory.Type);
                    _gameManager.Model.Save();

                    _gameManager.Player.Model.Inventories.Remove(inventory);

                    _inventoryItemMap.Add(inventory, item);

                    int inventories = _gameManager.Model.InventoryTypes.Count;
                    _gameManager.Player.View.UpdateCurrentAnimation(inventories);

                    inventory.Fly(item.View.AimPosition, _inventoryFlyTime);
                    inventory.ON_FLY_END += OnPlayerDeliveredInventory;

                    break;
                }
            }
        }

        private void OnPlayerDeliveredInventory(InventoryController inventory)
        {
            inventory.ON_FLY_END -= OnPlayerDeliveredInventory;

            var item = _inventoryItemMap[inventory];
            _inventoryItemMap.Remove(inventory);

            inventory.FireRemove();

            item.Model.Duration = 0;
            item.Model.SetChanged();

            item.FireItemFinished();
        }

        private void OnPlayersHudOpen(bool value)
        {
            if (value)
                DestroyPlayerInventories();
            else
                CreatePlayerInventories();
        }
    }
}

