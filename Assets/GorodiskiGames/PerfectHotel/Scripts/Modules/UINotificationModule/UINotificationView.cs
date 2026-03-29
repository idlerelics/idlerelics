using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Game.Modules.UINotificationModule
{
    /// <summary>
    /// Types of UI notifications that can appear on screen.
    /// Used to distinguish between different notification contexts.
    /// </summary>
    public enum UINotificationType
    {
        None,
        AreaLocked   // Shown when the player tries to access a locked area/floor
    }

    /// <summary>
    /// A floating UI notification that pops up at a screen position, drifts upward,
    /// and then removes itself. Used to show contextual messages like "Area Locked"
    /// with a required level indicator.
    ///
    /// The animation sequence:
    /// 1. Appears at a screen position with a small scale (pop-in effect)
    /// 2. Scales up to full size with a quick ease
    /// 3. Drifts upward over 2.5 seconds with a decelerating curve
    /// 4. Fires ON_REMOVE when the drift completes so the spawner can recycle it
    ///
    /// "sealed" means no other class can inherit from this one.
    /// </summary>
    public sealed class UINotificationView : MonoBehaviour
    {
        /// <summary>
        /// Event fired when the notification animation finishes and it should be removed.
        /// The spawning system listens to this to return the notification to the object pool.
        /// Action&lt;UINotificationView&gt; is a delegate that passes this view as a parameter.
        /// </summary>
        public Action<UINotificationView> ON_REMOVE;

        // Animation constants
        private const float _duartionMove = 2.5f;          // How long the upward drift takes (seconds)
        private const float _initialScale = 0.5f;          // Starting scale (half size for a pop-in effect)
        private const float _duartionScale = 0.25f;        // How long the scale-up animation takes
        private const float _amplitudeScaleIn = 2f;        // Overshoot amplitude for the scale-in ease

        // TMP_Text is TextMesh Pro's text component -- more feature-rich than Unity's built-in Text.
        [SerializeField] private TMP_Text _text;           // Main message text (e.g., "Area Locked")
        [SerializeField] private TMP_Text _lvlText;        // Level requirement text (e.g., "5")
        [SerializeField] private RectTransform _rectTransform;  // UI transform for positioning

        /// <summary>
        /// Sets up and plays the notification animation.
        /// Places the notification at the given screen position, sets the message and
        /// level text, then starts the scale-up and upward drift animations.
        /// </summary>
        /// <param name="screenPosition">Where on screen to show the notification.</param>
        /// <param name="message">The notification message text to display.</param>
        /// <param name="targetLvl">The level number to display (e.g., required area level).</param>
        public void Initialize(Vector3 screenPosition, string message, int targetLvl)
        {
            _text.text = message.ToString();
            _lvlText.text = targetLvl.ToString();

            // Start at half scale for the pop-in effect
            _rectTransform.localScale = Vector3.one * _initialScale;
            _rectTransform.position = screenPosition;

            // Calculate a random upward drift distance (100-150 pixels) for visual variety
            float height = UnityEngine.Random.Range(100f, 150f);
            float endPositionY = _rectTransform.position.y + height;
            // Drift upward with a decelerating curve, then fire removal when done
            _rectTransform.DOMoveY(endPositionY, _duartionMove).SetEase(Ease.OutQuart).OnComplete(FireRemove);

            // Scale up to full size quickly with slight overshoot for a punchy feel
            _rectTransform.DOScale(Vector3.one, _duartionScale).SetEase(Ease.OutQuad, _amplitudeScaleIn);
        }

        /// <summary>
        /// Called when the upward drift animation completes.
        /// Fires the ON_REMOVE event so the spawning system can recycle or destroy
        /// this notification. The "?." operator ensures no error if nobody is listening.
        /// </summary>
        private void FireRemove()
        {
            ON_REMOVE?.Invoke(this);
        }
    }
}
