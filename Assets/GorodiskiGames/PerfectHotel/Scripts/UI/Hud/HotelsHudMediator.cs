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
    /// <summary>
    /// Mediator for the Hotels selection HUD. Manages the logic behind the
    /// hotel/dorm picker screen where players can switch between different
    /// hotel levels (each hotel is a separate Unity scene).
    ///
    /// Extends Mediator&lt;HotelsHudView&gt;, which follows the Mediator pattern:
    /// the view (HotelsHudView) handles visual elements, while this mediator
    /// handles the business logic (creating slots, handling clicks, switching scenes).
    ///
    /// Dependencies are injected via the custom [Inject] attribute and resolved
    /// by the IoC container at runtime.
    /// </summary>
    public class HotelsHudMediator : Mediator<HotelsHudView>
    {
        // Injected dependencies resolved by the custom DI container
        [Inject] private GameConfig _config;               // Central game configuration
        [Inject] private GameManager _gameManager;         // Manages game state and player data
        [Inject] private GameStateManager _gameStateManager; // State machine for game flow
        [Inject] private HudManager _hudManager;           // Manages showing/hiding HUD panels

        /// <summary>
        /// List of instantiated hotel slot views. Tracked so they can be cleaned up
        /// when the HUD is hidden. "protected" allows subclasses to access this list.
        /// </summary>
        protected List<HotelSlotView> _slots;

        public HotelsHudMediator()
        {
            _slots = new List<HotelSlotView>();
        }

        /// <summary>
        /// Called when the HUD is shown. Creates hotel slot UI elements
        /// and subscribes to the close button click event.
        /// </summary>
        protected override void Show()
        {
            InsantiateSlots();

            _view.CloseButton.onClick.AddListener(OnCloseButtonClick);
        }

        /// <summary>
        /// Called when the HUD is hidden. Unsubscribes all slot click events,
        /// destroys all instantiated slot GameObjects, and clears the list.
        /// Always clean up instantiated objects to prevent memory leaks.
        /// </summary>
        protected override void Hide()
        {
            // First unsubscribe all event handlers
            foreach (var slot in _slots)
            {
                slot.ON_SLOT_CLICK -= OnSlotClick;
            }

            // Then destroy all slot GameObjects
            foreach (var slot in _slots)
            {
                GameObject.Destroy(slot.gameObject);
            }
            _slots.Clear();

            _view.CloseButton.onClick.RemoveListener(OnCloseButtonClick);
        }

        /// <summary>
        /// Creates one hotel slot UI element for each hotel defined in the GameConfig.
        /// Each slot shows the hotel's info and allows the player to select it.
        ///
        /// Checks that the scene index exists in Build Settings before instantiating,
        /// since each hotel corresponds to a Unity scene that must be in the build.
        ///
        /// GameObject.Instantiate creates a copy of a prefab at runtime.
        /// GetComponent retrieves the HotelSlotView script from the instantiated object.
        /// </summary>
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

        /// <summary>
        /// Handles a hotel slot being clicked. Saves the selected hotel scene index
        /// to the game model and transitions to the GameLoadLevelState, which will
        /// unload the current hotel scene and load the selected one.
        ///
        /// SceneManager.sceneCountInBuildSettings returns the total number of scenes
        /// added to File > Build Settings. The scene index must be valid.
        /// </summary>
        /// <param name="hotelSceneIndex">The Build Settings index of the selected hotel scene.</param>
        private void OnSlotClick(int hotelSceneIndex)
        {
            if (hotelSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                InternalHide();

                // Save the selected hotel and reload the level
                _gameManager.Model.Hotel = hotelSceneIndex;
                _gameManager.Model.Save();

                // Switch to the load state, which handles scene transitions
                _gameStateManager.SwitchToState(new GameLoadLevelState());
            }
            else Log.Error("OnSlotClick. Hotel Scene Index Not Exists: " + hotelSceneIndex);
        }

        /// <summary>
        /// Handles the close button click. Pauses the player and hides the HUD.
        /// PlayerPauseState prevents the player from moving while the UI transition occurs.
        /// </summary>
        private void OnCloseButtonClick()
        {
            _gameManager.Player.SwitchToState(new PlayerPauseState());
            InternalHide();
        }
    }
}
