using Game.Level.Item;
using Game.Level.Player;

namespace Game.Level.Loader.LoaderStates
{
    public class LoaderUpdateState : LoaderState
    {
        public override void Initialize()
        {
            _loader.ItemBuyUpdate.Model.Duration = 1f;
            _loader.View.HudView.ReadyToUpdate();

            CheckIsUpdatable(_gameManager.Model.LoadProgress());

            _loader.ItemBuyUpdate.PLAYER_ON_ITEM += PlayerOnItem;
            _gameManager.PROGRESS_CHANGED += CheckIsUpdatable;
        }

        public override void Dispose()
        {
            _loader.ItemBuyUpdate.PLAYER_ON_ITEM -= PlayerOnItem;
            _gameManager.PROGRESS_CHANGED -= CheckIsUpdatable;
        }

        private void CheckIsUpdatable(int progress)
        {
            bool isUpdatable = _loader.IsUpdatable(_loader.Model.IsMaxed, progress, _loader.Model.TargetUpdateProgress);
            _loader.View.HudView.gameObject.SetActive(isUpdatable);

            if (isUpdatable)
                _gameManager.AddItem(_loader.ItemBuyUpdate);
            else
                _gameManager.RemoveItem(_loader.ItemBuyUpdate);
        }

        private void PlayerOnItem(ItemController item)
        {
            if (_gameManager.Model.Cash <= 0 || !_loader.IsUpdatable(_loader.Model.IsMaxed, _gameManager.Model.LoadProgress(), _loader.Model.TargetUpdateProgress)) return;

            var amount = item.GetAmount(_gameManager.Model.Cash);

            _gameManager.Model.Cash -= amount;
            _gameManager.Model.Save();
            _gameManager.Model.SetChanged();

            _loader.Model.PriceUpdate -= amount;

            _gameManager.FireFlyToRemoveCash(_loader.View.HudView.transform.position);

            if (_loader.Model.PriceUpdate <= 0)
            {
                _loader.ItemBuyUpdate.Model.Duration = 0f;

                _loader.Model.Lvl++;
                _gameManager.Model.SavePlaceLvl(_loader.Model.ID, _loader.Model.Lvl);
                _loader.Model.UpdateModel();

                CheckIsUpdatable(_gameManager.Model.LoadProgress());

                _loader.UnitView.PlayUnitParticles();

                _gameView.CameraController.SetTarget(_loader.UnitView.transform);
                _gameView.CameraController.ZoomIn(true);

                _gameManager.Player.SwitchToState(new PlayerPauseState());

                _gameManager.FireTryShowInterstitial();
            }

            _loader.Model.SetChanged();
        }
    }
}

