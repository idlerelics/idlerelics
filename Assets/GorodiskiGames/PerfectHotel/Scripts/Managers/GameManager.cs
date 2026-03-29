using System;
using System.Collections.Generic;
using Game.Config;
using Game.Domain;
using Game.Level.Utility;
using UnityEngine;
using Game.Level.Reception;
using Game.Level.Area;
using Game.Level.Elevator;
using Game.Level.Item;
using Game.Level.Room;
using Game.Level.Entity;
using Game.Level.Toilet;
using Game.Level.Player;
using Game.Level.Unit;
using Utilities;

namespace Game
{
    /// <summary>
    /// Central hub for all runtime game state. Holds references to the player,
    /// rooms, toilets, reception, elevator, and all interactive items.
    ///
    /// This is NOT a MonoBehaviour -- it's a plain C# class managed by the DI container.
    /// Other systems use events on this class to communicate (loose coupling).
    ///
    /// Events (Action delegates) are the primary communication mechanism:
    /// - Systems subscribe to events they care about
    /// - When something happens, the event is fired (SafeInvoke)
    /// - All subscribers are notified without the sender knowing about them
    /// </summary>
    public sealed class GameManager : IDisposable
    {
        // ---- Events ----
        // These events notify other systems when important things happen.
        // Action<T> is a delegate that takes parameters and returns void.
        public event Action<Vector3, int> ADD_GAME_PROGRESS;           // Progress earned at a world position
        public event Action<int> PROGRESS_CHANGED;                     // Total progress value changed
        public event Action<int> LEVEL_CHANGED;                        // Player changed to a new level
        public event Action<AreaController> AREA_PURCHASED;            // A new area was unlocked
        public event Action<Vector3> FLY_TO_REMOVE_CASH;               // Cash should fly to a position and be removed
        public event Action<ItemController> ITEM_ADDED;                // A new interactive item appeared
        public event Action<Vector3, int> ON_NOTIFICATION_NEED_LVL;    // "Need level X" notification at position
        public event Action ELEVATOR_PURCHASED;                        // The elevator was bought
        public event Action<ItemToiletController> ON_PLAYER_TRY_DROP_INVENTORY; // Player tries to deliver inventory to toilet
        public event Action<bool> ON_PLAYERS_HUD_OPEN;                 // Players HUD opened/closed
        public event Action ON_TRY_SHOW_INTERSTITIAL;                  // Time to try showing an interstitial ad

        // ---- Game State ----
        private List<ItemController> _items;            // All active interactive items in the world
        public List<ItemController> Items => _items;

        public readonly GameModel Model;                // Persistent save data (readonly = set only in constructor)
        public PlayerController Player;                 // The player character
        public ReceptionController Reception;           // The front desk
        public List<RoomController> Rooms;              // All rooms in the current level
        public List<ToiletController> Toilets;          // All toilets
        public readonly Dictionary<int, AreaController> AreasMap;  // Areas indexed by number
        public ElevatorController Elevator;              // The elevator (for switching levels)
        public List<EntityController> Entities;          // All purchasable entities
        public UtilityController Utility;                // The utility room (supplies)
        public readonly Dictionary<UnitController, RoomController> CustomerRoomMap; // Maps customers to their assigned rooms

        /// <summary>
        /// Constructor -- loads the game model from save data and initializes all collections.
        /// </summary>
        public GameManager(GameConfig config)
        {
            Model = GameModel.Load(config);                  // Load saved progress or create new
            AreasMap = new Dictionary<int, AreaController>();
            Rooms = new List<RoomController>();
            CustomerRoomMap = new Dictionary<UnitController, RoomController>();
            _items = new List<ItemController>();
            Toilets = new List<ToiletController>();
            Entities = new List<EntityController>();
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Adds an interactive item to the world (if not already present).
        /// Fires ITEM_ADDED so systems like PlayerIdleState can check for nearby items.
        /// </summary>
        public void AddItem(ItemController item)
        {
            if (!_items.Contains(item))
            {
                _items.Add(item);

                ITEM_ADDED.SafeInvoke(item); // Notify subscribers
            }
        }

        /// <summary>Removes an item from the active items list (e.g., when player starts using it).</summary>
        public void RemoveItem(ItemController item)
        {
            if (_items.Contains(item))
            {
                _items.Remove(item);
            }
        }

        internal void FireElevatorPurchased()
        {
            ELEVATOR_PURCHASED?.Invoke(); // ?. is the null-conditional operator (same as SafeInvoke)
        }

        /// <summary>
        /// Finds the first room that is available (not occupied by a customer).
        /// Returns null if all rooms are full.
        /// </summary>
        public RoomController FindAvailableRoom()
        {
            foreach (var room in Rooms)
            {
                if (room != null && room.IsAvailable == true)
                    return room;
            }
            return null;
        }

        /// <summary>
        /// Finds an item of a specific type in a specific area.
        /// Used by NPCs to find where they need to go (e.g., find the cleaning spot in area 2).
        /// </summary>
        public ItemController FindUsedItem(int targetArea, ItemType targetItem)
        {
            foreach (var item in _items)
            {
                if (item != null && AreaMatch(targetArea, item.Area) && item.Type == targetItem)
                    return item;
            }
            return null;
        }

        /// <summary>
        /// Checks if an item's area matches the target area.
        /// -1 means "any area" (wildcard match).
        /// </summary>
        private bool AreaMatch(int targetArea, int itemArea)
        {
            bool result = false;

            if (targetArea == -1)
                result = true;           // -1 = match any area
            else if (itemArea == targetArea)
                result = true;           // Exact match

            return result;
        }

        /// <summary>
        /// Finds the closest interactive item to the player that's within interaction range.
        /// Uses Vector3.Distance to calculate the straight-line distance in 3D space.
        /// </summary>
        public ItemController FindClosestUsedItem()
        {
            foreach (var item in _items)
            {
                if (item != null && Vector3.Distance(item.Transform.position, Player.View.transform.position) < item.Radius)
                    return item;
            }
            return null;
        }

        /// <summary>
        /// Finds the closest entity (purchasable building/room) within a given radius of the player.
        /// </summary>
        public EntityController FindClosestEntity(float radius)
        {
            foreach (var entity in Entities)
            {
                if (entity != null && Vector3.Distance(entity.Transform.position, Player.View.transform.position) < radius)
                    return entity;
            }
            return null;
        }

        /// <summary>Finds a purchased toilet in a specific area.</summary>
        public ToiletController FindToilet(int area)
        {
            foreach (var toilet in Toilets)
            {
                if (toilet != null && toilet.Model.IsPurchased && toilet.Model.Area == area)
                    return toilet;
            }
            return null;
        }

        /// <summary>Finds an area by its number. Logs an error if not found.</summary>
        public AreaController FindArea(int number)
        {
            foreach (var area in AreasMap.Values)
            {
                if (area.View.Config.Number == number)
                    return area;
            }
            Log.Error("Error. Area " + number + " not found");
            return null;
        }

        // ---- Event fire methods ----
        // These methods are called by other systems to broadcast events.

        public void FireAddGameProgress(Vector3 position, int progressDelta)
        {
            ADD_GAME_PROGRESS.SafeInvoke(position, progressDelta);
        }

        public void FireProgressChanged(int progress)
        {
            PROGRESS_CHANGED.SafeInvoke(progress);
        }

        public void FireAreaPurchased(AreaController area)
        {
            AREA_PURCHASED.SafeInvoke(area);
        }

        public void FireLevelChanged(int lvl)
        {
            LEVEL_CHANGED.SafeInvoke(lvl);
        }

        public void FireFlyToRemoveCash(Vector3 endPosition)
        {
            FLY_TO_REMOVE_CASH.SafeInvoke(endPosition);
        }

        public void FireNotificationNeedLvl(Vector3 itemPosition, int lvl)
        {
            ON_NOTIFICATION_NEED_LVL.SafeInvoke(itemPosition, lvl);
        }

        public void FirePlayerTryDropInventory(ItemToiletController item)
        {
            ON_PLAYER_TRY_DROP_INVENTORY?.Invoke(item);
        }

        public void FirePlayersHudOpen(bool value)
        {
            ON_PLAYERS_HUD_OPEN.SafeInvoke(value);
        }

        public void FireTryShowInterstitial()
        {
            ON_TRY_SHOW_INTERSTITIAL.SafeInvoke();
        }
    }
}
