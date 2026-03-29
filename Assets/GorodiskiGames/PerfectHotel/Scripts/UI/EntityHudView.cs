using Game;
using Game.Level.Place;
using Game.UI.Hud;
using TMPro;
using UnityEngine;
using Utilities;

namespace Game.Level.Entity
{
    /// <summary>
    /// Displays a floating HUD panel above an entity (room, elevator, reception, etc.)
    /// showing its current state: locked, ready to purchase, or ready to upgrade.
    ///
    /// Inherits from BaseHudWithModel, which means it automatically reacts to changes
    /// in the EntityModel via the Observer pattern -- whenever the model changes,
    /// OnModelChanged is called and the display updates.
    ///
    /// The three visual states are:
    /// - Locked: shows a lock icon with the required level
    /// - Ready to Purchase: shows a buy icon with the purchase price
    /// - Ready to Update: shows an upgrade icon with the upgrade price
    /// </summary>
    public class EntityHudView : BaseHudWithModel<EntityModel>
    {
        // Icons toggled to represent the entity's current state
        [SerializeField] private GameObject _lockedIcon;
        [SerializeField] private GameObject _purchaseIcon;
        [SerializeField] private GameObject _updateIcon;

        // Text fields for the entity name/info and its price
        [SerializeField] private TMP_Text _infoText;
        [SerializeField] private TMP_Text _priceText;

        // Cached strings to avoid creating new string objects every frame
        private string _info;
        private string _price;

        /// <summary>
        /// Intentionally empty -- this HUD does not subscribe to any events on enable.
        /// Required by the abstract BaseHud class.
        /// </summary>
        protected override void OnEnable()
        {
        }

        /// <summary>
        /// Intentionally empty -- this HUD does not unsubscribe from any events on disable.
        /// Required by the abstract BaseHud class.
        /// </summary>
        protected override void OnDisable()
        {
        }

        /// <summary>
        /// Sets the HUD to the "locked" visual state.
        /// Shows only the lock icon, hides purchase and update icons.
        /// </summary>
        public void Locked()
        {
            _lockedIcon.SetActive(true);
            _purchaseIcon.SetActive(false);
            _updateIcon.SetActive(false);
        }

        /// <summary>
        /// Sets the HUD to the "ready to purchase" visual state.
        /// Shows only the purchase icon, hides locked and update icons.
        /// </summary>
        public void ReadyToPuchase()
        {
            _lockedIcon.SetActive(false);
            _purchaseIcon.SetActive(true);
            _updateIcon.SetActive(false);
        }

        /// <summary>
        /// Sets the HUD to the "ready to upgrade" visual state.
        /// Shows only the update icon, hides locked and purchase icons.
        /// </summary>
        public void ReadyToUpdate()
        {
            _lockedIcon.SetActive(false);
            _purchaseIcon.SetActive(false);
            _updateIcon.SetActive(true);
        }

        /// <summary>
        /// Called when a new model is assigned. Calls the base implementation.
        /// Can be extended to perform setup when the entity model is first applied.
        /// </summary>
        /// <param name="model">The entity model being applied to this view.</param>
        protected override void OnApplyModel(EntityModel model)
        {
            base.OnApplyModel(model);
        }

        /// <summary>
        /// Called whenever the EntityModel changes (via the Observer pattern).
        /// Updates the info text and price text based on the entity's type and state.
        ///
        /// Logic flow:
        /// 1. Default: show entity type name and purchase price
        /// 2. For Area/Elevator entities: show level requirement if locked, or "NEW AREA"/"NEW HOTEL"
        /// 3. If already purchased: show upgrade price and next level number
        /// 4. Special labels for Reception ("RECEPTIONIST") and Cleaner ("SPEED")
        ///
        /// MathUtil.NiceCash formats large numbers into readable strings (e.g., "1.2K").
        /// GameConstants.CashIcon is a TextMeshPro sprite tag that renders an inline icon.
        /// </summary>
        /// <param name="model">The updated entity model.</param>
        protected override void OnModelChanged(EntityModel model)
        {
            _info = model.PurchaseLabel;
            _price = GameConstants.CashIcon + " " + MathUtil.NiceCash(model.PricePurchase);

            if (model.IsLocked)
            {
                _info = "LEVEL " + model.TargetPurchaseValue;
                _price = "";
            }

            if (model.IsPurchased)
            {
                _price = GameConstants.CashIcon + " " + MathUtil.NiceCash(model.PriceUpdate);
                _info = model.UpgradeLabel ?? "LVL " + (model.LvlNext + 1);
            }

            _infoText.text = _info;
            _priceText.text = _price;
        }
    }
}
