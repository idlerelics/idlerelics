using UnityEngine;
using UnityEngine.UI;
using System;

namespace Game.UI.Hud
{
    /// <summary>
    /// View component for the Shop HUD screen. Holds references to the UI elements
    /// needed by the ShopHudMediator: close button and arrays of product views
    /// for both IAP (real money) and ad-based products.
    ///
    /// Also detects when the app regains focus, so the mediator can recalculate
    /// cooldown timers that may have expired while the app was backgrounded.
    ///
    /// "sealed" means no other class can inherit from ShopHudView.
    /// </summary>
    public sealed class ShopHudView : BaseHud
    {
        /// <summary>
        /// Event fired when the app regains focus (player returns from another app).
        /// The mediator uses this to recalculate cooldown timers with real system time.
        /// </summary>
        public event Action ON_APPLICATION_FOCUS;

        // UI element references assigned in the Unity Inspector
        [SerializeField] private Button _closeBtn;                 // Button to close the shop
        [SerializeField] private ShopProductView[] _productsIAP;   // Products purchased with real money
        [SerializeField] private ShopProductView[] _productForAds; // Products earned by watching ads

        /// <summary>Button that closes the shop HUD.</summary>
        public Button CloseBtn => _closeBtn;

        /// <summary>Array of IAP (In-App Purchase) product views for real-money items.</summary>
        public ShopProductView[] ProductsIAP => _productsIAP;

        /// <summary>Array of ad-based product views (earned by watching rewarded ads).</summary>
        public ShopProductView[] ProductForAds => _productForAds;

        /// <summary>
        /// Intentionally empty -- event subscriptions are handled by the mediator.
        /// Required by the abstract BaseHud class.
        /// </summary>
        protected override void OnEnable()
        {

        }

        /// <summary>
        /// Intentionally empty -- event unsubscriptions are handled by the mediator.
        /// Required by the abstract BaseHud class.
        /// </summary>
        protected override void OnDisable()
        {

        }

        /// <summary>
        /// Unity lifecycle callback that fires when the app gains or loses focus.
        /// On mobile, this happens when the player switches away from and back to the app.
        /// Only fires the event when focus is regained (focus == true).
        ///
        /// This is important for cooldown timers -- Time.deltaTime does not advance
        /// while the app is backgrounded, so timers must be recalculated using
        /// the real system clock when the app returns to focus.
        /// </summary>
        /// <param name="focus">True when the app gains focus, false when it loses focus.</param>
        void OnApplicationFocus(bool focus)
        {
            if (focus == true)
                ON_APPLICATION_FOCUS?.Invoke();
        }
    }
}
