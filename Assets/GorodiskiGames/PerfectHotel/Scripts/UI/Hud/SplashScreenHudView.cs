using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.AspectRatioFitter;

namespace Game.UI.Hud
{
    public sealed class SplashScreenHudView : BaseHud
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _fillBarImage;
        [SerializeField] private TMP_Text _appVersionText;
        [SerializeField] private TMP_Text _deviceIDText;
        [SerializeField] private RectTransform _aspectRatioTransform;
        [SerializeField] private AspectRatioFitter _aspectRatio;

        public Image Icon => _icon;
        public Image FillBarImage => _fillBarImage;
        public TMP_Text AppVersionText => _appVersionText;

        protected override void OnEnable()
        {
            SetDeviceID();
            SetAspectMode();
            ResetTransform();
        }

        protected override void OnDisable()
        {
            
        }

        private void SetDeviceID()
        {
            var deviceID = SystemInfo.deviceUniqueIdentifier;
            _deviceIDText.text = deviceID;
        }

        private void SetAspectMode()
        {
            var mode = AspectMode.HeightControlsWidth;

            if (Screen.width > Screen.height)
                mode = AspectMode.WidthControlsHeight;

            _aspectRatio.aspectMode = mode;
        }

        private void ResetTransform()
        {
            _aspectRatioTransform.offsetMin = Vector2.zero;
            _aspectRatioTransform.offsetMax = Vector2.zero;
        }
    }
}