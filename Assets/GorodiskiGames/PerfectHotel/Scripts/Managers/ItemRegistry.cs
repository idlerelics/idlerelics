using System;
using System.Collections.Generic;
using Game.Level.Item;
using UnityEngine;
using Utilities;

namespace Game
{
    public sealed class ItemRegistry : IDisposable
    {
        public event Action<ItemController> ITEM_ADDED;

        private readonly List<ItemController> _items;
        private readonly Dictionary<int, List<ItemController>> _itemsByArea;

        public List<ItemController> Items => _items;

        public ItemRegistry()
        {
            _items = new List<ItemController>();
            _itemsByArea = new Dictionary<int, List<ItemController>>();
        }

        public void AddItem(ItemController item)
        {
            if (item == null) return;
            if (!_items.Contains(item))
            {
                _items.Add(item);

                if (!_itemsByArea.TryGetValue(item.Area, out var areaList))
                {
                    areaList = new List<ItemController>();
                    _itemsByArea[item.Area] = areaList;
                }
                areaList.Add(item);

                ITEM_ADDED.SafeInvoke(item);
            }
        }

        public void RemoveItem(ItemController item)
        {
            if (_items.Remove(item))
            {
                if (_itemsByArea.TryGetValue(item.Area, out var areaList))
                    areaList.Remove(item);
            }
        }

        public ItemController FindUsedItem(int targetArea, ItemType targetItem)
        {
            // Used by NPCs (cleaner, loader) — skip items already claimed by another worker
            // so two cleaners don't both try to grab the same torch.
            if (targetArea == -1)
            {
                foreach (var item in _items)
                {
                    if (item != null && item.Type == targetItem && !item.IsClaimed)
                        return item;
                }
                return null;
            }

            if (_itemsByArea.TryGetValue(targetArea, out var areaList))
            {
                foreach (var item in areaList)
                {
                    if (item != null && item.Type == targetItem && !item.IsClaimed)
                        return item;
                }
            }
            return null;
        }

        public ItemController FindClosestUsedItem(Vector3 playerPosition)
        {
            foreach (var item in _items)
            {
                if (item == null) continue;

                var delta = item.Transform.position - playerPosition;
                delta.y = 0f;
                if (delta.sqrMagnitude < item.Radius * item.Radius)
                    return item;
            }
            return null;
        }

        public void Dispose()
        {
        }
    }
}
