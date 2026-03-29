using System;
using Game.Level.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Game.UI.Hud
{
    public sealed class PlayerSlotView : BaseHudWithModel<PlayerModel>
    {
        public Action<PlayerModel> ON_CLICK;

        [SerializeField] private TMP_Text _labelText;
        [SerializeField] private TMP_Text _selectedText;
        [SerializeField] private Image _icon;
        [SerializeField] private Button _button;
        [SerializeField] private ScrollView _attributesScroll;

        public ScrollView AttributesScroll => _attributesScroll;

        protected override void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClick);
        }

        protected override void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClick);
        }

        protected override void OnModelChanged(PlayerModel model)
        {
            _labelText.text = model.Label;
            _icon.sprite = model.Icon;
            _selectedText.gameObject.SetActive(model.IsSelected);
        }

        private void OnButtonClick()
        {
            ON_CLICK.SafeInvoke(Model);
        }
    }
}

