using System.Collections.Generic;
using System.Linq;
using Game.Config;
using Game.Core.UI;
using Game.Level.Player;
using Game.Managers;
using Injection;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    /// <summary>
    /// Mediator for the Player Selection HUD where players can browse, preview,
    /// and select different character models. Uses a render-to-texture camera
    /// to display a 3D character preview in the UI.
    ///
    /// This is one of the more complex mediators because it:
    /// 1. Spawns a secondary camera for 3D character previews (render-to-texture)
    /// 2. Manages character slot creation with attribute sub-slots
    /// 3. Handles character switching, selection, and persistence
    /// 4. Toggles scene lighting for proper UI rendering
    ///
    /// "sealed" means no other class can inherit from PlayersHudMediator.
    /// </summary>
    public sealed class PlayersHudMediator : Mediator<PlayersHudView>
    {
        // Y-position offset for the render-to-texture camera setup.
        // Placed high (4000 units up) to avoid overlapping with the game world.
        private const float _rawCameraHeight = 4000f;

        // Injected dependencies resolved by the custom DI container
        [Inject] private GameManager _gameManager;             // Player data and game state
        [Inject] private GameView _gameView;                   // Root UI view with camera/lighting access
        [Inject] private GameConfig _config;                   // Player character configurations
        [Inject] private ResourcesManager _resourcesManager;   // Loads prefabs from Resources folder
        [Inject] private HudManager _hudManager;               // Manages HUD visibility

        // The render-to-texture camera view instance (spawned at runtime)
        private RawCameraView _rawCameraView;

        // Cached player position to restore when closing the HUD
        private Vector3 _positionCached;

        // Tracking collections for cleanup
        private readonly List<PlayerSlotView> _slots;           // All instantiated player slot views
        private readonly Dictionary<int, PlayerModel> _models;  // Player models keyed by index

        public PlayersHudMediator()
        {
            _slots = new List<PlayerSlotView>();
            _models = new Dictionary<int, PlayerModel>();
        }

        /// <summary>
        /// Called when the Player Selection HUD is shown. Performs extensive setup:
        /// 1. Switches lighting from game light to UI light for proper rendering
        /// 2. Sets up the render-to-texture camera for 3D character preview
        /// 3. Moves the player character to the preview spawn position
        /// 4. Creates a scrollable list of player slots, each with attribute sub-slots
        /// 5. Subscribes to close and select button events
        ///
        /// The render-to-texture technique renders a 3D scene to a texture,
        /// which is then displayed on a RawImage in the UI Canvas.
        /// </summary>
        protected override void Show()
        {
            // Switch to UI lighting for clean character rendering
            _gameView.Light.SetActive(false);
            _gameView.UILight.SetActive(true);

            // Notify the game that the players HUD is open (may pause gameplay)
            _gameManager.FirePlayersHudOpen(true);

            // Set up the render-to-texture camera
            SetViewFromCamera();

            // Cache the player's current position so we can restore it when closing
            _positionCached = _gameManager.Player.View.Position;

            // Move the player to the camera's spawn position for previewing
            var position = _rawCameraView.SpawnPlace.position;
            _gameManager.Player.SwitchToState(new PlayerSelectionState(position));

            // Position the main camera to look at the preview area
            _gameView.CameraController.SetPosition(position);

            // Load prefabs for the player slot and attribute slot UI elements
            var playerSlotPrefab = _resourcesManager.LoadPlayerSlot();
            var attributeSlotPrefab = _resourcesManager.LoadAttributeSlot();

            var current = _gameManager.Player.Model.Index;

            // Create a slot for each available player character
            foreach (var index in _config.PlayersMap.Keys)
            {
                var slot = GameObject.Instantiate(playerSlotPrefab, _view.PlayersScroll.Content).GetComponent<PlayerSlotView>();
                PlayerModel model = null;
                if (index == current)
                {
                    // Use the existing model for the currently active player
                    model = _gameManager.Player.Model;
                    model.IsSelected = true;
                    _view.Model = model;

                    // Trigger the click handler to set up the initial preview
                    OnSlotClick(model);
                }
                else
                {
                    // Create a new model for characters not currently in use
                    var config = _config.PlayersMap[index];
                    model = new PlayerModel(config, _config, _gameManager);
                }

                // Create attribute sub-slots (speed, capacity, etc.) for each character
                foreach (var type in model.Attributes.Keys)
                {
                    var attributeModel = model.Attributes[type];
                    var attributeSlot = GameObject.Instantiate(attributeSlotPrefab, slot.AttributesScroll.Content).GetComponent<AttributeSlotView>();

                    // Bind the attribute model to the slot view via the Observer pattern
                    attributeSlot.Model = attributeModel;
                }

                // Bind the player model to the slot view
                slot.Model = model;

                _slots.Add(slot);
                _models.Add(index, model);

                slot.ON_CLICK += OnSlotClick;
                slot.AttributesScroll.SetContainerSize();
            }

            // Resize the scroll container to fit all player slots
            _view.PlayersScroll.SetContainerSize();

            // Subscribe to button events
            _view.CloseButton.onClick.AddListener(OnCloseButtonClick);
            _view.SelectButton.onClick.AddListener(OnSelectButtonClick);
        }

        /// <summary>
        /// Called when the HUD is hidden. Restores game lighting, player position,
        /// and cleans up all instantiated UI elements and event subscriptions.
        ///
        /// Uses .ToList() when iterating _slots because destroying objects during
        /// iteration can cause collection-modified exceptions.
        /// </summary>
        protected override void Hide()
        {
            // Restore normal game lighting
            _gameView.Light.SetActive(true);
            _gameView.UILight.SetActive(false);

            // Notify the game that the players HUD is closed
            _gameManager.FirePlayersHudOpen(false);

            // Return the player to idle state
            _gameManager.Player.SwitchToState(new PlayerIdleState());

            // Restore the camera to the player's original position
            _gameView.CameraController.SetPosition(_positionCached);

            // Unsubscribe button events
            _view.CloseButton.onClick.RemoveListener(OnCloseButtonClick);
            _view.SelectButton.onClick.RemoveListener(OnSelectButtonClick);

            // Destroy all instantiated slot objects and clean up
            foreach (var slot in _slots.ToList())
            {
                slot.Model = null;  // Unbind the model (removes observer)
                slot.ON_CLICK -= OnSlotClick;
                GameObject.Destroy(slot.gameObject);
            }
            _slots.Clear();
            _models.Clear();
        }

        /// <summary>
        /// Handles a player slot being clicked. If a different character is clicked,
        /// switches the player's model and updates the 3D preview.
        /// Shows the "Select" button only if the clicked character is different
        /// from the currently saved one AND is unlocked.
        /// </summary>
        /// <param name="model">The PlayerModel associated with the clicked slot.</param>
        private void OnSlotClick(PlayerModel model)
        {
            var current = _gameManager.Player.Model.Index;
            var clicked = model.Index;

            if (current != clicked)
            {
                // Switch to the clicked character for preview
                _view.Model = model;
                _gameManager.Player.SetModel(model);
                _gameManager.Player.View.Idle(_gameManager.Player.Model.Sex, 0);
            }

            // Only show "Select" if this is a different character than the saved one
            var saved = _gameManager.Model.Player;
            var isClickedOther = clicked != saved;

            // Also check if the character is unlocked before allowing selection
            var isUnlocked = _gameManager.Player.Model.UnlockModel.IsUnlocked;

            _view.SelectButton.gameObject.SetActive(isClickedOther && isUnlocked);
        }

        /// <summary>
        /// Handles the "Select" button click. Saves the currently previewed character
        /// as the active player, persists the choice, and updates the selected
        /// state on all slot models so the UI reflects the new selection.
        ///
        /// SetChanged() notifies all observers to refresh their visuals.
        /// </summary>
        private void OnSelectButtonClick()
        {
            var current = _gameManager.Player.Model.Index;
            var saved = _gameManager.Model.Player;

            if (current != saved)
            {
                // Persist the player selection
                _gameManager.Model.Player = current;
                _gameManager.Model.Save();

                // Hide the select button since this character is now the active one
                _view.SelectButton.gameObject.SetActive(false);

                // Update all models to reflect the new selection state
                foreach (var model in _models.Values)
                {
                    var index = model.Index;
                    model.IsSelected = index == current;
                    model.SetChanged();  // Notify observers to update visuals
                }
            }
        }

        /// <summary>
        /// Handles the close button click. If the player was previewing a different
        /// character, reverts back to the saved character before closing.
        /// Uses HudManager to hide this HUD panel.
        /// </summary>
        private void OnCloseButtonClick()
        {
            var current = _gameManager.Player.Model.Index;
            var saved = _gameManager.Model.Player;

            // Revert to the saved character if the player was previewing a different one
            if (current != saved)
            {
                var model = _models[saved];
                _gameManager.Player.SetModel(model);
            }

            _hudManager.HideAdditional<PlayersHudMediator>();
        }

        /// <summary>
        /// Sets up a render-to-texture camera for the 3D character preview.
        ///
        /// How render-to-texture works:
        /// 1. A secondary camera is placed high above the game world (Y=4000) to avoid overlap
        /// 2. A RenderTexture is created as the camera's output target
        /// 3. The RenderTexture is assigned to a RawImage in the UI Canvas
        /// 4. The camera renders the 3D character model to the texture each frame
        /// 5. The UI displays the rendered texture, creating a "3D preview in 2D UI" effect
        ///
        /// RenderTexture(width, height, 24) creates a texture with 24-bit depth buffer.
        /// AspectRatioFitter ensures the preview maintains the correct proportions.
        /// </summary>
        private void SetViewFromCamera()
        {
            // Load and instantiate the camera prefab
            var prefab = _resourcesManager.LoadRawCameraPrefab();

            _rawCameraView = GameObject.Instantiate(prefab).GetComponent<RawCameraView>();
            // Place the camera high up so it doesn't see the main game world
            _rawCameraView.gameObject.transform.position = Vector3.up * _rawCameraHeight;

            // Create a square render texture matching the screen height
            int height = Screen.height;
            int width = height;

            _rawCameraView.Camera.gameObject.SetActive(true);
            RenderTexture renderTexture = new RenderTexture(width, height, 24);
            _rawCameraView.Camera.targetTexture = renderTexture;

            // Set the aspect ratio fitter to match the render texture proportions
            float screenRatio = width / (1f * height);
            _view.RawImage.GetComponent<AspectRatioFitter>().aspectRatio = screenRatio;

            // Display the render texture on the UI RawImage
            _view.RawImage.gameObject.SetActive(true);
            _view.RawImage.texture = renderTexture;
        }
    }
}
