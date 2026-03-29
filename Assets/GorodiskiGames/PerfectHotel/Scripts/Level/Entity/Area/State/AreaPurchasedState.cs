using System.Collections.Generic;
using Game.Level.Hotel;
using Injection;

namespace Game.Level.Area
{
    public sealed class AreaPurchasedState : AreaState
    {
        [Inject] private GameManager _gameManager;

        private Dictionary<AreaController, List<HotelWallView>> _wallAreaMap;

        public override void Initialize()
        {
            _wallAreaMap = new Dictionary<AreaController, List<HotelWallView>>();
            foreach (var wall in _area.View.HidingWallsViews)
            {
                var area = _gameManager.FindArea(wall.HideOnArea);

                if (!_wallAreaMap.TryGetValue(area, out var wallList))
                {
                    wallList = new List<HotelWallView>();
                    _wallAreaMap.Add(area, wallList);
                }

                wallList.Add(wall);

                if (area.Model.IsPurchased)
                    OnAreaPurchased(area);
            }

            _area.View.HudView.gameObject.SetActive(false);

            OnLvlChanged(_gameManager.Model.LoadLvl());

            _gameManager.LEVEL_CHANGED += OnLvlChanged;
            _gameManager.AREA_PURCHASED += OnAreaPurchased;
        }

        public override void Dispose()
        {
            _gameManager.LEVEL_CHANGED -= OnLvlChanged;
            _gameManager.AREA_PURCHASED -= OnAreaPurchased;
        }

        private void OnAreaPurchased(AreaController area)
        {
            if (_wallAreaMap.TryGetValue(area, out var walls))
            {
                foreach (var wall in walls)
                {
                    wall.gameObject.SetActive(false);
                }
            }
        }

        private void OnLvlChanged(int lvl)
        {
            _area.View.UpdateFloors(lvl);
            _area.View.UpdateHidingWalls(lvl);
            _area.View.UpdatePermanentWalls(lvl);
        }
    }
}

