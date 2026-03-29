using Game.Level;
using Game.UI;
using Injection;
using UnityEngine;

namespace Game.Modules.UINotificationModule
{
    public sealed class UINotificationModule : Module<UINotificationModuleView>
    {
        private const string _areaUnlockMessage01 = "Reach Level";
        private const string _areaUnlockMessage02 = "To Unlock";

        private const float _areaLockedOffset = 1.5f;

        [Inject] private GameManager _gameManager;
        [Inject] private GameView _gameView;

        public UINotificationModule(UINotificationModuleView view) : base(view)
        {
        }

        public override void Initialize()
        {
            _gameManager.ON_NOTIFICATION_NEED_LVL += OnAreaLockedNotification;
        }
        public override void Dispose()
        {
            _gameManager.ON_NOTIFICATION_NEED_LVL += OnAreaLockedNotification;

            _view.UINotification.ReleaseAllInstances();
        }

        void OnAreaLockedNotification(Vector3 itemPosition, int lvl)
        {
            Vector3 screenPosition = _gameView.CameraController.Camera.WorldToScreenPoint(itemPosition + new Vector3(0f, _areaLockedOffset, 0f));
            string message = _areaUnlockMessage01 + " " + lvl + " " + _areaUnlockMessage02;

            var view = _view.UINotification.Get<UINotificationView>();
            view.Initialize(screenPosition, message, lvl);
            view.ON_REMOVE += RemoveNotificationPopup;
        }

        private void RemoveNotificationPopup(UINotificationView view)
        {
            view.ON_REMOVE -= RemoveNotificationPopup;
            _view.UINotification.Release(view);
        }
    }
}


