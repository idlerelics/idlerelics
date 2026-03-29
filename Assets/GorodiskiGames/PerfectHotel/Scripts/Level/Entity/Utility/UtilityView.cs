using Game.Config;
using Game.Level.Item;
using Game.Level.Place;
using UnityEngine;

namespace Game.Level.Utility
{
    public sealed class UtilityView : EntityView
    {
        [SerializeField] private int _sign;
        [SerializeField] private ItemUtilityView[] _items;
        [SerializeField] private UtilityConfig _config;
        [SerializeField] private GameObject _meshesHolder;
        [SerializeField] private ConstructionItemView _outsideWalls;
        [SerializeField] private GameObject _wallsHiddenStateHolder;

        private ConstructionItemView[] _wallsHiddenState;

        public ItemUtilityView[] Items => _items;
        public UtilityConfig Config => _config;
        public GameObject MeshesHolder => _meshesHolder;
        public ConstructionItemView OutsideWalls => _outsideWalls;
        public int Sign => _sign;

        private void Awake()
        {
            _wallsHiddenState = _wallsHiddenStateHolder.GetComponentsInChildren<ConstructionItemView>();
        }

        internal void HideWallsHiddenState()
        {
            foreach (var wallView in _wallsHiddenState)
            {
                wallView.HideAllMeshes();
            }
        }

        internal void UpdateWallsHiddenState(int lvl)
        {
            foreach (var wallView in _wallsHiddenState)
            {
                wallView.MeshesVisibilityLvl(lvl);
            }
        }
    }
}

