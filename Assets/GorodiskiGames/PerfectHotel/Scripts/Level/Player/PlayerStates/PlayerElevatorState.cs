using Game.Level.Item;
using Game.Managers;
using Game.UI.Hud;
using Injection;

namespace Game.Level.Player
{
    /// <summary>
    /// State for when the player is at the elevator (used to switch between hotel floors/scenes).
    /// Opens the hotels HUD so the player can pick which floor to visit.
    ///
    /// [Inject] tells the DI container to automatically provide the HudManager instance.
    /// </summary>
    public sealed class PlayerElevatorState : PlayerItemState
    {
        [Inject] private HudManager _hudManager;

        public PlayerElevatorState(ItemController item) : base(item)
        {
            _item = item;
        }

        /// <summary>
        /// Opens the hotels/floors selection HUD and plays the idle animation.
        /// ShowAdditional means this HUD appears on top of the main gameplay HUD.
        /// </summary>
        public override void Initialize()
        {
            _hudManager.ShowAdditional<HotelsHudMediator>();

            _player.View.Idle(_player.Model.Sex, _gameManager.Model.InventoryTypes.Count);

            base.Initialize();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        /// <summary>
        /// Empty override -- the elevator item never "finishes" like a timed task.
        /// The player leaves by walking away or selecting a floor.
        /// </summary>
        public override void OnItemFinished()
        {
        }
    }
}
