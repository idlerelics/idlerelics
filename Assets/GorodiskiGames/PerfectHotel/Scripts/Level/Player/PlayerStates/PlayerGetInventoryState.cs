using Game.Level.Item;

namespace Game.Level.Player
{
    public sealed class PlayerGetInventoryState : PlayerItemState
    {
        public PlayerGetInventoryState(ItemController item) : base(item)
        {
            _item = item;
        }

        public override void Initialize()
        {
            _player.View.Throw();
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}

