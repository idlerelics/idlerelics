using System.Collections.Generic;
using Game.Controls;
using Game.Level.Player;
using Game.UI.Hud;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// The root UI view for the entire game scene. Holds references to all major
    /// visual components: the player, all HUD panels, the joystick input,
    /// camera controller, and lighting objects.
    ///
    /// This MonoBehaviour lives on the top-level UI Canvas and acts as a central
    /// hub that other systems (managers, mediators) can access to find UI elements.
    ///
    /// "sealed" means no other class can inherit from GameView.
    /// </summary>
    public sealed class GameView : MonoBehaviour
    {
        // Public fields are assigned in the Unity Inspector.
        // Unlike [SerializeField] private fields, public fields are directly accessible
        // from other scripts without needing properties.

        /// <summary>Reference to the player character's view component in the scene.</summary>
        public PlayerView PlayerView;

        /// <summary>Array of all HUD panels (splash screen, gameplay, shop, etc.).</summary>
        public BaseHud[] Huds;

        /// <summary>The on-screen joystick used for player movement on mobile.</summary>
        public Joystick Joystick;

        /// <summary>Controls camera position, zoom, and follow behavior.</summary>
        public CameraController CameraController;

        /// <summary>Background RectTransform for the HUD area, adjusted for aspect ratio.</summary>
        public RectTransform HudBG;

        /// <summary>The main 3D scene light -- toggled off when UI overlays are shown.</summary>
        public GameObject Light;

        /// <summary>A separate light used for UI-only rendering (e.g., player selection screen).</summary>
        public GameObject UILight;

        /// <summary>
        /// Returns all HUD panels as IHud interfaces using an iterator (yield return).
        /// This allows other systems to iterate over HUDs without knowing their concrete types.
        ///
        /// "yield return" creates a lazy iterator -- values are produced one at a time
        /// as the caller iterates, rather than building a whole list in memory.
        /// </summary>
        public IEnumerable<IHud> AllHuds()
        {
            foreach (var hud in Huds)
            {
                yield return hud;
            }
        }

        /// <summary>
        /// Awake is called once when the script instance is loaded.
        /// Configures the HUD background aspect ratio for the current device.
        /// </summary>
        private void Awake()
        {
            SetHudBG();
        }

        /// <summary>
        /// Adjusts the HUD background to handle different screen aspect ratios.
        /// On developer iPad builds (landscape-capable), it uses an AspectRatioFitter
        /// to maintain a 16:9 ratio regardless of orientation.
        /// On normal mobile builds, it locks the screen to portrait orientation.
        ///
        /// 1.77777 is 16/9 (landscape), 0.5625 is 9/16 (portrait).
        /// </summary>
        private void SetHudBG()
        {
            bool isDeveloperIPad = GameConstants.IsDeveloperIPad();
            if (isDeveloperIPad)
            {
                // Get the AspectRatioFitter component to enforce a specific aspect ratio
                var aspect = HudBG.GetComponent<AspectRatioFitter>();

                if (Screen.height < Screen.width)
                {
                    // Landscape mode: width drives the height calculation
                    aspect.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
                    aspect.aspectRatio = 1.77777f;
                }
                else
                {
                    // Portrait mode: height drives the width calculation
                    aspect.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
                    aspect.aspectRatio = 0.5625f;
                }
            }
            else
            {
                // On regular mobile devices, force portrait orientation
                Screen.orientation = ScreenOrientation.Portrait;
            }
        }
    }
}
