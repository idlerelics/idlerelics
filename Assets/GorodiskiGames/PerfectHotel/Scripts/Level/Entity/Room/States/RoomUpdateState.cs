using Core;
using Game.Level.Item;
using Game.Level.Player;
using Game.Managers;
using Game.UI.Hud;
using Injection;

namespace Game.Level.Room
{
    public class RoomUpdateState : RoomState, IObserver
    {
        [Inject] protected GameManager _gameManager;
        [Inject] protected HudManager _hudManager;

        protected int _visualIndex = 0;

        public override void Initialize()
        {
            _room.ItemBuyUpdate.Model.Duration = 1f;
            _room.View.HideWallsHiddenState();

            _room.View.HudView.ReadyToUpdate();
            _room.View.MeshesHolder.SetActive(true);

            SetInsideVisual(_room.Model.VisualIndex);
            OnLvlChanged(_gameManager.Model.LoadLvl());

            CheckIsUpdatable(_gameManager.Model.LoadProgress());

            _visualIndex = _room.Model.VisualIndex;
            _room.Model.AddObserver(this);

            _room.ItemBuyUpdate.PLAYER_ON_ITEM += PlayerOnItem;
            _gameManager.PROGRESS_CHANGED += CheckIsUpdatable;
            _gameManager.LEVEL_CHANGED += OnLvlChanged;
        }

        public override void Dispose()
        {
            _room.Model.RemoveObserver(this);

            _room.ItemBuyUpdate.PLAYER_ON_ITEM -= PlayerOnItem;
            _gameManager.PROGRESS_CHANGED -= CheckIsUpdatable;
            _gameManager.LEVEL_CHANGED -= OnLvlChanged;
        }

        private void CheckIsUpdatable(int progress)
        {
            bool isUpdatable = _room.IsUpdatable(_room.Model.IsMaxed, progress, _room.Model.TargetUpdateProgress);
            _room.View.HudView.gameObject.SetActive(isUpdatable);

            if (isUpdatable)
                _gameManager.AddItem(_room.ItemBuyUpdate);
            else
                _gameManager.RemoveItem(_room.ItemBuyUpdate);
        }

        private void OnLvlChanged(int lvl)
        {
            _room.View.OutsideWalls.MeshesVisibilityLvl(lvl);
            _room.View.UpdateWallsPurchasedState(lvl);
        }

        private void SetInsideVisual(int visualIndex)
        {
            _room.View.InsideWalls.MeshesVisibilityIndex(visualIndex);
        }

        private void PlayerOnItem(ItemController item)
        {
            if (_gameManager.Model.Cash <= 0 || !_room.IsUpdatable(_room.Model.IsMaxed, _gameManager.Model.LoadProgress(), _room.Model.TargetUpdateProgress)) return;

            var amount = item.GetAmount(_gameManager.Model.Cash);

            _gameManager.Model.Cash -= amount;
            _gameManager.Model.Save();
            _gameManager.Model.SetChanged();

            _room.Model.PriceUpdate -= amount;

            _gameManager.FireFlyToRemoveCash(_room.View.HudView.transform.position);

            if (_room.Model.PriceUpdate <= 0)
            {
                _room.ItemBuyUpdate.Model.Duration = 0f;

                _room.Model.Lvl++;
                _gameManager.Model.SavePlaceLvl(_room.Model.ID, _room.Model.Lvl);
                _room.Model.UpdateModel();

                CheckIsUpdatable(_gameManager.Model.LoadProgress());

                _hudManager.ShowAdditional<RoomUpgradeHudMediator>(new object[] { _room} );
                _gameManager.Player.SwitchToState(new PlayerPauseState());

                _gameManager.FireTryShowInterstitial();
            }

            _room.Model.SetChanged();
        }

        public void OnObjectChanged(Observable observable)
        {
            if (_visualIndex == _room.Model.VisualIndex)
                return;

            _visualIndex = _room.Model.VisualIndex;

            UpdateRoomVisual(_visualIndex);
        }

        public virtual void UpdateRoomVisual(int visualIndex)
        {
            SetInsideVisual(visualIndex);
        }
    }
}

