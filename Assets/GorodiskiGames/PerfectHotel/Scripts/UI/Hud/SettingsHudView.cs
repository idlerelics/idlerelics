using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class SettingsHudView : BaseHud
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _developerButton;
        [SerializeField] private Button _restorePurchasesButton;
        [SerializeField] private Toggle _joystickVisibilityToggle;

        public Button CloseButton => _closeButton;
        public Button DeveloperButton => _developerButton;
        public Button RestorePurchasesButton => _restorePurchasesButton;

        public Toggle JoystickVisibilityToggle => _joystickVisibilityToggle;

        protected override void OnEnable()
        {
        }

        protected override void OnDisable()
        {
        }
    }
}