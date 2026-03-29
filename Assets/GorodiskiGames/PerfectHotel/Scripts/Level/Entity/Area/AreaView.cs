using Game.Config;
using Game.Level.Hotel;
using Game.Level.Item;
using Game.Level.Place;
using UnityEngine;

namespace Game.Level.Area
{
    public sealed class AreaView : EntityWithHudView
    {
        [SerializeField] private ItemView _itemBuyUpdateView;
        [SerializeField] private GameObject _floorsHolder;
        [SerializeField] private GameObject _hidingWallsHolder;
        [SerializeField] private GameObject _permanentWallsHolder;
        [HideInInspector] [SerializeField] private ConstructionItemView[] _floorViews;
        [HideInInspector] [SerializeField] private HotelWallView[] _hidingWallsViews;
        [HideInInspector] [SerializeField] private ConstructionItemView[] _permanentWallsViews;
        [SerializeField] private AreaConfig _config;

        public ItemView ItemBuyUpdateView => _itemBuyUpdateView;
        public AreaConfig Config => _config;
        public HotelWallView[] HidingWallsViews => _hidingWallsViews;

        private void Awake()
        {
            _floorViews = _floorsHolder.GetComponentsInChildren<ConstructionItemView>();
            _hidingWallsViews = _hidingWallsHolder.GetComponentsInChildren<HotelWallView>();
            _permanentWallsViews = _permanentWallsHolder.GetComponentsInChildren<ConstructionItemView>();
        }

        internal void HideFloors()
        {
            foreach (var floorView in _floorViews)
            {
                floorView.HideAllMeshes();
            }
        }
        internal void UpdateFloors(int lvl)
        {
            foreach (var floorView in _floorViews)
            {
                floorView.MeshesVisibilityLvl(lvl);
            }
        }

        internal void HideHidingWalls()
        {
            foreach (var wallView in _hidingWallsViews)
            {
                wallView.Walls.HideAllMeshes();
            }
        }
        internal void UpdateHidingWalls(int lvl)
        {
            foreach (var wallView in _hidingWallsViews)
            {
                wallView.Walls.MeshesVisibilityLvl(lvl);
            }
        }

        internal void HidePermanentWalls()
        {
            foreach (var wallView in _permanentWallsViews)
            {
                wallView.HideAllMeshes();
            }
        }
        internal void UpdatePermanentWalls(int lvl)
        {
            foreach (var wallView in _permanentWallsViews)
            {
                wallView.MeshesVisibilityLvl(lvl);
            }
        }
    }
}