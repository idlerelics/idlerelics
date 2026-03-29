using System;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Game.Config
{
    /// <summary>
    /// Configuration for a shop product that uses In-App Purchases (IAP).
    /// Inherits from ShopProductConfig (base class for all shop items).
    /// Created via Unity menu: Create > Config > ShopProductIAPConfig.
    ///
    /// ProductType (from Unity IAP) can be:
    /// - Consumable: can be bought multiple times (e.g., coin packs)
    /// - NonConsumable: bought once permanently (e.g., remove ads)
    /// - Subscription: recurring payment
    /// </summary>
    [Serializable]
    [CreateAssetMenu(menuName = "Config/ShopProductIAPConfig")]
    public sealed class ShopProductIAPConfig : ShopProductConfig
    {
        public ProductType Type; // The IAP product type (Consumable, NonConsumable, Subscription)
    }
}
