using Game.Core;
using Game.Core.UI;
using Game.Managers;
using Injection;
using UnityEngine;

namespace Game.UI.Hud
{
    /// <summary>
    /// Mediator for the Purchase HUD overlay that displays status messages
    /// during IAP (In-App Purchase) transactions and purchase restoration.
    ///
    /// Shows a blocking overlay with status text during these flows:
    /// - "PURCHASE PROCESSING..." while a purchase is being processed
    /// - Error messages if a purchase fails
    /// - "RESTORING PURCHASES..." during purchase restoration (iOS)
    /// - Result messages after restoration completes
    ///
    /// Messages auto-hide after a short delay (1 second).
    /// "sealed" means no other class can inherit from PurchaseHudMediator.
    /// </summary>
    public sealed class PurchaseHudMediator : Mediator<PurchaseHudView>
    {
        // How long to show status/error messages before auto-hiding (in seconds)
        private const float _delay = 1f;

        // Status message constants
        private const string _purchaseProcessingWord = "PURCHASE PROCESSING...";
        private const string _purchaseRestoringWord = "RESTORING PURCHASES...";

        // Injected dependencies resolved by the custom DI container
        [Inject] private IAPManager _IAPManager;   // Handles purchase events and callbacks
        [Inject] private Timer _timer;             // Frame-based timer for auto-hide countdown

        // The Time.time value at which the current message should auto-hide.
        // Set to float.MaxValue when no auto-hide is pending.
        private float _hideTime;

        /// <summary>
        /// Called when the purchase HUD is shown. Initializes the display to a blank state
        /// and subscribes to all IAP-related events (purchase click, fail, complete, restore).
        ///
        /// The Timer.TICK event fires every frame, used to check if the auto-hide
        /// time has been reached.
        /// </summary>
        protected override void Show()
        {
            // Set to max so no auto-hide happens until triggered
            _hideTime = float.MaxValue;

            // Start with a clean slate (no text, no background)
            _view.InfoText.text = "";
            _view.BackgroundImage.SetActive(false);

            // Subscribe to purchase lifecycle events
            _IAPManager.ON_PURCHASE_CLICKED += OnPurchaseClicked;
            _IAPManager.ON_PURCHASE_FAILED += OnPurchaseFailed;
            _IAPManager.ON_PURCHASE_PROCESS_COMPLETE += OnPurchaseProcessComplete;

            // Subscribe to restore purchase events (used primarily on iOS)
            _IAPManager.ON_RESTORE_PURCHASES += OnRestorePurchases;
            _IAPManager.ON_RESTORE_PURCHASES_END += OnRestorePurchasesEnd;

            // Subscribe to per-frame updates for auto-hide countdown
            _timer.TICK += OnTICK;
        }

        /// <summary>
        /// Called when the purchase HUD is hidden. Hides the background overlay
        /// and unsubscribes all IAP and timer events.
        /// </summary>
        protected override void Hide()
        {
            _view.BackgroundImage.SetActive(false);

            _IAPManager.ON_PURCHASE_CLICKED -= OnPurchaseClicked;
            _IAPManager.ON_PURCHASE_FAILED -= OnPurchaseFailed;
            _IAPManager.ON_PURCHASE_PROCESS_COMPLETE -= OnPurchaseProcessComplete;

            _IAPManager.ON_RESTORE_PURCHASES -= OnRestorePurchases;
            _IAPManager.ON_RESTORE_PURCHASES_END -= OnRestorePurchasesEnd;

            _timer.TICK -= OnTICK;
        }

        /// <summary>
        /// Called every frame via the Timer. Checks if the auto-hide time has been reached.
        /// If so, clears the status text and hides the background overlay.
        ///
        /// Time.time is Unity's built-in time since the game started (in seconds).
        /// Using Time.time instead of a countdown avoids accumulating floating-point errors.
        /// </summary>
        private void OnTICK()
        {
            if (Time.time < _hideTime)
                return;

            // Auto-hide triggered: reset display and disable further hiding
            _hideTime = float.MaxValue;
            _view.InfoText.text = "";
            _view.BackgroundImage.SetActive(false);
        }

        /// <summary>
        /// Called when the player initiates a purchase.
        /// Shows the blocking overlay with a "processing" message.
        /// The background prevents the player from interacting with other UI
        /// while the purchase is being processed by the app store.
        /// </summary>
        private void OnPurchaseClicked()
        {
            _view.InfoText.text = _purchaseProcessingWord;
            _view.BackgroundImage.SetActive(true);
        }

        /// <summary>
        /// Called when a purchase fails (e.g., cancelled, network error).
        /// Displays the error info and schedules auto-hide after the delay.
        /// </summary>
        /// <param name="info">The error or failure message from the IAP system.</param>
        private void OnPurchaseFailed(string info)
        {
            _view.InfoText.text = info;
            _hideTime = Time.time + _delay;
        }

        /// <summary>
        /// Called when a purchase is successfully processed.
        /// Schedules auto-hide after the delay (the overlay will clear automatically).
        /// The reward granting is handled by ShopHudMediator.OnProductPurchased.
        /// </summary>
        private void OnPurchaseProcessComplete()
        {
            _hideTime = Time.time + _delay;
        }

        /// <summary>
        /// Called when purchase restoration begins (typically iOS "Restore Purchases" button).
        /// Shows the blocking overlay with a "restoring" message.
        /// </summary>
        private void OnRestorePurchases()
        {
            _view.InfoText.text = _purchaseRestoringWord;
            _view.BackgroundImage.SetActive(true);
        }

        /// <summary>
        /// Called when purchase restoration completes.
        /// Displays the result info (e.g., "Purchases restored" or an error)
        /// and schedules auto-hide after the delay.
        /// </summary>
        /// <param name="info">The result message from the restoration process.</param>
        private void OnRestorePurchasesEnd(string info)
        {
            _view.InfoText.text = info;
            _hideTime = Time.time + _delay;
        }
    }
}
