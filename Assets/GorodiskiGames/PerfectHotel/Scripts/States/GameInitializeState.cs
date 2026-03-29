using Game.Config;
using Game.Domain;
using Game.Managers;
using Injection;

namespace Game.States
{
    public partial class GameInitializeState : GameState
    {
        [Inject] private GameStateManager _gameStateManager;
        [Inject] private Context _context;
        [Inject] private AdsManager _adsManager;
        [Inject] private IAPManager _IAPManager;
        [Inject] private LoginManager _loginManager;

        public override void Initialize()
        {
            var config = GameConfig.Load();
            var model = GameModel.Load(config);

            _context.Install(config);
            _context.ApplyInstall();

#if !UNITY_WEBGL
            _IAPManager.Initialize(config);
#endif
            _loginManager.Initialize(model);
            _adsManager.Initialize(model.IsNoAds, config);

            _gameStateManager.SwitchToState(new GameLoadLevelState());
        }

        public override void Dispose()
        {
        }
    }
}