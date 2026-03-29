using Game.Level.Area;
using Injection;

namespace Game.Level.Room
{
    public sealed class RoomHiddenState : RoomState
    {
        [Inject] private GameManager _gameManager;

        private AreaController _area;

        public override void Initialize()
        {
            _area = _gameManager.FindArea(_room.Model.Area);

            _room.View.HudView.gameObject.SetActive(false);
            _room.View.MeshesHolder.SetActive(false);

            _room.View.HideWallsPurchasedState();
            _room.View.HideWallsHiddenState();

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
            if (_room.IsPurchasable(_area.Model.IsPurchased, progress, _room.Model.TargetPurchaseValue))
                _room.SwitchToState(new RoomReadyToPurchaseState());
        }

        private void OnLvlChanged(int lvl)
        {
            if (!_area.Model.IsPurchased) return;

            _room.View.UpdateWallsHiddenState(lvl);
        }
    }
}