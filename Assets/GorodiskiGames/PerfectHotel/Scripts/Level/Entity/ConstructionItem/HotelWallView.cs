using Game.Level.Place;
using UnityEngine;

namespace Game.Level.Hotel
{
    public sealed class HotelWallView : MonoBehaviour
    {
        [SerializeField] private int _hideOnArea;
        [SerializeField] private ConstructionItemView _walls;

        public int HideOnArea => _hideOnArea;
        public ConstructionItemView Walls => _walls;
    }
}

