using System;
using System.Collections.Generic;
using Game.Config;
using Game.Level;
using Game.Managers;
using Game.UI;
using Injection;
using UnityEngine;
using Game.Level.Player;
using Game.UI.Hud;
using Game.Level.Reception;
using Game.Modules.CashModule;
using Game.Level.Entity;
using Game.Modules.UISpritesModule;
using Game.Modules.UINotificationModule;
using Game.Modules.Utility;
using Game.Modules.ToiletModule;

namespace Game.States
{
    public sealed class GamePlayState : GameState
    {
        [Inject] private Injector _injector;
        [Inject] private Context _context;
        [Inject] private GameView _gameView;
        [Inject] private HudManager _hudManager;
        [Inject] private GameConfig _config;
        [Inject] private LevelView _levelView;
        [Inject] private AdsManager _adsManager;

        private GameManager _gameManager;

        private readonly List<Module> _levelModules;

        public GamePlayState()
        {
            _levelModules = new List<Module>();
        }

        public override void Initialize()
        {
            _gameManager = new GameManager(_config);
            _context.Install(_gameManager);

            var player = _gameManager.Model.Player;
            if (!_config.PlayersMap.ContainsKey(player))
                player = 0;

            var playerConfig = _config.PlayersMap[player];
            var model = new PlayerModel(playerConfig, _config, _gameManager);

            _gameManager.Player = new PlayerController(_gameView.PlayerView, model, _context);
            _gameManager.Player.View.Euler = new Vector3(0f, 180f, 0f);
            _gameManager.Player.View.Position = Vector3.zero;

            _gameView.CameraController.SetPlayer(_gameManager.Player.View.transform);
            _gameView.CameraController.enabled = true;

            InitLevelModules();

            _hudManager.ShowAdditional<GamePlayHudMediator>();
            _hudManager.ShowAdditional<PurchaseHudMediator>();

            _gameView.Joystick.Visibility(_gameManager.Model.JoystickVisibility);
            _gameView.Joystick.gameObject.SetActive(true);

            _gameManager.Player.SwitchToState(new PlayerIdleState());

            _gameManager.ON_TRY_SHOW_INTERSTITIAL += OnTryShowInterstitial;
            _adsManager.ON_REWARDED_WATCHED += OnRewardedWatched;
        }

        public override void Dispose()
        {
            _gameView.CameraController.enabled = false;

            DisposeLevelModules();

            _hudManager.HideAdditional<GamePlayHudMediator>();
            _hudManager.HideAdditional<PurchaseHudMediator>();

            _gameView.Joystick.gameObject.SetActive(false);

            _gameManager.ON_TRY_SHOW_INTERSTITIAL -= OnTryShowInterstitial;
            _adsManager.ON_REWARDED_WATCHED -= OnRewardedWatched;

            _gameManager.Player.Dispose();
            _gameManager.Dispose();

            _context.Uninstall(_gameManager);
        }

        private void InitLevelModules()
        {
            AddModule<ReceptionModule, ReceptionModuleView>(_levelView);
            AddModule<EntityModule, EntityModuleView>(_levelView);
            AddModule<ToiletModule, ToiletModuleView>(_levelView);
            AddModule<UtilityModule, UtilityModuleView>(_levelView);
            AddModule<CashModule, CashModuleView>(_gameView);
            AddModule<UISpritesModule, UISpritesModuleView>(_gameView);
            AddModule<UINotificationModule, UINotificationModuleView>(_gameView);
        }

        private void AddModule<T, T1>(Component component) where T : Module
        {
            var view = component.GetComponent<T1>();
            var result = (T)Activator.CreateInstance(typeof(T), new object[] { view });
            _levelModules.Add(result);
            _injector.Inject(result);
            result.Initialize();
        }

        private void DisposeLevelModules()
        {
            foreach (var levelModule in _levelModules)
            {
                levelModule.Dispose();
            }
            _levelModules.Clear();
        }

        private void OnTryShowInterstitial()
        {
            _adsManager.ShowInterstitial();
        }

        private void OnRewardedWatched()
        {
            _gameManager.Model.SaveWatchAdsTimes();
        }
    }
}