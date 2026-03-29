using Game.Level.Area;

namespace Game.Level.Cleaner
{
    public sealed class CleanerHiddenState : CleanerState
    {
        public override void Initialize()
        {
            _cleaner.View.UnitView.gameObject.SetActive(false);
            _cleaner.View.HudView.gameObject.SetActive(false);

            _gameManager.PROGRESS_CHANGED += OnProgressChanged;
            _gameManager.AREA_PURCHASED += OnAreaPurchased;
        }

        public override void Dispose()
        {
            _gameManager.PROGRESS_CHANGED -= OnProgressChanged;
            _gameManager.AREA_PURCHASED -= OnAreaPurchased;
        }

        private void OnProgressChanged(int progress)
        {
            var area = _gameManager.FindArea(_cleaner.Model.Area);
            CheckIsPurchasable(area, progress);
        }

        private void OnAreaPurchased(AreaController area)
        {
            if (area.View.Config.Number != _cleaner.Model.Area) return;
            CheckIsPurchasable(area, _gameManager.Model.LoadProgress());
        }

        private void CheckIsPurchasable(AreaController area, int progress)
        {
            if (_cleaner.IsPurchasable(area.Model.IsPurchased, progress, _cleaner.Model.TargetPurchaseValue))
                _cleaner.SwitchToState(new CleanerReadyToPurchaseState());
        }
    }
}