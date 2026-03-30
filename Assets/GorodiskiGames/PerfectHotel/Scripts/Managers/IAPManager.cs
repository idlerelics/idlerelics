using System;
using System.Threading.Tasks;
using Game.Config;
using Game.Core;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Game.Managers
{
    public sealed class Receipt
    {
        public string Store;
        public string TransactionID;
        public string Payload;

        public Receipt()
        {
            Store = TransactionID = Payload = "";
        }

        public Receipt(string store, string transactionID, string payload)
        {
            Store = store;
            TransactionID = transactionID;
            Payload = payload;
        }
    }

    public sealed class PayloadAndroid
    {
        public string json;
        public string signature;

        public PayloadAndroid()
        {
            json = signature = "";
        }

        public PayloadAndroid(string _json, string _signature)
        {
            json = _json;
            signature = _signature;
        }
    }

    public sealed class IAPManager : IDetailedStoreListener
    {
        public event Action ON_INITIALIZED;
        public event Action ON_PURCHASE_CLICKED;
        public event Action<string> ON_PURCHASE_FAILED;
        public event Action ON_PURCHASE_PROCESS_COMPLETE;
        public event Action ON_RESTORE_PURCHASES;
        public event Action<string> ON_RESTORE_PURCHASES_END;
        public event Action<string> ON_PRODUCT_PURCHASED;

        private const string kEnvironment = "production";

        private IStoreController controller;
        private IExtensionProvider extension;

        async public void Initialize(GameConfig config)
        {
            try
            {
                var options = new InitializationOptions().SetEnvironmentName(kEnvironment);
                await UnityServices.InitializeAsync(options);
            }
            catch (Exception exception)
            {
                Log.Info(exception.Message);
            }

            InitializePurchasing(config);
        }

        public string GetPrice(string productID)
        {
            var result = "";
            if (IsPurchaseInitialized())
            {
                Product product = controller.products.WithID(productID);
                if (product != null && product.availableToPurchase)
                    result = controller.products.WithID(productID).metadata.localizedPriceString;
            }
            return result;
        }

        public string GetTitle(string productID)
        {
            return controller.products.WithID(productID).metadata.localizedTitle;
        }

        public void OnPurchaseClicked(string productId)
        {
            if (IsPurchaseInitialized())
            {
                Product product = controller.products.WithID(productId);

                if (product != null && product.availableToPurchase)
                {
                    ON_PURCHASE_CLICKED?.Invoke();

                    Log.Info(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                    controller.InitiatePurchase(product);
                }
                else
                {
                    Log.Info("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            else
            {
                Log.Info("BuyProductID FAIL. Not initialized.");
            }
        }

        public void RestorePurchases()
        {
            if (!IsPurchaseInitialized())
            {
                Log.Info("RestorePurchases FAIL. Not initialized.");
                return;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Log.Info("RestorePurchases started ...");
                ON_RESTORE_PURCHASES?.Invoke();

                var apple = extension.GetExtension<IAppleExtensions>();

                apple.RestoreTransactions((result, info) =>
                {
                    ON_RESTORE_PURCHASES_END?.Invoke(info);
                    Log.Info("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
                });
            }
            else
            {
                Log.Info("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            }
        }

        private bool IsPurchaseInitialized()
        {
            return controller != null && extension != null;
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Log.Info("IAP. Initialize success!");

            this.controller = controller;
            this.extension = extensions;

            ON_INITIALIZED?.Invoke();
        }

        private void InitializePurchasing(GameConfig config)
        {
            if (IsPurchaseInitialized())
            {
                return;
            }

            var purchasing = StandardPurchasingModule.Instance();
            var builder = ConfigurationBuilder.Instance(purchasing);

#if UNITY_EDITOR
            purchasing.useFakeStoreAlways = true;
            purchasing.useFakeStoreUIMode = FakeStoreUIMode.Default;
#endif

            foreach (var product in config.ShopProductIAPMap.Values)
            {
                builder.AddProduct(product.ID, product.Type);
            }

            UnityPurchasing.Initialize(this, builder);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var id = args.purchasedProduct.definition.id;
            ON_PRODUCT_PURCHASED?.Invoke(id);

            Log.Info("OnProductPurchased. ProductID: " + id);

            ON_PURCHASE_PROCESS_COMPLETE?.Invoke();

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            string info = $"Purchase {product.metadata.localizedTitle} Failed. Reason: {failureReason}";
            ON_PURCHASE_FAILED.Invoke(info);
            Log.Info(info);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            string info = $"Purchase {product.metadata.localizedTitle} Failed. Reason: {failureDescription}";
            ON_PURCHASE_FAILED.Invoke(info);
            Log.Info(info);
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Log.Info("Initialize failed due to: " + error);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Log.Info("Initialize failed due to: " + error);
        }

        public Product GetMetaDataById(string id)
        {
            foreach (var product in controller.products.all)
            {
                if (product.definition.id == id)
                    return product;
            }

            return null;
        }
    }
}


