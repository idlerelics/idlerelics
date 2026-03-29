using System.Collections.Generic;
using Game.Controls;
using Game.Level.Player;
using Game.UI.Hud;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public sealed class GameView : MonoBehaviour
    {
        public PlayerView PlayerView;
        public BaseHud[] Huds;
        public Joystick Joystick;
        public CameraController CameraController;
        public RectTransform HudBG;
        public GameObject Light;
        public GameObject UILight;

        public IEnumerable<IHud> AllHuds()
        {
            foreach (var hud in Huds)
            {
                yield return hud;
            }
        }

        private void Awake()
        {
            SetHudBG();
        }

        private void SetHudBG()
        {
            bool isDeveloperIPad = GameConstants.IsDeveloperIPad();
            if (isDeveloperIPad)
            {
                var aspect = HudBG.GetComponent<AspectRatioFitter>();

                if (Screen.height < Screen.width)
                {
                    aspect.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
                    aspect.aspectRatio = 1.77777f;
                }
                else
                {
                    aspect.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
                    aspect.aspectRatio = 0.5625f;
                }
            }
            else
            {
                Screen.orientation = ScreenOrientation.Portrait;
            }
        }
    }
}