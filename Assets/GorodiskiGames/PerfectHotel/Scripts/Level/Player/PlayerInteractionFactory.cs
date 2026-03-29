using System;
using System.Collections.Generic;
using Game.Level.Item;

namespace Game.Level.Player
{
    /// <summary>
    /// FIX #5: Replace PlayerIdleState if/else chain with a factory.
    /// Previously, PlayerIdleState had a hardcoded if/else for each ItemType (Clean,
    /// ReceptionDesk, BuyUpdate, ShowHud). Adding a new interaction type meant modifying
    /// PlayerIdleState every time. Now types are registered here via a dictionary, and
    /// PlayerIdleState just calls CreateState() — open for extension, closed for modification.
    ///
    /// Default registrations happen in GamePlayState.Initialize().
    /// To add a new interaction: _interactionFactory.Register(ItemType.X, item => new PlayerXState(item));
    /// </summary>
    public sealed class PlayerInteractionFactory
    {
        private readonly Dictionary<ItemType, Func<ItemController, PlayerState>> _stateCreators
            = new Dictionary<ItemType, Func<ItemController, PlayerState>>();

        /// <summary>
        /// Registers a state creator for a given item type.
        /// Example: Register(ItemType.Clean, item => new PlayerCleaningState(item));
        /// </summary>
        public void Register(ItemType type, Func<ItemController, PlayerState> creator)
        {
            _stateCreators[type] = creator;
        }

        /// <summary>
        /// Returns a new PlayerState for the given item, or null if no handler is registered.
        /// </summary>
        public PlayerState CreateState(ItemController item)
        {
            if (_stateCreators.TryGetValue(item.Type, out var creator))
                return creator(item);
            return null;
        }
    }
}
