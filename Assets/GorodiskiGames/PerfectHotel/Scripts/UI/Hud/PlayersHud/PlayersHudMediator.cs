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
    public sealed class PlayersHudMediator : Mediator<PlayersHudView>
    {
        private const float _rawCameraHeight = 4000f;

        [Inject] private GameManager _gameManager;
        [Inject] private GameView _gameView;
        [Inject] private GameConfig _config;
        [Inject] private ResourcesManager _resourcesManager;
        [Inject] private HudManager _hudManager;

        private RawCameraView _rawCameraView;
        private Vector3 _positionCached;

        private readonly List<PlayerSlotView> _slots;
        private readonly Dictionary<int, PlayerModel> _models;

        public PlayersHudMediator()
        {
            _slots = new List<PlayerSlotView>();
            _models = new Dictionary<int, PlayerModel>();
        }

        protected override void Show()
        {
            _gameView.Light.SetActive(false);
            _gameView.UILight.SetActive(true);

            _gameManager.FirePlayersHudOpen(true);

            SetViewFromCamera();

            _positionCached = _gameManager.Player.View.Position;

            var position = _rawCameraView.SpawnPlace.position;
            _gameManager.Player.SwitchToState(new PlayerSelectionState(position));

            _gameView.CameraController.SetPosition(position);

            var playerSlotPrefab = _resourcesManager.LoadPlayerSlot();
            var attributeSlotPrefab = _resourcesManager.LoadAttributeSlot();

            var current = _gameManager.Player.Model.Index;

            foreach (var index in _config.PlayersMap.Keys)
            {
                var slot = GameObject.Instantiate(playerSlotPrefab, _view.PlayersScroll.Content).GetComponent<PlayerSlotView>();
                PlayerModel model = null;
                if (index == current)
                {
                    model = _gameManager.Player.Model;
                    model.IsSelected = true;
                    _view.Model = model;

                    OnSlotClick(model);
                }
                else
                {
                    var config = _config.PlayersMap[index];
                    model = new PlayerModel(config, _config, _gameManager);
                }

                foreach (var type in model.Attributes.Keys)
                {
                    var attributeModel = model.Attributes[type];
                    var attributeSlot = GameObject.Instantiate(attributeSlotPrefab, slot.AttributesScroll.Content).GetComponent<AttributeSlotView>();

                    attributeSlot.Model = attributeModel;
                }

                slot.Model = model;

                _slots.Add(slot);
                _models.Add(index, model);

                slot.ON_CLICK += OnSlotClick;
                slot.AttributesScroll.SetContainerSize();
            }

            _view.PlayersScroll.SetContainerSize();

            _view.CloseButton.onClick.AddListener(OnCloseButtonClick);
            _view.SelectButton.onClick.AddListener(OnSelectButtonClick);
        }

        protected override void Hide()
        {
            _gameView.Light.SetActive(true);
            _gameView.UILight.SetActive(false);

            _gameManager.FirePlayersHudOpen(false);

            _gameManager.Player.SwitchToState(new PlayerIdleState());

            _gameView.CameraController.SetPosition(_positionCached);

            _view.CloseButton.onClick.RemoveListener(OnCloseButtonClick);
            _view.SelectButton.onClick.RemoveListener(OnSelectButtonClick);

            foreach (var slot in _slots.ToList())
            {
                slot.Model = null;
                slot.ON_CLICK -= OnSlotClick;
                GameObject.Destroy(slot.gameObject);
            }
            _slots.Clear();
            _models.Clear();
        }

        private void OnSlotClick(PlayerModel model)
        {
            var current = _gameManager.Player.Model.Index;
            var clicked = model.Index;

            if (current != clicked)
            {
                _view.Model = model;
                _gameManager.Player.SetModel(model);
                _gameManager.Player.View.Idle(_gameManager.Player.Model.Sex, 0);
            }

            var saved = _gameManager.Model.Player;
            var isClickedOther = clicked != saved;

            var isUnlocked = _gameManager.Player.Model.UnlockModel.IsUnlocked;

            _view.SelectButton.gameObject.SetActive(isClickedOther && isUnlocked);
        }

        private void OnSelectButtonClick()
        {
            var current = _gameManager.Player.Model.Index;
            var saved = _gameManager.Model.Player;

            if (current != saved)
            {
                _gameManager.Model.Player = current;
                _gameManager.Model.Save();

                _view.SelectButton.gameObject.SetActive(false);

                foreach (var model in _models.Values)
                {
                    var index = model.Index;
                    model.IsSelected = index == current;
                    model.SetChanged();
                }
            }
        }

        private void OnCloseButtonClick()
        {
            var current = _gameManager.Player.Model.Index;
            var saved = _gameManager.Model.Player;

            if (current != saved)
            {
                var model = _models[saved];
                _gameManager.Player.SetModel(model);
            }

            _hudManager.HideAdditional<PlayersHudMediator>();
        }

        private void SetViewFromCamera()
        {
            var prefab = _resourcesManager.LoadRawCameraPrefab();

            _rawCameraView = GameObject.Instantiate(prefab).GetComponent<RawCameraView>();
            _rawCameraView.gameObject.transform.position = Vector3.up * _rawCameraHeight;

            int height = Screen.height;
            int width = height;

            _rawCameraView.Camera.gameObject.SetActive(true);
            RenderTexture renderTexture = new RenderTexture(width, height, 24);
            _rawCameraView.Camera.targetTexture = renderTexture;

            float screenRatio = width / (1f * height);
            _view.RawImage.GetComponent<AspectRatioFitter>().aspectRatio = screenRatio;

            _view.RawImage.gameObject.SetActive(true);
            _view.RawImage.texture = renderTexture;
        }
    }
}

