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

namespace Game
{
    /// <summary>
    /// Central hub for all runtime game state. Delegates item management to
    /// ItemRegistry and event broadcasting to GameEventBus.
    ///
    /// All existing public API is preserved as forwarding methods so no
    /// consumer code needs to change.
    /// </summary>
    public sealed class GameManager : IDisposable
    {
        // ---- Sub-systems ----
        public readonly ItemRegistry ItemRegistry;
        public readonly GameEventBus EventBus;

        // ---- Forwarding: events (subscribe/unsubscribe goes to the real owner) ----
        public event Action<Vector3, int> ADD_GAME_PROGRESS
        {
            add => EventBus.ADD_GAME_PROGRESS += value;
            remove => EventBus.ADD_GAME_PROGRESS -= value;
        }
        public event Action<int> PROGRESS_CHANGED
        {
            add => EventBus.PROGRESS_CHANGED += value;
            remove => EventBus.PROGRESS_CHANGED -= value;
        }
        public event Action<int> LEVEL_CHANGED
        {
            add => EventBus.LEVEL_CHANGED += value;
            remove => EventBus.LEVEL_CHANGED -= value;
        }
        public event Action<AreaController> AREA_PURCHASED
        {
            add => EventBus.AREA_PURCHASED += value;
            remove => EventBus.AREA_PURCHASED -= value;
        }
        public event Action<Vector3> FLY_TO_REMOVE_CASH
        {
            add => EventBus.FLY_TO_REMOVE_CASH += value;
            remove => EventBus.FLY_TO_REMOVE_CASH -= value;
        }
        public event Action<ItemController> ITEM_ADDED
        {
            add => ItemRegistry.ITEM_ADDED += value;
            remove => ItemRegistry.ITEM_ADDED -= value;
        }
        public event Action<Vector3, int> ON_NOTIFICATION_NEED_LVL
        {
            add => EventBus.ON_NOTIFICATION_NEED_LVL += value;
            remove => EventBus.ON_NOTIFICATION_NEED_LVL -= value;
        }
        public event Action ELEVATOR_PURCHASED
        {
            add => EventBus.ELEVATOR_PURCHASED += value;
            remove => EventBus.ELEVATOR_PURCHASED -= value;
        }
        public event Action<ItemController> ON_PLAYER_TRY_DROP_INVENTORY
        {
            add => EventBus.ON_PLAYER_TRY_DROP_INVENTORY += value;
            remove => EventBus.ON_PLAYER_TRY_DROP_INVENTORY -= value;
        }
        public event Action<bool> ON_PLAYERS_HUD_OPEN
        {
            add => EventBus.ON_PLAYERS_HUD_OPEN += value;
            remove => EventBus.ON_PLAYERS_HUD_OPEN -= value;
        }
        public event Action ON_TRY_SHOW_INTERSTITIAL
        {
            add => EventBus.ON_TRY_SHOW_INTERSTITIAL += value;
            remove => EventBus.ON_TRY_SHOW_INTERSTITIAL -= value;
        }

        // ---- Game State ----
        public List<ItemController> Items => ItemRegistry.Items;

        public readonly GameModel Model;
        public PlayerController Player;
        public ReceptionController Reception;
        public List<RoomController> Rooms;
        public List<ToiletController> Toilets;
        public readonly Dictionary<int, AreaController> AreasMap;
        public ElevatorController Elevator;
        public List<EntityController> Entities;
        public UtilityController Utility;
        public readonly Dictionary<UnitController, RoomController> CustomerRoomMap;

        public GameManager(GameConfig config)
        {
            Model = GameModel.Load(config);
            ItemRegistry = new ItemRegistry();
            EventBus = new GameEventBus();
            AreasMap = new Dictionary<int, AreaController>();
            Rooms = new List<RoomController>();
            CustomerRoomMap = new Dictionary<UnitController, RoomController>();
            Toilets = new List<ToiletController>();
            Entities = new List<EntityController>();
        }

        public void Dispose()
        {
            ItemRegistry.Dispose();
            EventBus.Dispose();
        }

        // ---- Forwarding: item methods ----
        public void AddItem(ItemController item) => ItemRegistry.AddItem(item);
        public void RemoveItem(ItemController item) => ItemRegistry.RemoveItem(item);
        public ItemController FindUsedItem(int targetArea, ItemType targetItem) => ItemRegistry.FindUsedItem(targetArea, targetItem);
        public ItemController FindClosestUsedItem() => ItemRegistry.FindClosestUsedItem(Player.View.transform.position);

        // ---- Find methods (stay on GameManager — they use collections that live here) ----

        public RoomController FindAvailableRoom()
        {
            foreach (var room in Rooms)
            {
                if (room != null && room.IsAvailable == true)
                    return room;
            }
            return null;
        }

        public EntityController FindClosestEntity(float radius)
        {
            foreach (var entity in Entities)
            {
                if (entity != null && (entity.Transform.position - Player.View.transform.position).sqrMagnitude < radius * radius)
                    return entity;
            }
            return null;
        }

        public ToiletController FindToilet(int area)
        {
            foreach (var toilet in Toilets)
            {
                if (toilet != null && toilet.Model.IsPurchased && toilet.Model.Area == area)
                    return toilet;
            }
            return null;
        }

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

        // ---- Forwarding: fire methods ----
        public void FireAddGameProgress(Vector3 position, int progressDelta) => EventBus.FireAddGameProgress(position, progressDelta);
        public void FireProgressChanged(int progress) => EventBus.FireProgressChanged(progress);
        public void FireAreaPurchased(AreaController area) => EventBus.FireAreaPurchased(area);
        public void FireLevelChanged(int lvl) => EventBus.FireLevelChanged(lvl);
        public void FireFlyToRemoveCash(Vector3 endPosition) => EventBus.FireFlyToRemoveCash(endPosition);
        public void FireNotificationNeedLvl(Vector3 itemPosition, int lvl) => EventBus.FireNotificationNeedLvl(itemPosition, lvl);
        internal void FireElevatorPurchased() => EventBus.FireElevatorPurchased();
        public void FirePlayerTryDropInventory(ItemController item) => EventBus.FirePlayerTryDropInventory(item);
        public void FirePlayersHudOpen(bool value) => EventBus.FirePlayersHudOpen(value);
        public void FireTryShowInterstitial() => EventBus.FireTryShowInterstitial();
    }
}
