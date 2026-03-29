using UnityEngine;
using UnityEngine.UI;
using System;

namespace Game.UI.Hud
{
    public sealed class ShopHudView : BaseHud
    {
        public event Action ON_APPLICATION_FOCUS;

        [SerializeField] private Button _closeBtn;
        [SerializeField] private ShopProductView[] _productsIAP;
        [SerializeField] private ShopProductView[] _productForAds;

        public Button CloseBtn => _closeBtn;
        public ShopProductView[] ProductsIAP => _productsIAP;
        public ShopProductView[] ProductForAds => _productForAds;

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }

        void OnApplicationFocus(bool focus)
        {
            if (focus == true)
                ON_APPLICATION_FOCUS?.Invoke();
        }
    }
}


