using System;
using System.Collections.Generic;
using System.Linq;
using Game.Config;
using Game.Core;
using Game.Core.UI;
using Game.Managers;
using Injection;
using UnityEngine;
using Utilities;

namespace Game.UI.Hud
{
    /// <summary>
    /// Mediator for the Shop HUD screen. Manages two categories of shop products:
    /// 1. IAP (In-App Purchase) products -- bought with real money via the app store
    /// 2. Ad-based products -- earned by watching rewarded ads
    ///
    /// Ad-based products use a "scenario" cooldown system: after watching an ad,
    /// the product becomes temporarily unavailable. The cooldown duration follows
    /// a configurable sequence (scenario), and the entire cycle resets after a
    /// configurable number of hours.
    ///
    /// "sealed" means no other class can inherit from ShopHudMediator.
    /// </summary>
    public sealed class ShopHudMediator : Mediator<ShopHudView>
    {
		// PlayerPrefs key prefixes for persisting cooldown/scenario state
		private const string _allScenariosDatePrefix = "allScenariosDate";  // When all scenarios reset
		private const string _scenarioDatePrefix = "scenarioDate";          // When current cooldown ends
		private const string _scenarioIndexPrefix = "scenarioIndex";        // Current position in scenario sequence

		// Format strings for displaying product prices
		private const string _priceAdsFormat = "{0} {1}";    // e.g., "[ad icon] FREE"
		private const string _priceTimerFormat = "{0} {1}";  // e.g., "[clock icon] 00:05:30"
		private const string _adsWord = "FREE";

		// Injected dependencies resolved by the custom DI container
		[Inject] private IAPManager _IAPManager;       // Handles real-money purchases via app store
		[Inject] private GameManager _gameManager;     // Game state and model access
		[Inject] private GameConfig _config;           // Central game configuration
		[Inject] private AdsManager _adsManager;       // Manages ad display (banner, interstitial, rewarded)
		[Inject] private Timer _timer;                 // Frame-based timer for countdown updates

		// The product ID of the most recently clicked ad product (used in reward callback)
		private string _productID;

		// Pre-formatted price string for ad products (e.g., "[ad icon] FREE")
		private string _priceAds;

		// Cached clock icon string for countdown display
		private string _clockIcon;

		// Maps product IDs to their view components for quick lookup
		private Dictionary<string, ShopProductView> _productMap;

		// Tracks products currently on cooldown, with their remaining delay in seconds
		private Dictionary<ShopProductView, float> _productDelayMap;
		private readonly List<ShopProductView> _tempProducts = new List<ShopProductView>();

		public ShopHudMediator()
		{
			_productMap = new Dictionary<string, ShopProductView>();
			_productDelayMap = new Dictionary<ShopProductView, float>();
		}

		/// <summary>
		/// Called when the shop HUD is shown. Sets up all products (both IAP and ad-based),
		/// subscribes to purchase events, ad events, timer ticks, and button clicks.
		///
		/// GameConstants.AdsIcon and ClockIcon are TextMeshPro sprite tags that render
		/// inline icons within text strings.
		/// </summary>
		protected override void Show()
		{
			// Pre-format the "FREE" price string with the ad icon
			_priceAds = string.Format(_priceAdsFormat, GameConstants.AdsIcon, _adsWord);
			_clockIcon = GameConstants.ClockIcon;

			// Hide "No Ads" product if the player already purchased it
			NoAdsProductVisibility();

			// Initialize both product types
			SetProductsForAds();
			SetProductsIAP();

			// Subscribe to IAP events
			_IAPManager.ON_INITIALIZED += OnInitialized;
			_IAPManager.ON_PRODUCT_PURCHASED += OnProductPurchased;

			// Subscribe to click events on all IAP product views
			foreach (var product in _view.ProductsIAP)
			{
				product.ON_CLICK += OnProductIAPClick;
			}
			// Subscribe to click events on all ad-based product views
			foreach (var product in _view.ProductForAds)
			{
				product.ON_CLICK += OnProductForAdsClick;
			}

			// Subscribe to ad completion and UI events
			_adsManager.ON_REWARDED_WATCHED += OnRewardedWatched;
			_view.CloseBtn.onClick.AddListener(CloseBtnClick);

			// Listen for app focus changes (to recalculate cooldown timers)
			_view.ON_APPLICATION_FOCUS += OnApplicationFocus;

			// Subscribe to per-frame timer for countdown updates
			_timer.TICK += OnTick;
		}

		/// <summary>
		/// Called when the shop HUD is hidden. Unsubscribes all events to prevent
		/// stale callbacks and memory leaks.
		/// </summary>
		protected override void Hide()
		{
			_IAPManager.ON_INITIALIZED -= OnInitialized;
			_IAPManager.ON_PRODUCT_PURCHASED -= OnProductPurchased;

			foreach (var product in _view.ProductsIAP)
			{
				product.ON_CLICK -= OnProductIAPClick;
			}
			foreach (var product in _view.ProductForAds)
			{
				product.ON_CLICK -= OnProductForAdsClick;
			}

			_adsManager.ON_REWARDED_WATCHED -= OnRewardedWatched;
			_view.CloseBtn.onClick.RemoveListener(CloseBtnClick);

			_view.ON_APPLICATION_FOCUS -= OnApplicationFocus;
			_timer.TICK -= OnTick;
		}

		/// <summary>
		/// Called when the IAP system finishes initializing (may happen asynchronously).
		/// Re-initializes IAP products to display their real prices from the store.
		/// Unsubscribes from the event since initialization only happens once.
		/// </summary>
		private void OnInitialized()
		{
			_IAPManager.ON_INITIALIZED -= OnInitialized;
			SetProductsIAP();
		}

		/// <summary>
		/// Called every frame via the Timer. Counts down the cooldown timers for
		/// all ad products currently on delay, updating their displayed time remaining.
		///
		/// Uses .ToList() to create a copy of the keys collection before iterating,
		/// because UpdateProductDelay may remove entries from the dictionary.
		/// </summary>
		private void OnTick()
		{
			_tempProducts.Clear();
			_tempProducts.AddRange(_productDelayMap.Keys);
			foreach (var product in _tempProducts)
			{
				var delay = _productDelayMap[product];
				delay -= Time.deltaTime;
				UpdateProductDelay(product, delay);
			}
		}

		/// <summary>
		/// Initializes all ad-based products with the "FREE" price label
		/// and checks if each product is currently on cooldown.
		/// </summary>
		private void SetProductsForAds()
		{
			foreach (var product in _view.ProductForAds)
			{
				product.Initialize(_priceAds);

				TryToAddProduct(product);
				CheckInteractable(product);
			}
		}

		/// <summary>
		/// Initializes all IAP products with their real store prices.
		/// Validates each product exists in the GameConfig before setting up.
		///
		/// _IAPManager.GetPrice returns the localized price string from the app store
		/// (e.g., "$0.99", "0,99 EUR") or a placeholder if IAP is not yet initialized.
		/// </summary>
		private void SetProductsIAP()
		{
			foreach (var product in _view.ProductsIAP)
			{
				var config = product.Config;
				if (!_config.ShopProductIAPMap.Keys.Contains(config.ID))
                {
					Log.Warning(config.Title + " product not added to the GameConfig");
					return;
                }

                var price = _IAPManager.GetPrice(config.ID);
				product.Initialize(price);
				TryToAddProduct(product);
			}
		}

		/// <summary>
		/// Called when an IAP product button is clicked.
		/// Forwards the purchase request to the IAPManager, which handles
		/// the platform-specific app store purchase flow.
		/// </summary>
		/// <param name="productID">The store product identifier (e.g., "com.game.cash_100").</param>
		private void OnProductIAPClick(string productID)
		{
			_IAPManager.OnPurchaseClicked(productID);
		}

		/// <summary>
		/// Called when an ad-based product button is clicked.
		/// Stores the product ID for the reward callback, then shows a rewarded ad.
		/// The actual reward is granted in OnRewardedWatched when the ad completes.
		/// </summary>
		/// <param name="productID">The identifier of the ad product clicked.</param>
		private void OnProductForAdsClick(string productID)
		{
			_productID = productID;
			_adsManager.ShowRewarded();
		}

		/// <summary>
		/// Called when a product purchase is successfully completed (both IAP and ad-based).
		/// Grants the appropriate reward based on the product's configured reward type:
		/// - NoAds: permanently disables ads for the player
		/// - Cash: adds the configured amount to the player's cash
		///
		/// SetChanged() notifies all observers of the GameModel to update their UI.
		/// </summary>
		/// <param name="productID">The identifier of the purchased product.</param>
		private void OnProductPurchased(string productID)
		{
			var config = _productMap[productID].Config;
			var reward = config.Reward;
			if (reward == ShopProductReward.NoAds)
			{
				// Permanently disable ads
				_gameManager.Model.IsNoAds = true;
				_gameManager.Model.Save();
				_adsManager.SetNoAds();

				NoAdsProductVisibility();
			}
			else if (reward == ShopProductReward.Cash)
			{
				// Add cash reward -- cast to ShopProductWithScenarioConfig for the Amount field
				var newConfig = config as ShopProductWithScenarioConfig;

				_gameManager.Model.Cash += newConfig.Amount;
				_gameManager.Model.Save();
				_gameManager.Model.SetChanged();
			}
		}

		/// <summary>
		/// Close button handler. Hides the shop HUD via the base Mediator.
		/// </summary>
		private void CloseBtnClick()
		{
			InternalHide();
		}

		/// <summary>
		/// Hides the "No Ads" IAP product if the player has already purchased it.
		/// Prevents the player from buying the same product twice.
		/// </summary>
		private void NoAdsProductVisibility()
		{
			foreach (var product in _view.ProductsIAP)
			{
				if(product.Config.Reward == ShopProductReward.NoAds && _gameManager.Model.IsNoAds)
					product.gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// Called when a rewarded ad is fully watched. Grants the reward,
		/// advances the scenario index, calculates the next cooldown duration,
		/// and starts the cooldown timer for that product.
		///
		/// The scenario system works like this:
		/// 1. Each ad product has a sequence of cooldown durations (e.g., [0, 5, 10, 30] minutes)
		/// 2. After each ad watch, the next duration in the sequence is used
		/// 3. After all scenarios are exhausted, the entire cycle resets
		/// </summary>
		private void OnRewardedWatched()
		{
			// Grant the reward for the watched ad
			OnProductPurchased(_productID);

			var product = _productMap[_productID];
			var productID = product.Config.ID;

			// Check if the full scenario cycle needs to reset
			CheckIsNeedReset(product);

			// Advance to the next scenario step
			var scenarioIndex = LoadScenarioIndex(productID);
			scenarioIndex++;
			SaveScenarioIndex(productID, scenarioIndex);

			// Calculate and save the cooldown end time
			var duration = GetNewDuration(product);
			var scenarioDate = DateTime.Now.AddMinutes(duration);
			SaveDate(productID, _scenarioDatePrefix, scenarioDate);

			// Update the product's interactable state (will start showing countdown)
			CheckInteractable(product);
		}

		/// <summary>
		/// Registers a product in the product map if it hasn't been added yet.
		/// Uses the product's config ID as the key for quick lookup.
		/// </summary>
		/// <param name="product">The shop product view to register.</param>
		private void TryToAddProduct(ShopProductView product)
		{
			if (!_productMap.Values.Contains(product))
				_productMap.Add(product.Config.ID, product);
		}

		/// <summary>
		/// Checks if a product is currently on cooldown and updates its interactable state.
		/// If on cooldown, the product is grayed out and added to the delay tracking map
		/// so the countdown timer updates each frame.
		/// </summary>
		/// <param name="product">The shop product view to check.</param>
		private void CheckInteractable(ShopProductView product)
		{
			var productID = product.Config.ID;

			var currentDelay = GetCurrentDelay(productID);
			bool isInteractable = currentDelay <= 0f;

			// If on cooldown, add to the delay map for per-frame countdown updates
			if (isInteractable == false)
				_productDelayMap.Add(product, currentDelay);

			product.SetInteractable(isInteractable);
		}

		/// <summary>
		/// Calculates the remaining cooldown time (in seconds) for a product.
		/// Returns a positive value if on cooldown, or zero/negative if ready.
		///
		/// DateTime.Subtract returns a TimeSpan, and TotalSeconds converts it to a float.
		/// </summary>
		/// <param name="productID">The product identifier to check.</param>
		/// <returns>Remaining cooldown time in seconds (negative means ready).</returns>
		float GetCurrentDelay(string productID)
		{
			var scenarioDate = LoadDate(productID, _scenarioDatePrefix);
			var result = (float)scenarioDate.Subtract(DateTime.Now).TotalSeconds;
			return result;
		}

		/// <summary>
		/// Loads a persisted DateTime from PlayerPrefs using a composite key.
		/// Dates are stored as binary (long) values converted to strings.
		///
		/// DateTime.ToBinary() converts a DateTime to a long for storage.
		/// DateTime.FromBinary() converts it back.
		/// The default value is DateTime.Now if no saved date exists.
		/// </summary>
		/// <param name="productID">The product identifier.</param>
		/// <param name="prefix">The key prefix (scenario date or all-scenarios date).</param>
		/// <returns>The loaded DateTime, or DateTime.Now if not found.</returns>
		DateTime LoadDate(string productID, string prefix)
		{
			var key = productID + prefix;
			var dateString = PlayerPrefs.GetString(key, DateTime.Now.ToBinary().ToString());
			var dateLong = Convert.ToInt64(dateString);
			return DateTime.FromBinary(dateLong);
		}

		/// <summary>
		/// Checks if the entire scenario cycle needs to reset for a product.
		/// The cycle resets after a configurable number of hours (AllScenariosDurationHrs).
		/// On reset, the scenario index goes back to 0 and the cooldown clears.
		/// </summary>
		/// <param name="product">The shop product view to check.</param>
		private void CheckIsNeedReset(ShopProductView product)
		{
			var productID = product.Config.ID;
			var allScenariosDate = LoadDate(productID, _allScenariosDatePrefix);
			Log.Info("Product: " + productID + ". all scenarios reset date: " + allScenariosDate);
			var resetDelay = (float)allScenariosDate.Subtract(DateTime.Now).TotalSeconds;
			bool isNeedReset = resetDelay <= 0f;
			if (isNeedReset == true)
			{
				// Reset the full cycle: set new reset time, clear cooldown, reset scenario index
				SaveDate(productID, _allScenariosDatePrefix, DateTime.Now.AddHours(_config.AllScenariosDurationHrs));
				SaveDate(productID, _scenarioDatePrefix, DateTime.Now);
				SaveScenarioIndex(productID, 0);

				var allScenariosNewDate = LoadDate(productID, _allScenariosDatePrefix);
				Log.Info("Reseted. New all scenarios reset date " + allScenariosNewDate);
			}
		}

		/// <summary>
		/// Persists a DateTime to PlayerPrefs using a composite key.
		/// Calls PlayerPrefs.Save() to immediately flush to disk.
		/// </summary>
		/// <param name="productID">The product identifier.</param>
		/// <param name="prefix">The key prefix.</param>
		/// <param name="date">The DateTime value to save.</param>
		private void SaveDate(string productID, string prefix, DateTime date)
		{
			var key = productID + prefix;
			PlayerPrefs.SetString(key, date.ToBinary().ToString());
			PlayerPrefs.Save();
		}

		/// <summary>
		/// Loads the current scenario index for a product from PlayerPrefs.
		/// Defaults to 0 (first scenario step) if no value has been saved.
		/// </summary>
		/// <param name="productID">The product identifier.</param>
		/// <returns>The current scenario step index.</returns>
		private int LoadScenarioIndex(string productID)
		{
			var key = productID + _scenarioIndexPrefix;
			return PlayerPrefs.GetInt(key, 0);
		}

		/// <summary>
		/// Saves the current scenario index to PlayerPrefs.
		/// </summary>
		/// <param name="productID">The product identifier.</param>
		/// <param name="index">The scenario step index to save.</param>
		private void SaveScenarioIndex(string productID, int index)
		{
			var key = productID + _scenarioIndexPrefix;
			PlayerPrefs.SetInt(key, index);
			PlayerPrefs.Save();
		}

		/// <summary>
		/// Determines the cooldown duration (in minutes) for the next ad watch
		/// based on the product's scenario configuration.
		///
		/// The scenario is an array of cooldown durations (e.g., [0, 5, 10, 30]).
		/// Each time the player watches an ad, the next duration in the array is used.
		/// If the scenario index exceeds the array length, the last value is repeated.
		///
		/// Debug builds can use a separate scenario array (ScenarioDebug) with shorter
		/// durations for faster testing.
		/// </summary>
		/// <param name="product">The shop product view to get the duration for.</param>
		/// <returns>Cooldown duration in minutes.</returns>
		private float GetNewDuration(ShopProductView product)
		{
			var scenarioDuration = _config.NoScenarioDurationMinutes;
			var scenarioIndex = LoadScenarioIndex(product.Config.ID);

			// Cast to the scenario config type which has the duration arrays
			var config = product.Config as ShopProductWithScenarioConfig;

			var scenario = config.Scenario;
			if (GameConstants.IsDebugBuild())
				scenario = config.ScenarioDebug;

			if (scenario.Length > 0)
			{
				if (scenarioIndex < scenario.Length)
					scenarioDuration = scenario[scenarioIndex];
				else
					// If past the end of the array, repeat the last duration
					scenarioDuration = scenario[scenario.Length - 1];
			}
			return scenarioDuration;
		}

		/// <summary>
		/// Called when the app regains focus (e.g., player switches back from another app).
		/// Recalculates all cooldown timers using the real system time, since
		/// Time.deltaTime does not advance while the app is backgrounded.
		/// </summary>
		private void OnApplicationFocus()
		{
			_tempProducts.Clear();
			_tempProducts.AddRange(_productDelayMap.Keys);
			foreach (var product in _tempProducts)
			{
				var delay = GetCurrentDelay(product.Config.ID);
				UpdateProductDelay(product, delay);
			}
		}

		/// <summary>
		/// Updates a product's cooldown display and state. If the cooldown has expired
		/// (delay <= 0), removes the product from the delay map and re-enables it.
		/// Otherwise, displays the remaining time in HH:MM:SS format.
		///
		/// MathUtil.TimeToHMS converts seconds to a formatted time string.
		/// </summary>
		/// <param name="product">The shop product view to update.</param>
		/// <param name="delay">The remaining cooldown time in seconds.</param>
		private void UpdateProductDelay(ShopProductView product, float delay)
		{
			_productDelayMap[product] = delay;

			// Format the countdown or show "FREE" if ready
			var price = string.Format(_priceTimerFormat, _clockIcon, MathUtil.TimeToHMS(delay));
			if (delay <= 0f)
			{
				// Cooldown expired -- remove from tracking and re-enable the product
				_productDelayMap.Remove(product);
				price = _priceAds;
				product.SetInteractable(true);
			}
			product.PriceText.text = price;
		}
	}
}
