using Game.Level.Area;

namespace Game.Level.Toilet
{
    public sealed class ToiletHiddenState : ToiletState
    {
        private AreaController _area;

        public override void Initialize()
        {
            _area = _gameManager.FindArea(_toilet.Model.Area);

            _toilet.View.MeshesHolder.SetActive(false);
            _toilet.View.HudView.gameObject.SetActive(false);
            _toilet.View.HideWallsPurchasedState();
            _toilet.View.HideWallsHiddenState();

            OnLvlChanged(_gameManager.Model.LoadLvl());

            _gameManager.PROGRESS_CHANGED += OnProgressChanged;
            _gameManager.AREA_PURCHASED += OnAreaPurchased;
            _gameManager.LEVEL_CHANGED += OnLvlChanged;
        }

        public override void Dispose()
        {
            _gameManager.PROGRESS_CHANGED -= OnProgressChanged;
            _gameManager.AREA_PURCHASED -= OnAreaPurchased;
            _gameManager.LEVEL_CHANGED -= OnLvlChanged;
        }

        private void OnProgressChanged(int progress)
        {
            CheckIsPurchasable(progress);
        }

        private void OnAreaPurchased(AreaController area)
        {
            if (area.View.Config.Number != _area.View.Config.Number) return;

            CheckIsPurchasable(_gameManager.Model.LoadProgress());
            OnLvlChanged(_gameManager.Model.LoadLvl());
        }

        private void CheckIsPurchasable(int progress)
        {
            if (_toilet.IsPurchasable(_area.Model.IsPurchased, progress, _toilet.Model.TargetPurchaseValue))
                _toilet.SwitchToState(new ToiletReadyToPurchaseState());
        }

        private void OnLvlChanged(int lvl)
        {
            if (!_area.Model.IsPurchased) return;
            _toilet.View.UpdateWallsHiddenState(lvl);
        }
    }
}