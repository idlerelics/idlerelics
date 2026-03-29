using System;
using System.Collections.Generic;
using Game.Config;
using Game.Core.UI;
using Game.Level.Player;
using Game.Managers;
using Game.States;
using Injection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.UI.Hud
{
    public class HotelsHudMediator : Mediator<HotelsHudView>
    {
        [Inject] private GameConfig _config;
        [Inject] private GameManager _gameManager;
        [Inject] private GameStateManager _gameStateManager;
        [Inject] private HudManager _hudManager;

        protected List<HotelSlotView> _slots;

        public HotelsHudMediator()
        {
            _slots = new List<HotelSlotView>();
        }

        protected override void Show()
        {
            InsantiateSlots();

            _view.CloseButton.onClick.AddListener(OnCloseButtonClick);
        }

        protected override void Hide()
        {
            foreach (var slot in _slots)
            {
                slot.ON_SLOT_CLICK -= OnSlotClick;
            }

            foreach (var slot in _slots)
            {
                GameObject.Destroy(slot.gameObject);
            }
            _slots.Clear();

            _view.CloseButton.onClick.RemoveListener(OnCloseButtonClick);
        }

        private void InsantiateSlots()
        {
            foreach (var sceneIndex in _config.HotelConfigMap.Keys)
            {
                if (sceneIndex < SceneManager.sceneCountInBuildSettings)
                {
                    HotelSlotView slot = GameObject.Instantiate(_view.HotelSlotPrefab, _view.Container).GetComponent<HotelSlotView>();
                    var config = _config.HotelConfigMap[sceneIndex];
                    slot.Initialize(config, _gameManager.Model.Hotel);
                    _slots.Add(slot);
                    slot.ON_SLOT_CLICK += OnSlotClick;
                }
                else Log.Error("Error. Hotel Scene Index " + sceneIndex + " not found in Build Settings");
            }
        }

        private void OnSlotClick(int hotelSceneIndex)
        {
            if (hotelSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                InternalHide();

                _gameManager.Model.Hotel = hotelSceneIndex;
                _gameManager.Model.Save();

                _gameStateManager.SwitchToState(new GameLoadLevelState());
            }
            else Log.Error("OnSlotClick. Hotel Scene Index Not Exists: " + hotelSceneIndex);
        }

        private void OnCloseButtonClick()
        {
            _gameManager.Player.SwitchToState(new PlayerPauseState());
            InternalHide();
        }
    }
}


