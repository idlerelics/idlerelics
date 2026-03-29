using Game.Config;
using Game.Domain;
using Game.Managers;
using Injection;

namespace Game.States
{
    /// <summary>
    /// The first state the game enters after startup.
    /// Its job is to load configuration and saved data, set up services,
    /// and then move on to loading the level.
    ///
    /// "partial" means this class can be split across multiple files.
    /// This is handy when a class gets large or is auto-generated in part.
    ///
    /// It inherits from GameState, which provides the Initialize()/Dispose() pattern
    /// that every game state must follow (this is called the STATE PATTERN).
    /// </summary>
    public partial class GameInitializeState : GameState
    {
        // [Inject] is an ATTRIBUTE -- a tag you put on a field to give it
        // special meaning. Here, the dependency injection system sees [Inject]
        // and automatically fills in these fields with the matching objects
        // from the Context. You never have to manually assign them!
        [Inject] private GameStateManager _gameStateManager;
        [Inject] private Context _context;
        [Inject] private AdsManager _adsManager;
        [Inject] private IAPManager _IAPManager;
        [Inject] private LoginManager _loginManager;

        /// <summary>
        /// Called when this state becomes active.
        /// Loads game config (settings) and the player's saved data (model),
        /// initializes services, then transitions to the level-loading state.
        /// </summary>
        public override void Initialize()
        {
            // Load the game configuration (things like default cash, level settings, etc.)
            var config = GameConfig.Load();
            // Load (or create) the player's saved progress
            var model = GameModel.Load(config);

            // Register the config in the context so other systems can access it
            _context.Install(config);
            _context.ApplyInstall();

            // #if !UNITY_WEBGL is a PREPROCESSOR DIRECTIVE.
            // Code inside this block is only compiled when NOT building for WebGL.
            // WebGL (browser games) doesn't support in-app purchases, so we skip it.
#if !UNITY_WEBGL
            _IAPManager.Initialize(config);
#endif
            // Set up the login manager (tracks daily logins / rewards)
            _loginManager.Initialize(model);
            // Set up the ads manager (knows whether ads are disabled and the ad config)
            _adsManager.Initialize(model.IsNoAds, config);

            // Move to the next state: loading the level/scene.
            // "new GameLoadLevelState()" creates a fresh instance of that state.
            _gameStateManager.SwitchToState(new GameLoadLevelState());
        }

        /// <summary>
        /// Called when leaving this state. Nothing to clean up here,
        /// but the method must exist because GameState requires it
        /// (it is abstract -- meaning subclasses MUST implement it).
        /// "override" means we are providing our own version of a method
        /// that was declared in the parent class (GameState).
        /// </summary>
        public override void Dispose()
        {
        }
    }
}
