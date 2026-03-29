using Game.Level.Item;

namespace Game.Level.Player
{
    public sealed class PlayerCleaningState : PlayerItemState
    {
        public PlayerCleaningState(ItemController item) : base(item)
        {
            _item = item;
        }

        public override void Initialize()
        {
            _player.View.Clean();

            base.Initialize();
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}

