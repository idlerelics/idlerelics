using UnityEngine;
using Game.Level.Place;

namespace Game.Level.Item
{
    public class ItemReusableView : ItemFillBarView
    {
        [SerializeField] private ConstructionItemView _available;
        [SerializeField] private ConstructionItemView _used;

        public ConstructionItemView Available => _available;
        public ConstructionItemView Used => _used;

        internal void SetVisual(bool isAvailable, int visualIndex)
        {
            if (isAvailable)
            {
                _available.MeshesVisibilityIndex(visualIndex);
                _used.HideAllMeshes();
            }
            else
            {
                _used.MeshesVisibilityIndex(visualIndex);
                _available.HideAllMeshes();
            }
        }
    }
}


