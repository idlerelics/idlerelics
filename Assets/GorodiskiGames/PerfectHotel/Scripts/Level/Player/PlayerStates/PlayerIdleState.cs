using Game.Level.Item;
using Injection;

namespace Game.Level.Player
{
    /// <summary>
    /// The player's idle state -- active when the player is standing still.
    /// Inherits from PlayerFindEntityState, which handles camera angle adjustments
    /// when the player is near entities.
    ///
    /// While idle, the player:
    /// 1. Checks if any interactive items (cleaning spots, reception, purchases, elevator) are nearby
    /// 2. Listens for joystick input to transition to the walk state
    /// 3. Listens for new items being added to the game to re-check proximity
    /// </summary>
    public sealed class PlayerIdleState : PlayerFindEntityState
    {
        [Inject] private PlayerInteractionFactory _interactionFactory;

        /// <summary>
        /// Called when this state becomes active.
        /// Sets up the idle animation, subscribes to events, and checks for nearby items.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize(); // Call parent to subscribe to ONE_SECOND_TICK for entity detection

            // Disable NavMeshAgent so the player doesn't try to pathfind while idle
            _player.View.NavMeshAgent.enabled = false;
            // Play the idle animation (different animations based on sex and whether carrying items)
            _player.View.Idle(_player.Model.Sex, _gameManager.Model.InventoryTypes.Count);

            // Notify listeners that the player is now idle
            _player.FireIdle();

            // Subscribe to events:
            // ITEM_ADDED fires when a new interactive item appears in the world
            _gameManager.ITEM_ADDED += OnItemAdded;
            // TICK fires every frame -- used to check for joystick input
            _timer.TICK += OnTICK;

            // Immediately check if there's an item the player is already standing on
            FindClosestUsedItem();
        }

        /// <summary>When a new item is added to the game, re-check if it's close enough to interact with.</summary>
        private void OnItemAdded(ItemController item)
        {
            FindClosestUsedItem();
        }

        /// <summary>
        /// Called when this state is deactivated (player leaves idle).
        /// Unsubscribes from all events to prevent memory leaks.
        /// Always unsubscribe (-=) anything you subscribe (+=) to!
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            _gameManager.ITEM_ADDED -= OnItemAdded;
            _timer.TICK -= OnTICK;
        }

        /// <summary>
        /// Checks if the player is standing on an interactive item and transitions
        /// to the appropriate state based on the item type.
        /// 'internal' means this method is accessible within the same assembly (project)
        /// but not from external assemblies.
        /// </summary>
        internal void FindClosestUsedItem()
        {
            var item = _gameManager.FindClosestUsedItem();
            if (item == null) return;

            var state = _interactionFactory.CreateState(item);
            if (state != null)
                _player.SwitchToState(state);
        }

        /// <summary>
        /// Called every frame via the Timer's TICK event.
        /// Checks if the joystick has input -- if so, transition to walk state.
        /// Also re-checks for nearby items so the player doesn't have to
        /// stop at the exact pixel to trigger cleaning.
        /// </summary>
        private void OnTICK()
        {
            if (_gameView.Joystick.HasInput)
            {
                _player.SwitchToState(new PlayerWalkState());
                return;
            }

            FindClosestUsedItem();
        }
    }
}
