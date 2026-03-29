using Game.Level.Item;
using Game.Level.Player;

namespace Game.Level.Cleaner
{
    public class CleanerUpdateState : CleanerState
    {
        public override void Initialize()
        {
            _cleaner.ItemBuyUpdate.Model.Duration = 1f;
            _cleaner.View.HudView.ReadyToUpdate();

            CheckIsUpdatable(_gameManager.Model.LoadProgress());

            _cleaner.ItemBuyUpdate.PLAYER_ON_ITEM += PlayerOnItem;
            _gameManager.PROGRESS_CHANGED += CheckIsUpdatable;
        }

        public override void Dispose()
        {
            _cleaner.ItemBuyUpdate.PLAYER_ON_ITEM -= PlayerOnItem;
            _gameManager.PROGRESS_CHANGED -= CheckIsUpdatable;
        }

        private void CheckIsUpdatable(int progress)
        {
            bool isUpdatable = _cleaner.IsUpdatable(_cleaner.Model.IsMaxed, progress, _cleaner.Model.TargetUpdateProgress);
            _cleaner.View.HudView.gameObject.SetActive(isUpdatable);

            if (isUpdatable)
                _gameManager.AddItem(_cleaner.ItemBuyUpdate);
            else
                _gameManager.RemoveItem(_cleaner.ItemBuyUpdate);
        }

        private void PlayerOnItem(ItemController item)
        {
            if (_gameManager.Model.Cash <= 0 || !_cleaner.IsUpdatable(_cleaner.Model.IsMaxed, _gameManager.Model.LoadProgress(), _cleaner.Model.TargetUpdateProgress))
                return;

            var amount = item.GetAmount(_gameManager.Model.Cash);

            _gameManager.Model.Cash -= amount;
            _gameManager.Model.Save();
            _gameManager.Model.SetChanged();

            _cleaner.Model.PriceUpdate -= amount;

            _gameManager.FireFlyToRemoveCash(_cleaner.View.HudView.transform.position);

            if (_cleaner.Model.PriceUpdate <= 0)
            {
                _cleaner.ItemBuyUpdate.Model.Duration = 0f;

                _cleaner.Model.Lvl++;
                _gameManager.Model.SavePlaceLvl(_cleaner.Model.ID, _cleaner.Model.Lvl);
                _cleaner.Model.UpdateModel();

                CheckIsUpdatable(_gameManager.Model.LoadProgress());

                _cleaner.UnitView.PlayUnitParticles();

                _gameView.CameraController.SetTarget(_cleaner.UnitView.transform);
                _gameView.CameraController.ZoomIn(true);

                _gameManager.Player.SwitchToState(new PlayerPauseState());

                _gameManager.FireTryShowInterstitial();
            }

            _cleaner.Model.SetChanged();
        }
    }
}

