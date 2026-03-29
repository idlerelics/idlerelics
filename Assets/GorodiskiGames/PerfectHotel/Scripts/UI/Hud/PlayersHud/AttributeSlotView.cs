using Game.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Core;
using Utilities;

namespace Game.UI.Hud
{
    public sealed class AttributeModel : Observable
    {
        public string Label;
        public AttributeType Type;
        public Sprite Icon;
        public float NominalValue;
        public float AddValue;
        public float Value;

        public AttributeModel(AttributeConfig config, float addValue)
        {
            Label = config.LabelKey;
            Type = config.Type;
            Icon = config.Icon;
            NominalValue = config.NominalValue;
            AddValue = addValue;
            Value = NominalValue + addValue;
        }
    }

    public sealed class AttributeSlotView : BaseHudWithModel<AttributeModel>
    {
        private const string _addValueFormat = "+{0}";
        private const string _valueFormat = "{0}";
        private const string _valueAddValueFormat = "{0} ({1})";

        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _labelText;
        [SerializeField] private TMP_Text _valueText;

        protected override void OnEnable()
        {
        }

        protected override void OnDisable()
        {
        }

        protected override void OnModelChanged(AttributeModel model)
        {
            _icon.sprite = model.Icon;
            _labelText.text = model.Label;

            var addValue = model.AddValue;
            var addValueString = ColorUtil.ColorString(string.Format(_addValueFormat, addValue), Color.green);
            var valueresult = string.Format(_valueAddValueFormat, model.Value, addValueString);

            if (addValue <= 0)
                valueresult = string.Format(_valueFormat, model.Value);

            _valueText.text = valueresult;
        }
    }
}

