using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class RoomSlotView : BaseHud
    {
        public event Action<int> ON_SLOT_CLICK;

        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;

        private int _index;

        public void Initialize(Sprite icon, int index)
        {
            _icon.sprite = icon;
            _index = index;
        }

        protected override void OnEnable()
        {
            _button.onClick.AddListener(OnSlotButtonClick);
        }

        protected override void OnDisable()
        {
            _button.onClick.RemoveListener(OnSlotButtonClick);
        }

        void OnSlotButtonClick()
        {
            ON_SLOT_CLICK?.Invoke(_index);
        }
    }
}

