using Game.Level.Area;

namespace Game.Level.Loader.LoaderStates
{
    public sealed class LoaderHiddenState : LoaderState
    {
        public override void Initialize()
        {
            _loader.View.UnitView.gameObject.SetActive(false);
            _loader.View.HudView.gameObject.SetActive(false);

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
            var area = _gameManager.FindArea(_loader.Model.Area);
            CheckIsPurchasable(area, progress);
        }

        private void OnAreaPurchased(AreaController area)
        {
            if (area.View.Config.Number != _loader.Model.Area) return;
            CheckIsPurchasable(area, _gameManager.Model.LoadProgress());
        }

        private void CheckIsPurchasable(AreaController area, int progress)
        {
            if (_loader.IsPurchasable(area.Model.IsPurchased, progress, _loader.Model.TargetPurchaseValue))
                _loader.ReadyToPurchase();
        }
    }
}
