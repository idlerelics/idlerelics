using Game.Config;
using Game.Core;
using Game.Core.UI;
using Game.Domain;
using Game.Managers;
using Injection;
using UnityEngine;

namespace Game.UI.Hud
{
    public sealed class SplashScreenHudMediator : Mediator<SplashScreenHudView>
    {
        [Inject] private GameConfig _config;
        [Inject] private Timer _timer;

        private float _duration;
        private float _elapsed;
        private int _hotel;

        protected override void Show()
        {
            _view.AppVersionText.text = "v" + Application.version;

            _duration = _config.SplashScreenDurationMobile;
#if UNITY_EDITOR
            _duration = _config.SplashScreenDurationEditor;
#endif

            SetIcon();
            UpdateBar();

            _timer.TICK += OnTICK;
        }

        protected override void Hide()
        {
            _timer.TICK -= OnTICK;
        }

        private void OnTICK()
        {
            UpdateBar();

            _elapsed += Time.deltaTime;

            if (_elapsed >= _duration)
                InternalHide();
        }

        private void UpdateBar()
        {
            float value = _elapsed / _duration;
            _view.FillBarImage.fillAmount = value;
        }

        private void SetIcon()
        {
            var model = GameModel.Load(_config);
            foreach (var config in _config.HotelConfigMap.Values)
            {
                if (config.SceneIndex == model.Hotel)
                {
                    _view.Icon.sprite = config.Icon;
                    break;
                }
            }
        }
    }
}