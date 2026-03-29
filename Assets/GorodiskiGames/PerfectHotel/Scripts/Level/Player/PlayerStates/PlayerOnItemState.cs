using Game.Level.Item;

namespace Game.Level.Player
{
    public sealed class PlayerOnItemState : PlayerItemState
    {
        public PlayerOnItemState(ItemController item) : base(item)
        {
            _item = item;
        }

        public override void Initialize()
        {
            _player.View.Idle(_player.Model.Sex, _gameManager.Model.InventoryTypes.Count);

            base.Initialize();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void PlayerOnItem()
        {
            _item.FirePlayerOnItem();
        }

        public override void OnItemFinished()
        {
        }
    }
}

