using Game.Level.Toilet;
using UnityEngine;

namespace Game.Modules.ToiletModule
{
    public sealed class ToiletModuleView : MonoBehaviour
    {
        [HideInInspector]
        public ToiletView[] ToiletViews;

        private void Awake()
        {
            ToiletViews = GetComponentsInChildren<ToiletView>();
        }
    }
}

