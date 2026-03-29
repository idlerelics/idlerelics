using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public class ScrollView : BaseHud
    {
        public event Action ON_SCROLL;

        [SerializeField] private RectTransform _content;
        [SerializeField] private GridLayoutGroup _layoutGroup;

        public RectTransform Content => _content;

        protected override void OnEnable()
        {
        }

        protected override void OnDisable()
        {
        }

        internal void SetContainerSize()
        {
            var items = _content.childCount;

            var itemsPerRow = _layoutGroup.constraintCount;
            int rows = (int)Mathf.Ceil((float)items / itemsPerRow);

            float paddingTop = _layoutGroup.padding.top;
            float paddingBottom = _layoutGroup.padding.bottom;
            float spacing = _layoutGroup.spacing.y;

            var ySlotSize = _layoutGroup.cellSize.y;
            float ySize = paddingTop + (rows * ySlotSize) + ((rows - 1) * spacing) + paddingBottom;
            _content.sizeDelta = new Vector2(_content.sizeDelta.x, ySize);
        }

        public void OnBeginDrag()
        {
            ON_SCROLL?.Invoke();
        }
    }
}

