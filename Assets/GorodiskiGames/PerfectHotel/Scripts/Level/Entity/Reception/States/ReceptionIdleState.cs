using Game.Level.Item;
using Game.Level.Player;
using Game.UI;
using Injection;

namespace Game.Level.Reception
{
    public sealed class ReceptionIdleState : ReceptionState
    {
        [Inject] private GameManager _gameManager;
        [Inject] private GameView _gameView;

        public override void Initialize()
        {
            CheckIsUpdatable(_gameManager.Model.LoadProgress());

            _reception.ItemBuyUpdate.PLAYER_ON_ITEM += PlayerOnItem;
            _gameManager.PROGRESS_CHANGED += CheckIsUpdatable;
        }

        public override void Dispose()
        {
            _reception.ItemBuyUpdate.PLAYER_ON_ITEM -= PlayerOnItem;
            _gameManager.PROGRESS_CHANGED -= CheckIsUpdatable;
        }

        private void CheckIsUpdatable(int progress)
        {
            bool isUpdatable = _reception.IsUpdatable(_reception.Model.IsMaxed, progress, _reception.Model.TargetUpdateProgress);
            _reception.View.HudView.gameObject.SetActive(isUpdatable);

            if (isUpdatable)
            {
                _reception.ItemBuyUpdate.Model.Duration = 1f;
                _gameManager.AddItem(_reception.ItemBuyUpdate);
                _reception.View.HudView.ReadyToUpdate();
            }
            else _gameManager.RemoveItem(_reception.ItemBuyUpdate);
        }

        void PlayerOnItem(ItemController item)
        {
            if (_gameManager.Model.Cash <= 0 || !_reception.IsUpdatable(_reception.Model.IsMaxed, _gameManager.Model.LoadProgress(), _reception.Model.TargetUpdateProgress)) return;

            var amount = item.GetAmount(_gameManager.Model.Cash);

            _gameManager.Model.Cash -= amount;
            _gameManager.Model.Save();
            _gameManager.Model.SetChanged();

            _reception.Model.PriceUpdate -= amount;

            _gameManager.FireFlyToRemoveCash(_reception.View.HudView.transform.position);

            if (_reception.Model.PriceUpdate <= 0)
            {
                _reception.ItemBuyUpdate.Model.Duration = 0f;

                _reception.Model.Lvl++;
                _gameManager.Model.SavePlaceLvl(_reception.Model.ID, _reception.Model.Lvl);
                _reception.Model.UpdateModel();

                CheckIsUpdatable(_gameManager.Model.LoadProgress());

                _gameView.CameraController.SetTarget(_reception.View.transform);
                _gameView.CameraController.ZoomIn(true);

                _gameManager.Player.SwitchToState(new PlayerPauseState());
            }

            _reception.Model.SetChanged();
        }
    }
}


