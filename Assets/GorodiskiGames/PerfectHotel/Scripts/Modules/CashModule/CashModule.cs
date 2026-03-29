using System.Collections.Generic;
using Game.Level.Cash;
using Game.Level;
using Injection;
using UnityEngine;
using Game.Core;
using Game.Config;
using Game.Level.Item;
using Game.Level.Entity;

namespace Game.Modules.CashModule
{
    /// <summary>
    /// Manages ALL cash (money) in the game: creating cash piles at rooms/reception,
    /// animating cash flying to piles or to the player, and collecting cash when
    /// the player walks near a cash pile.
    ///
    /// This is a "Module" -- a high-level system that coordinates many smaller objects.
    /// [Inject] is a Dependency Injection attribute: the DI framework automatically
    /// fills in these fields with the correct instances, so you don't have to pass
    /// them manually through constructors.
    /// </summary>
    public sealed class CashModule : Module<CashModuleView>
    {
        private const float _heightAbovePile = 3f;            // How high above a pile new cash spawns
        private const float _heightAbovePlayer = 1.5f;        // How high above the player cash spawns when spending
        private const float _cashFlyToRemoveRate = 0.1f;      // Minimum time between "fly to remove" animations (throttle)
        private const float _checkPlayerOnItemRate = 0.1f;    // How often to check if the player is near a cash pile (seconds)

        // [Inject] fields are automatically filled by the dependency injection system.
        [Inject] private GameManager _gameManager;  // Central game manager with access to all game systems
        [Inject] private Context _context;           // DI context for creating new objects
        [Inject] private Timer _timer;               // Game timer that fires TICK events each frame
        [Inject] private GameConfig _config;         // Game configuration data (set in Unity Editor)


        // Maps each cash pile's view to its controller (lookup table).
        private readonly Dictionary<CashPileView, CashPileController> _cashPilesMap;
        // Maps each item (room/reception) to its associated cash pile view.
        private readonly Dictionary<ItemController, CashPileView> _itemsMap;
        // Temporary list of cash objects that are currently flying (not yet in a pile).
        private List<CashController> _tempCashes;

        private float _cashFlyToRemoveTimer;   // Timer to throttle "fly to remove" animations
        private float _checkPlayerOnItemTime;  // Next time to check if player is near a cash pile
        private float _cashPileRadius;         // How close the player must be to collect cash

        /// <summary>Constructor: initializes the lookup dictionaries. ": base(view)" calls the parent constructor.</summary>
        public CashModule(CashModuleView view) : base(view)
        {
            _cashPilesMap = new Dictionary<CashPileView, CashPileController>();
            _itemsMap = new Dictionary<ItemController, CashPileView>();
            _tempCashes = new List<CashController>();
        }

        /// <summary>
        /// Sets up cash piles for the reception, all rooms, and all toilets.
        /// Also subscribes to events using "+=" (event subscription).
        /// When _timer fires TICK, OnTick() will be called. When _gameManager fires
        /// FLY_TO_REMOVE_CASH, CashFlyToRemove() will be called.
        /// </summary>
        public override void Initialize()
        {
            _cashPileRadius = _config.CashPileRadius;

            // Create a cash pile for the reception desk
            AddCashPile(_gameManager.Reception.View.CashPileView, _gameManager.Reception.ItemCashPile, _gameManager.Reception.Model);

            // Create cash piles for each room
            foreach (var room in _gameManager.Rooms)
            {
                AddCashPile(room.View.CashPileView, room.ItemCashPile, room.Model);
            }

            // Create cash piles for each toilet
            foreach (var toilet in _gameManager.Toilets)
            {
                AddCashPile(toilet.View.CashPileView, toilet.ItemCashPile, toilet.Model);
            }

            // Subscribe to events: "+=" means "start listening for this event"
            _gameManager.FLY_TO_REMOVE_CASH += CashFlyToRemove;
            _timer.TICK += OnTick;
        }

        /// <summary>
        /// Cleans up when this module is destroyed.
        /// "-=" unsubscribes from events (stops listening). Always unsubscribe to avoid memory leaks!
        /// </summary>
        public override void Dispose()
        {
            _gameManager.FLY_TO_REMOVE_CASH -= CashFlyToRemove;
            _timer.TICK -= OnTick;

            foreach (var cashPile in _cashPilesMap.Values)
            {
                cashPile.View.CASH_FLY_TO_PILE -= CashFlyToPile;
                cashPile.View.CASH_FLY_TO_PLAYER -= CashFlyToPlayer;

                foreach (var cash in cashPile.View.Cashes)
                {
                    cash.REMOVE_CASH -= OnRemoveCash;
                    cash.Dispose();
                }
                cashPile.View.Cashes.Clear();
            }

            foreach (var cash in _tempCashes)
            {
                cash.REMOVE_CASH -= OnRemoveCash;
                cash.Dispose();
            }
            _tempCashes.Clear();

            _view.ReleaseAllInstances();
        }

        /// <summary>
        /// Called every frame by the Timer. Periodically checks if the player
        /// is close enough to any cash pile to collect it.
        /// Uses a rate limiter so it doesn't check every single frame (for performance).
        /// </summary>
        private void OnTick()
        {
            // Only check at intervals, not every frame (Time.time = seconds since game start)
            if (Time.time >= _checkPlayerOnItemTime)
            {
                _checkPlayerOnItemTime = Time.time + _checkPlayerOnItemRate;

                // Check each cash pile to see if the player is within collection range
                foreach (var item in _itemsMap.Keys)
                {
                    float sqrDistance = (item.Transform.position - _gameManager.Player.View.Position).sqrMagnitude;
                    if (sqrDistance < _cashPileRadius * _cashPileRadius)
                    {
                        PlayerOnItem(item); // Player is close enough -- collect the cash!
                    }
                }
            }
        }

        /// <summary>Registers a new cash pile: subscribes to its events and adds it to the tracking maps.</summary>
        private void AddCashPile(CashPileView view, ItemController itemCashPile, EntityModel model)
        {
            view.CASH_FLY_TO_PILE += CashFlyToPile;
            view.CASH_FLY_TO_PLAYER += CashFlyToPlayer;

            var pile = new CashPileController(view, model);

            _cashPilesMap[view] = pile;
            _itemsMap[itemCashPile] = view;

            _gameManager.AddItem(itemCashPile);
        }

        /// <summary>Creates a new cash object at the given position (from the object pool).</summary>
        private CashController Cash(Vector3 position)
        {
            var cashView = _view.Get<CashView>();
            var cash = new CashController(cashView, position, _context);
            return cash;
        }

        /// <summary>Spawns a new cash object above the pile and animates it flying down to the pile.</summary>
        private void CashFlyToPile(CashPileView view, Vector3 endPosition)
        {
            CashController cash = Cash(view.transform.position + (Vector3.up * _heightAbovePile));
            cash.FlyToPile(endPosition);
            view.Cashes.Add(cash);
            cash.REMOVE_CASH += OnRemoveCash;
        }

        /// <summary>Takes a cash object from a pile and makes it fly toward the player.</summary>
        private void CashFlyToPlayer(CashPileView cashPileView, int index)
        {
            var cash = cashPileView.Cashes[index];
            cash.FlyToPlayer();
            cashPileView.Cashes.Remove(cash);
            _tempCashes.Add(cash);
        }

        /// <summary>Spawns cash above the player and animates it flying to a target (e.g., for spending).</summary>
        private void CashFlyToRemove(Vector3 endPosition)
        {
            _cashFlyToRemoveTimer += Time.deltaTime;
            if (_cashFlyToRemoveTimer < _cashFlyToRemoveRate) return;

            _cashFlyToRemoveTimer = 0f;

            CashController cash = Cash(_gameManager.Player.View.transform.position + (Vector3.up * _heightAbovePlayer));
            cash.FlyToRemove(endPosition);
            cash.REMOVE_CASH += OnRemoveCash;
        }

        /// <summary>Called when a cash object finishes its animation and should be cleaned up.</summary>
        private void OnRemoveCash(CashController cash)
        {
            cash.REMOVE_CASH -= OnRemoveCash;
            _view.Release(cash.View);
            cash.Dispose();
            _tempCashes.Remove(cash);
        }

        /// <summary>
        /// Called when the player is close enough to a cash pile to collect it.
        /// Transfers all cash from the pile to the player's total, saves the data,
        /// and notifies the UI to update (via SetChanged, which is part of the Observer pattern).
        /// </summary>
        private void PlayerOnItem(ItemController item)
        {
            var cashPileView = _itemsMap[item];
            var cashPile = _cashPilesMap[cashPileView];

            if (cashPile.Model.Cash <= 0) return; // Nothing to collect

            var amount = cashPile.Model.Cash;
            cashPile.Model.Cash -= amount;                                          // Empty the pile
            _gameManager.Model.SavePlaceCash(cashPile.Model.ID, cashPile.Model.Cash); // Save pile state to disk
            cashPile.Model.SetChanged();                                             // Notify UI the pile changed

            _gameManager.Model.Cash += amount;   // Add cash to the player's total
            _gameManager.Model.Save();           // Save player's total to disk
            _gameManager.Model.SetChanged();     // Notify UI the player's cash changed
        }
    }
}