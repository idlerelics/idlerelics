using System;
using System.Collections.Generic;
using Game.Level.Item;

namespace Game.Level.Player
{
    /// <summary>
    /// Maps ItemTypes to the PlayerState that should activate when the player
    /// interacts with that item. New interaction types can be registered without
    /// modifying PlayerIdleState.
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
