using Game.Level;
using Game.Managers;
using Game.UI;
using Game.UI.Hud;
using Injection;
using UnityEngine;

namespace Game.Modules.UISpritesModule
{
    public sealed class UISpritesModule : Module<UISpritesModuleView>
    {
        [Inject] private GameManager _gameManager;
        [Inject] private GameView _gameView;
        [Inject] private LevelView _levelView;
        [Inject] private HudManager _hudManager;

        public UISpritesModule(UISpritesModuleView view) : base(view)
        {
        }

        public override void Initialize()
        {
            _gameManager.ADD_GAME_PROGRESS += AddGameProgress;
        }

        public override void Dispose()
        {
            _gameManager.ADD_GAME_PROGRESS -= AddGameProgress;
        }

        private void AddGameProgress(Vector3 startPosition, int progressDelta)
        {
            if (progressDelta <= 0)
                return;

            _gameView.Joystick.gameObject.SetActive(false);

            _view.ShowParticles(progressDelta, _gameView.CameraController.Camera.WorldToScreenPoint(startPosition));

            _view.ON_ALL_FINISHED += OnAllFinished;
            _view.ON_ONE_FINISHED += OnOneFinished;
        }

        private void OnOneFinished()
        {
            int progress = _gameManager.Model.LoadProgress();
            progress++;
            _gameManager.Model.SaveProgress(progress);
            _gameManager.FireProgressChanged(progress);

            CheckIfReachNewLvl(progress);

            _gameManager.Model.Save();
            _gameManager.Model.SetChanged();
        }

        private void CheckIfReachNewLvl(int progress)
        {
            if (progress >= _levelView.MaxProgress(_gameManager.Model.LoadLvl()))
            {
                int lvl = _gameManager.Model.LoadLvl();
                var reward = _gameManager.AreasMap[lvl].View.Config.Reward;

                lvl++;

                if (lvl <= _levelView.MaxLevels)
                {
                    _gameManager.Model.SaveLvl(lvl);
                    _gameManager.FireLevelChanged(_gameManager.Model.LoadLvl());

                    _hudManager.ShowAdditional<LevelUpHudMediator>(new object[] { reward });
                }
            }
        }

        private void OnAllFinished(int progressDelta)
        {
            _gameView.Joystick.gameObject.SetActive(true);

            _view.ON_ONE_FINISHED -= OnOneFinished;
            _view.ON_ALL_FINISHED -= OnAllFinished;
        }
    }
}


