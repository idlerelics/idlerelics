using System;
using Game.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Game.UI.Hud
{
    /// <summary>
    /// View component for a single shop product entry in the Shop HUD.
    /// Displays the product's title, amount (for cash products), and price.
    /// Handles its own button click and can be toggled between active and
    /// inactive states (used for cooldown periods on ad-based products).
    ///
    /// Each product is configured via a ShopProductConfig ScriptableObject
    /// assigned in the Inspector, which defines its ID, title, reward type, etc.
    ///
    /// "sealed" means no other class can inherit from ShopProductView.
    /// </summary>
    public sealed class ShopProductView : BaseHud
    {
        // Format string for displaying cash amounts, e.g., "[coin icon] 1,000"
        private const string _cashPattern = "{0} {1}";

        /// <summary>
        /// Event fired when this product's button is clicked, passing the product's ID.
        /// The mediator subscribes to this to initiate the purchase or ad flow.
        ///
        /// "Action&lt;string&gt;" is a delegate that takes a string parameter (the product ID).
        /// </summary>
        public Action<string> ON_CLICK;

        // The ScriptableObject configuration for this product (ID, title, reward type, amounts)
        [SerializeField] private ShopProductConfig _config;

        /// <summary>The product's configuration data (assigned in Inspector).</summary>
        public ShopProductConfig Config => _config;

        // UI element references assigned in the Inspector
        [SerializeField] private Button _button;          // The clickable purchase button
        [SerializeField] private TMP_Text _titleText;     // Product title label
        [SerializeField] private TMP_Text _amountText;    // Cash amount display (for cash products)
        [SerializeField] private TMP_Text _priceText;     // Price label (real money or "FREE")
        [SerializeField] private Image _imageRim;         // Border/rim image around the product
        [SerializeField] private Image _imageBG;          // Background image of the product
        [SerializeField] private GameObject _imageFade;    // Fade overlay (visible when active)
        [SerializeField] private Color _inactiveColor;    // Background color when on cooldown
        [SerializeField] private Color _inactiveColorRim; // Rim color when on cooldown

        // Cached active-state colors, captured in Awake before any changes
        private Color _imageRimActiveColor;
        private Color _imageBGActiveColor;

        /// <summary>
        /// Exposes the price text for direct updates by the mediator
        /// (e.g., showing countdown timers during cooldown).
        /// </summary>
        public TMP_Text PriceText => _priceText;

        /// <summary>
        /// Awake is called once when the script instance is loaded.
        /// Caches the default (active) colors of the rim and background images
        /// so they can be restored when the product becomes interactable again.
        /// </summary>
        private void Awake()
        {
            _imageRimActiveColor = _imageRim.color;
            _imageBGActiveColor = _imageBG.color;
        }

        /// <summary>
        /// Initializes the product view with display data.
        /// Sets the title from the config and the price from the parameter.
        /// For cash reward products, also displays the cash amount with a green color
        /// and an inline coin icon.
        ///
        /// "as" is a safe cast -- returns null if the cast fails (e.g., for NoAds products).
        /// ColorUtil.ColorString wraps text in TextMeshPro color tags.
        /// </summary>
        /// <param name="price">The formatted price string (e.g., "$0.99" or "[ad icon] FREE").</param>
        public void Initialize(string price)
        {
            _titleText.text = _config.Title;

            _priceText.text = price;

            // NoAds products don't have a cash amount to display
            if (_config.Reward == ShopProductReward.NoAds) return;

            // For cash products, show the reward amount in green with a coin icon
            var config = _config as ShopProductWithScenarioConfig;
            var amount = string.Format(_cashPattern, GameConstants.CashIcon, config.Amount);
            var result = ColorUtil.ColorString(amount, Color.green);
            _amountText.text = result;
        }

        /// <summary>
        /// Called when this product's GameObject becomes active.
        /// Subscribes to the button's click event.
        /// </summary>
        protected override void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClick);
        }

        /// <summary>
        /// Called when this product's GameObject becomes inactive.
        /// Unsubscribes from the button's click event.
        /// Always pair AddListener with RemoveListener to avoid memory leaks.
        /// </summary>
        protected override void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClick);
        }

        /// <summary>
        /// Button click handler. Fires the ON_CLICK event with the product's config ID
        /// so the mediator knows which product was clicked.
        /// </summary>
        private void OnButtonClick()
        {
            ON_CLICK?.Invoke(_config.ID);
        }

        /// <summary>
        /// Toggles the product between active (interactable) and inactive (cooldown) states.
        /// When inactive:
        /// - The button cannot be clicked (interactable = false)
        /// - The rim and background change to muted/inactive colors
        /// - The fade overlay is hidden
        ///
        /// When active:
        /// - The button is clickable
        /// - Original colors are restored
        /// - The fade overlay is shown
        ///
        /// "internal" means this method is accessible within the same assembly but not from
        /// external assemblies.
        /// </summary>
        /// <param name="isInteractable">True to make the product clickable, false to disable it.</param>
        internal void SetInteractable(bool isInteractable)
        {
            _button.interactable = isInteractable;

            // Use active colors by default
            var imageRimColor = _imageRimActiveColor;
            var imageBGColor = _imageBGActiveColor;

            // Override with inactive colors when on cooldown
            if (isInteractable == false)
            {
                imageRimColor = _inactiveColorRim;
                imageBGColor = _inactiveColor;
            }

            _imageRim.color = imageRimColor;
            _imageBG.color = imageBGColor;
            _imageFade.SetActive(isInteractable);
        }
    }
}
