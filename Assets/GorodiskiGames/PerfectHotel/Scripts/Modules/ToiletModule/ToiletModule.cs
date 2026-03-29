using Game.Config;
using Game.Level;
using Game.Level.Item;
using Game.Level.Toilet;
using Injection;
using UnityEngine;

namespace Game.Modules.ToiletModule
{
    public sealed class ToiletModule : Module<ToiletModuleView>
    {
        [Inject] private GameManager _gameManager;
        [Inject] private LevelView _levelView;
        [Inject] private Context _context;
        [Inject] private GameConfig _config;

        public ToiletModule(ToiletModuleView view) : base(view)
        {

        }

        public override void Initialize()
        {
            SetToilets();

            _gameManager.Player.ON_IDLE += OnPlayerIdle;
        }

        public override void Dispose()
        {
            _gameManager.Player.ON_IDLE -= OnPlayerIdle;

            foreach (var toilet in _gameManager.Toilets)
            {
                toilet.Dispose();
            }
            _gameManager.Toilets.Clear();
        }

        private void SetToilets()
        {
            foreach (var view in _view.ToiletViews)
            {
                var toilet = new ToiletController(view, _context);
                _levelView.AddReward(toilet.Model.Area, toilet.GetTotalReward());
                _gameManager.Toilets.Add(toilet);
                _gameManager.Entities.Add(toilet);
            }
        }

        private void OnPlayerIdle()
        {
            var item = FindNearestCabine();

            if (item == null) return;
            if (item.IsAvailable) return;

            _gameManager.FirePlayerTryDropInventory(item);
        }

        public ItemToiletController FindNearestCabine()
        {
            ItemToiletController result = null;
            float minDistance = float.MaxValue;
            var playerPosition = new Vector2(_gameManager.Player.View.Position.x, _gameManager.Player.View.Position.z);
            float itemRadius = _config.ToiletItemRadius;

            foreach (var toilet in _gameManager.Toilets)
            {
                foreach (var item in toilet.CabinesMap.Keys)
                {
                    var itemPosition = new Vector2(item.View.Position.x, item.View.Position.z);
                    var distance = Vector2.Distance(itemPosition, playerPosition);

                    if (distance <= itemRadius && distance < minDistance)
                    {
                        minDistance = distance;
                        result = item;
                    }
                }
            }
            return result;
        }
    }
}


