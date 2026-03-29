using Game.Config;
using Game.Level.Place;
using UnityEngine;

namespace Game.Level.Room
{
    public sealed class RoomView : PlaceWithItemsReusableView
    {
        [SerializeField] private ConstructionInsideView _insideWalls;
        [SerializeField] private RoomConfig _config;
        [SerializeField] private GameObject _lightDark;
        [SerializeField] private Transform _customerPosition;

        public ConstructionInsideView InsideWalls => _insideWalls;
        public RoomConfig Config => _config;
        public Transform CustomerPosition => _customerPosition;

        internal void SetDarkLight(bool value)
        {
            _lightDark.SetActive(value);
        }

        public override void Awake()
        {
            base.Awake();
            _lightDark.SetActive(false);
        }
    }
}

