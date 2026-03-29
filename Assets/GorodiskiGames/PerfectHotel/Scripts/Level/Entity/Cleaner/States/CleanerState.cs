using Game.Core;
using Game.Level.Item;
using Game.UI;
using Injection;

namespace Game.Level.Cleaner
{
    public abstract class CleanerState : State
    {
        [Inject] protected CleanerController _cleaner;
        [Inject] protected Timer _timer;
        [Inject] protected GameManager _gameManager;
        [Inject] protected GameView _gameView;

        public override void Dispose()
        {
        }

        public override void Initialize()
        {
        }

        internal void FindUsedItem(ItemController item)
        {
            var targetItem = _gameManager.FindUsedItem(_cleaner.Model.Area, _cleaner.View.TargetItem);
            if (targetItem == null) return;

            _gameManager.RemoveItem(targetItem);
            _cleaner.SwitchToState(new CleanerWalkToItemState(targetItem));
        }
    }
}

