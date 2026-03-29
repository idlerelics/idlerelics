using Game.Config;
using Game.Domain;
using Game.Level;
using Game.Managers;
using Game.UI.Hud;
using Injection;
using UnityEngine.SceneManagement;

namespace Game.States
{
    public class GameLoadLevelState : GameState
    {
        [Inject] protected GameStateManager _gameStateManager;
        [Inject] protected GameConfig _config;
        [Inject] protected Context _context;
        [Inject] protected HudManager _hudManager;

        private int _hotel;

        public override void Initialize()
        {
            _hudManager.ShowAdditional<SplashScreenHudMediator>();

            var model = GameModel.Load(_config);
            _hotel = model.Hotel;

            if (_hotel < 1) _hotel = 1;
            else if (_hotel >= SceneManager.sceneCountInBuildSettings)
            {
                _hotel = SceneManager.sceneCountInBuildSettings - 1;
            }

            model.Hotel = _hotel;
            model.Save();

            SceneManager.sceneLoaded += OnSceneLoaded;

            for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                if (SceneManager.GetSceneByBuildIndex(i).isLoaded)
                {
                    SceneManager.UnloadSceneAsync(i);
                }
            }
            LoadScene();
        }

        public override void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public virtual void OnSceneLoaded(Scene scene, LoadSceneMode arg)
        {
            LevelView level = null;
            var sceneObjects = scene.GetRootGameObjects();
            foreach (var sceneObject in sceneObjects)
            {
                level = sceneObject.GetComponent<LevelView>();

                if (null != level)
                    break;
            }

            _context.Install(level);

            _gameStateManager.SwitchToState<GamePlayState>();
        }

        public virtual void LoadScene()
        {
            SceneManager.LoadScene(_hotel, LoadSceneMode.Additive);
        }
    }
}