using System.Collections.Generic;
using System.Linq;
using Game.Config;
using Game.Core;
using Game.Level.Item;
using Injection;
using UnityEngine;

namespace Game.Level.Utility.UtilityStates
{
    public sealed class UtilityPurchasedState : UtilityState
    {
        private const float _delay = 0.5f;

        [Inject] private GameConfig _config;
        [Inject] private Timer _timer;

        private readonly Dictionary<ItemController, float> _itemsMap;

        public UtilityPurchasedState()
        {
            _itemsMap = new Dictionary<ItemController, float>();
        }

        public override void Initialize()
        {
            _utility.View.MeshesHolder.SetActive(true);
            _utility.View.HideWallsHiddenState();

            OnLvlChanged(_gameManager.Model.LoadLvl());

            _gameManager.LEVEL_CHANGED += OnLvlChanged;
            _timer.TICK += OnTick;
        }

        public override void Dispose()
        {
            _gameManager.LEVEL_CHANGED -= OnLvlChanged;
            _timer.TICK -= OnTick;
        }


        private void OnLvlChanged(int lvl)
        {
            _utility.View.OutsideWalls.MeshesVisibilityLvl(lvl);
        }

        private void OnTick()
        {
            foreach (var inventory in _itemsMap.Keys.ToList())
            {
                float value = _itemsMap[inventory];

                if (value >= _delay) return;

                value += Time.deltaTime;
                _itemsMap[inventory] = value;

                if (value >= _delay)
                    AddItem(inventory);
            }
        }

        void AddItem(ItemController item)
        {
            item.Model.Duration = _config.ToiletPaperFlyTime;
            item.Model.DurationNominal = item.Model.Duration;
            item.Model.SetChanged();

            _gameManager.AddItem(item);

            item.ITEM_FINISHED += OnItemFinished;
        }

        void OnItemFinished(ItemController item)
        {
            item.ITEM_FINISHED -= OnItemFinished;

            _gameManager.Model.Save();
            _gameManager.Model.SetChanged();

            _itemsMap[item] = 0f;
        }
    }
}


