using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class HotelsHudView : BaseHud
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private RectTransform _container;
        [SerializeField] private GameObject _hotelSlotPrefab;

        public Button CloseButton => _closeButton;
        public GameObject HotelSlotPrefab => _hotelSlotPrefab;
        public RectTransform Container => _container;

        protected override void OnEnable()
        {
        }

        protected override void OnDisable()
        {
        }
    }
}

