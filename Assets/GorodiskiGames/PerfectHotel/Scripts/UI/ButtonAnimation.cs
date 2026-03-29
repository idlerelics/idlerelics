using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    /// <summary>
    /// Adds a satisfying press/release scale animation to any UI button.
    /// When the player presses down, the button shrinks slightly; when released,
    /// it bounces back to full size with an elastic "overshoot" effect.
    ///
    /// This component implements IPointerDownHandler and IPointerUpHandler,
    /// which are Unity's EventSystem interfaces for detecting touch/click input
    /// on UI elements. Attach this alongside a Button component on any UI object.
    ///
    /// "sealed" means no other class can inherit from ButtonAnimation.
    /// </summary>
    public sealed class ButtonAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        // Scale factor when pressed (0.95 = shrink to 95% of original size)
        private const float _toScaleIn = .95f;

        // Duration in seconds for the press-down animation (very fast at 50ms)
        private const float _durationIn = .05f;

        // Duration in seconds for the release animation (slightly slower at 100ms)
        private const float _durationOut = .1f;

        // Amplitude controls the "bounce" overshoot in the OutBack ease on release
        private const float _amplitude = 1f;

        // Cached reference to the RectTransform (the UI version of Transform)
        private RectTransform _rectTransform;

        // Reference to the Button component, used to check if the button is interactable
        private Button _button;

        /// <summary>
        /// Awake is called once when the script instance is loaded.
        /// Caches the RectTransform and Button references to avoid repeated GetComponent calls.
        ///
        /// "transform as RectTransform" is a C# cast -- all UI objects use RectTransform
        /// instead of regular Transform.
        /// </summary>
        private void Awake()
        {
            _rectTransform = transform as RectTransform;
            _button = GetComponent<Button>();
        }

        /// <summary>
        /// Called by Unity's EventSystem when the player presses down on this UI element.
        /// Only plays the animation if the button exists and is interactable.
        /// </summary>
        /// <param name="eventData">Contains info about the pointer event (position, button, etc.).</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_rectTransform || !_button.interactable)
                return;

            PlayScaleIn();
        }

        /// <summary>
        /// Called when the GameObject is disabled. Kills any running DOTween animations
        /// to prevent errors from tweens trying to update a disabled object.
        ///
        /// DOTween.Kill(this) stops all tweens associated with this component's ID.
        /// </summary>
        private void OnDisable()
        {
            DOTween.Kill(this);
        }

        /// <summary>
        /// Called by Unity's EventSystem when the player lifts their finger/mouse
        /// from this UI element. Plays the bounce-back animation.
        /// </summary>
        /// <param name="eventData">Contains info about the pointer event.</param>
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_rectTransform || !_button.interactable)
                return;

            PlayScaleOut();
        }

        /// <summary>
        /// Animates the button shrinking to 95% scale with a smooth InOutQuad easing.
        /// DOKill stops any previous tween on this transform before starting a new one
        /// to prevent conflicting animations. SetId(this) tags the tween so it can be
        /// killed later by reference.
        /// </summary>
        public void PlayScaleIn()
        {
            _rectTransform.DOKill(this);
            _rectTransform.DOScale(Vector3.one * _toScaleIn, _durationIn).SetEase(Ease.InOutQuad).SetId(this);
        }

        /// <summary>
        /// Animates the button bouncing back to full scale (Vector3.one = 1,1,1).
        /// Uses Ease.OutBack which overshoots slightly before settling, creating a
        /// satisfying "pop" feel. The _amplitude parameter controls how pronounced
        /// the overshoot is.
        /// </summary>
        public void PlayScaleOut()
        {
            _rectTransform.DOKill(this);
            _rectTransform.DOScale(Vector3.one, _durationOut).SetEase(Ease.OutBack, _amplitude).SetId(this);
        }
    }
}
