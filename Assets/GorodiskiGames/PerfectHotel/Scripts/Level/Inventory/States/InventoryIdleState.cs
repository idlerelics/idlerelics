using Injection;
using Game.Core;
using UnityEngine;

namespace Game.Level.Inventory.InventoryStates
{
    /// <summary>
    /// The idle state for an inventory item -- it's just sitting on the player, not moving.
    /// Initialize() and Dispose() are empty because no special logic is needed while idle.
    /// The state machine still requires these methods to be implemented since they're
    /// defined as 'abstract' in the State base class.
    /// </summary>
    public sealed class InventoryIdleState : InventoryState
    {
        public override void Initialize()
        {
        }

        public override void Dispose()
        {
        }
    }
}
