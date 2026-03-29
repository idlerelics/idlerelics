using System.Collections.Generic;
using Game.Core.UI;
using Game.Level;
using Game.Level.Room;
using Injection;
using UnityEngine;

namespace Game.UI.Hud
{
    public class RoomUpgradeHudMediator : Mediator<RoomUpgradeHudView>
    {
        [Inject] private GameManager _gameManager;
        [Inject] private GameView _gameView;
        [Inject] private LevelView _levelView;

        private RoomController _room;

        protected List<RoomSlotView> _slots;

        public RoomUpgradeHudMediator(RoomController room)
        {
            _room = room;
            _slots = new List<RoomSlotView>();
        }

        protected override void Show()
        {
            _gameView.Joystick.gameObject.SetActive(false);

            InsantiateSlots();

            _view.SetLvl(_room.Model.Lvl);

            _gameView.CameraController.SetTarget(_room.View.transform);
            _gameView.CameraController.ZoomIn(false);

            _view.ON_APP_QUIT += OnApplicationQuit;
        }

        protected override void Hide()
        {
            _view.ON_APP_QUIT -= OnApplicationQuit;

            foreach (var slot in _slots)
            {
                slot.ON_SLOT_CLICK -= OnSlotClick;
            }

            foreach (var slot in _slots)
            {
                GameObject.Destroy(slot.gameObject);
            }
            _slots.Clear();
        }

        private void OnSlotClick(int visualIndex)
        {
            _gameView.Joystick.gameObject.SetActive(true);

            _room.Model.VisualIndex = visualIndex;
            _gameManager.Model.SavePlaceVisualIndex(_room.Model.ID, _room.Model.VisualIndex);
            _room.Model.SetChanged();

            OnCloseButtonClick();
        }

        private void OnCloseButtonClick()
        {
            _gameManager.FireAddGameProgress(_room.View.transform.position, _room.Model.UpdateProgressReward);

            _gameView.CameraController.SetTarget(_gameManager.Player.View.transform);
            _gameView.CameraController.ZoomOut();

            InternalHide();
        }

        private void InsantiateSlots()
        {
            for (int i = 0; i < _room.View.InsideWalls.Icons.Length; i++)
            {
                RoomSlotView slot = GameObject.Instantiate(_view.RoomSlotPrefab, _view.Container).GetComponent<RoomSlotView>();
                slot.Initialize(_room.View.InsideWalls.Icons[i], i);
                _slots.Add(slot);
                slot.ON_SLOT_CLICK += OnSlotClick;
            }
        }

        private void OnApplicationQuit()
        {
            AddRewardProgress();
        }

        private void AddRewardProgress()
        {
            int progress = _gameManager.Model.LoadProgress();
            var progressReward = _room.Model.UpdateProgressReward;

            progress += progressReward;

            _gameManager.Model.SaveProgress(progress);

            CheckIfReachNewLvl(progress);

            _gameManager.Model.Save();
            _gameManager.Model.SetChanged();
        }

        private void CheckIfReachNewLvl(int progress)
        {
            int lvl = _gameManager.Model.LoadLvl();
            if (progress >= _levelView.MaxProgress(lvl))
            {
                lvl++;

                if (lvl <= _levelView.MaxLevels)
                    _gameManager.Model.SaveLvl(lvl);
            }
        }
    }
}