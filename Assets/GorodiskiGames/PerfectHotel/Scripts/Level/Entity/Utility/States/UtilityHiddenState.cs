using Game.Level.Area;

namespace Game.Level.Utility.UtilityStates
{
    public sealed class UtilityHiddenState : UtilityState
    {
        public override void Initialize()
        {
            _utility.View.MeshesHolder.SetActive(false);

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
            var area = _gameManager.FindArea(_utility.Model.Area);
            CheckIsPurchasable(area, progress);
        }

        private void OnAreaPurchased(AreaController area)
        {
            if (area.View.Config.Number != _utility.Model.Area) return;
            CheckIsPurchasable(area, _gameManager.Model.LoadProgress());
        }

        private void CheckIsPurchasable(AreaController area, int progress)
        {
            if (_utility.IsPurchasable(area.Model.IsPurchased, progress, _utility.Model.TargetPurchaseValue))
                _utility.SwitchToState(new UtilityPurchasedState());
        }

        private void OnLvlChanged(int lvl)
        {
            _utility.View.UpdateWallsHiddenState(lvl);
        }
    }
}