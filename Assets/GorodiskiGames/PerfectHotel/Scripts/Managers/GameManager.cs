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
    public sealed class GameManager : IDisposable
    {
        public event Action<Vector3, int> ADD_GAME_PROGRESS;
        public event Action<int> PROGRESS_CHANGED;
        public event Action<int> LEVEL_CHANGED;
        public event Action<AreaController> AREA_PURCHASED;
        public event Action<Vector3> FLY_TO_REMOVE_CASH;
        public event Action<ItemController> ITEM_ADDED;
        public event Action<Vector3, int> ON_NOTIFICATION_NEED_LVL;
        public event Action ELEVATOR_PURCHASED;
        public event Action<ItemToiletController> ON_PLAYER_TRY_DROP_INVENTORY;
        public event Action<bool> ON_PLAYERS_HUD_OPEN;
        public event Action ON_TRY_SHOW_INTERSTITIAL;

        private List<ItemController> _items;
        public List<ItemController> Items => _items;

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

        public void AddItem(ItemController item)
        {
            if (!_items.Contains(item))
            {
                _items.Add(item);

                ITEM_ADDED.SafeInvoke(item);
            }
        }

        public void RemoveItem(ItemController item)
        {
            if (_items.Contains(item))
            {
                _items.Remove(item);
            }
        }

        internal void FireElevatorPurchased()
        {
            ELEVATOR_PURCHASED?.Invoke();
        }

        public RoomController FindAvailableRoom()
        {
            foreach (var room in Rooms)
            {
                if (room != null && room.IsAvailable == true)
                    return room;
            }
            return null;
        }

        public ItemController FindUsedItem(int targetArea, ItemType targetItem)
        {
            foreach (var item in _items)
            {
                if (item != null && AreaMatch(targetArea, item.Area) && item.Type == targetItem)
                    return item;
            }
            return null;
        }

        private bool AreaMatch(int targetArea, int itemArea)
        {
            bool result = false;

            if (targetArea == -1)
                result = true;
            else if (itemArea == targetArea)
                result = true;

            return result;
        }

        public ItemController FindClosestUsedItem()
        {
            foreach (var item in _items)
            {
                if (item != null && Vector3.Distance(item.Transform.position, Player.View.transform.position) < item.Radius)
                    return item;
            }
            return null;
        }

        public EntityController FindClosestEntity(float radius)
        {
            foreach (var entity in Entities)
            {
                if (entity != null && Vector3.Distance(entity.Transform.position, Player.View.transform.position) < radius)
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