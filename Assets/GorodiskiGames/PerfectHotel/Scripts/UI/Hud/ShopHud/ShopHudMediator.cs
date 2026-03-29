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
    public sealed class ShopHudMediator : Mediator<ShopHudView>
    {
		private const string _allScenariosDatePrefix = "allScenariosDate";
		private const string _scenarioDatePrefix = "scenarioDate";
		private const string _scenarioIndexPrefix = "scenarioIndex";

		private const string _priceAdsFormat = "{0} {1}";
		private const string _priceTimerFormat = "{0} {1}";
		private const string _adsWord = "FREE";

		[Inject] private IAPManager _IAPManager;
		[Inject] private GameManager _gameManager;
		[Inject] private GameConfig _config;
		[Inject] private AdsManager _adsManager;
		[Inject] private Timer _timer;

		private string _productID;
		private string _priceAds;
		private string _clockIcon;

		private Dictionary<string, ShopProductView> _productMap;
		private Dictionary<ShopProductView, float> _productDelayMap;

		public ShopHudMediator()
		{
			_productMap = new Dictionary<string, ShopProductView>();
			_productDelayMap = new Dictionary<ShopProductView, float>();
		}

		protected override void Show()
		{
			_priceAds = string.Format(_priceAdsFormat, GameConstants.AdsIcon, _adsWord);
			_clockIcon = GameConstants.ClockIcon;

			NoAdsProductVisibility();

			SetProductsForAds();
			SetProductsIAP();

			_IAPManager.ON_INITIALIZED += OnInitialized;
			_IAPManager.ON_PRODUCT_PURCHASED += OnProductPurchased;

			foreach (var product in _view.ProductsIAP)
			{
				product.ON_CLICK += OnProductIAPClick;
			}
			foreach (var product in _view.ProductForAds)
			{
				product.ON_CLICK += OnProductForAdsClick;
			}

			_adsManager.ON_REWARDED_WATCHED += OnRewardedWatched;
			_view.CloseBtn.onClick.AddListener(CloseBtnClick);

			_view.ON_APPLICATION_FOCUS += OnApplicationFocus;
			_timer.TICK += OnTick;
		}

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

		private void OnInitialized()
		{
			_IAPManager.ON_INITIALIZED -= OnInitialized;
			SetProductsIAP();
		}

		private void OnTick()
		{
			foreach (var product in _productDelayMap.Keys.ToList())
			{
				var delay = _productDelayMap[product];
				delay -= Time.deltaTime;
				UpdateProductDelay(product, delay);
			}
		}

		private void SetProductsForAds()
		{
			foreach (var product in _view.ProductForAds)
			{
				product.Initialize(_priceAds);

				TryToAddProduct(product);
				CheckInteractable(product);
			}
		}

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

		private void OnProductIAPClick(string productID)
		{
			_IAPManager.OnPurchaseClicked(productID);
		}

		private void OnProductForAdsClick(string productID)
		{
			_productID = productID;
			_adsManager.ShowRewarded();
		}

		private void OnProductPurchased(string productID)
		{
			var config = _productMap[productID].Config;
			var reward = config.Reward;
			if (reward == ShopProductReward.NoAds)
			{
				_gameManager.Model.IsNoAds = true;
				_gameManager.Model.Save();
				_adsManager.SetNoAds();

				NoAdsProductVisibility();
			}
			else if (reward == ShopProductReward.Cash)
			{
				var newConfig = config as ShopProductWithScenarioConfig;

				_gameManager.Model.Cash += newConfig.Amount;
				_gameManager.Model.Save();
				_gameManager.Model.SetChanged();
			}
		}

		private void CloseBtnClick()
		{
			InternalHide();
		}

		private void NoAdsProductVisibility()
		{
			foreach (var product in _view.ProductsIAP)
			{
				if(product.Config.Reward == ShopProductReward.NoAds && _gameManager.Model.IsNoAds)
					product.gameObject.SetActive(false);
			}
		}

		private void OnRewardedWatched()
		{
			OnProductPurchased(_productID);

			var product = _productMap[_productID];
			var productID = product.Config.ID;

			CheckIsNeedReset(product);

			var scenarioIndex = LoadScenarioIndex(productID);
			scenarioIndex++;
			SaveScenarioIndex(productID, scenarioIndex);

			var duration = GetNewDuration(product);
			var scenarioDate = DateTime.Now.AddMinutes(duration);
			SaveDate(productID, _scenarioDatePrefix, scenarioDate);

			CheckInteractable(product);
		}

		private void TryToAddProduct(ShopProductView product)
		{
			if (!_productMap.Values.Contains(product))
				_productMap.Add(product.Config.ID, product);
		}

		private void CheckInteractable(ShopProductView product)
		{
			var productID = product.Config.ID;

			var currentDelay = GetCurrentDelay(productID);
			bool isInteractable = currentDelay <= 0f;

			if (isInteractable == false)
				_productDelayMap.Add(product, currentDelay);

			product.SetInteractable(isInteractable);
		}

		float GetCurrentDelay(string productID)
		{
			var scenarioDate = LoadDate(productID, _scenarioDatePrefix);
			var result = (float)scenarioDate.Subtract(DateTime.Now).TotalSeconds;
			return result;
		}

		DateTime LoadDate(string productID, string prefix)
		{
			var key = productID + prefix;
			var dateString = PlayerPrefs.GetString(key, DateTime.Now.ToBinary().ToString());
			var dateLong = Convert.ToInt64(dateString);
			return DateTime.FromBinary(dateLong);
		}

		private void CheckIsNeedReset(ShopProductView product)
		{
			var productID = product.Config.ID;
			var allScenariosDate = LoadDate(productID, _allScenariosDatePrefix);
			Log.Info("Product: " + productID + ". all scenarios reset date: " + allScenariosDate);
			var resetDelay = (float)allScenariosDate.Subtract(DateTime.Now).TotalSeconds;
			bool isNeedReset = resetDelay <= 0f;
			if (isNeedReset == true)
			{
				SaveDate(productID, _allScenariosDatePrefix, DateTime.Now.AddHours(_config.AllScenariosDurationHrs));
				SaveDate(productID, _scenarioDatePrefix, DateTime.Now);
				SaveScenarioIndex(productID, 0);

				var allScenariosNewDate = LoadDate(productID, _allScenariosDatePrefix);
				Log.Info("Reseted. New all scenarios reset date " + allScenariosNewDate);
			}
		}

		private void SaveDate(string productID, string prefix, DateTime date)
		{
			var key = productID + prefix;
			PlayerPrefs.SetString(key, date.ToBinary().ToString());
			PlayerPrefs.Save();
		}

		private int LoadScenarioIndex(string productID)
		{
			var key = productID + _scenarioIndexPrefix;
			return PlayerPrefs.GetInt(key, 0);
		}

		private void SaveScenarioIndex(string productID, int index)
		{
			var key = productID + _scenarioIndexPrefix;
			PlayerPrefs.SetInt(key, index);
			PlayerPrefs.Save();
		}

		private float GetNewDuration(ShopProductView product)
		{
			var scenarioDuration = _config.NoScenarioDurationMinutes;
			var scenarioIndex = LoadScenarioIndex(product.Config.ID);

			var config = product.Config as ShopProductWithScenarioConfig;

			var scenario = config.Scenario;
			if (GameConstants.IsDebugBuild())
				scenario = config.ScenarioDebug;

			if (scenario.Length > 0)
			{
				if (scenarioIndex < scenario.Length)
					scenarioDuration = scenario[scenarioIndex];
				else
					scenarioDuration = scenario[scenario.Length - 1];
			}
			return scenarioDuration;
		}

		private void OnApplicationFocus()
		{
			foreach (var product in _productDelayMap.Keys.ToList())
			{
				var delay = GetCurrentDelay(product.Config.ID);
				UpdateProductDelay(product, delay);
			}
		}

		private void UpdateProductDelay(ShopProductView product, float delay)
		{
			_productDelayMap[product] = delay;

			var price = string.Format(_priceTimerFormat, _clockIcon, MathUtil.TimeToHMS(delay));
			if (delay <= 0f)
			{
				_productDelayMap.Remove(product);
				price = _priceAds;
				product.SetInteractable(true);
			}
			product.PriceText.text = price;
		}
	}
}

