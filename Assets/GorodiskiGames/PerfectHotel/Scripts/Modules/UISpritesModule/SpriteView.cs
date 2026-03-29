using System;
using DG.Tweening;
using UnityEngine;
using Utilities;

namespace Game.UI.Hud
{
    /// <summary>
    /// A UI sprite that flies from a world position to a UI target (e.g., a progress bar).
    /// Used by UISpritesModule to visually represent earned progress points.
    ///
    /// The animation has two phases:
    /// 1. Intro: the sprite pops out (scales up with an overshoot ease) and moves to an
    ///    intermediate position, creating a "burst" effect
    /// 2. Move: the sprite flies smoothly to its final UI destination and scales back to normal
    ///
    /// "sealed" means no other class can inherit from this one.
    /// </summary>
    public sealed class SpriteView : MonoBehaviour
    {
        // Animation timing constants
        private const float _introDuration = 0.75f;       // Duration of the intro pop-out phase (seconds)
        private const float _moveDuration = 1f;            // Duration of the fly-to-target phase (seconds)
        private const float _scaleDurationKoef = 0.5f;     // Scale animation is this fraction of the move duration
        private const float _maxScale = 2.5f;              // Maximum scale during the intro pop-out

        /// <summary>
        /// Event fired when the sprite finishes its full animation (intro + move).
        /// The UISpritesModuleView listens to this to track when each sprite arrives.
        /// </summary>
        public event Action<SpriteView> ON_MOVE_COMPLETE;

        // RectTransform is the UI-specific Transform used for Canvas-based elements.
        // It adds anchoring, pivots, and sizing on top of regular Transform.
        [SerializeField] private RectTransform _rectTransform;

        /// <summary>The RectTransform for this sprite, used for positioning in screen space.</summary>
        public RectTransform Transform => _rectTransform;

        private Vector3 _endPosition;   // Final UI destination where the sprite flies to

        /// <summary>
        /// Starts the two-phase animation:
        /// Phase 1 (Intro): moves to an intermediate "burst" position while scaling up.
        /// DOTween's SetEase(Ease.OutSine) creates a smooth deceleration.
        /// Ease.OutBack adds an overshoot "bounce" effect on the scale-up.
        /// </summary>
        /// <param name="endIntroPosition">Intermediate position for the pop-out burst effect.</param>
        /// <param name="endPosition">Final UI destination (e.g., the progress bar icon).</param>
        public void DoIntroAnimation(Vector3 endIntroPosition, Vector3 endPosition)
        {
            _endPosition = endPosition;

            // Move to the intermediate position with a smooth deceleration curve
            _rectTransform.DOMove(endIntroPosition, _introDuration).SetEase(Ease.OutSine).OnComplete(DoMoveAnimation);
            // Scale up with an overshoot bounce, completing in half the intro time
            _rectTransform.DOScale(Vector3.one * _maxScale, _introDuration * _scaleDurationKoef).SetEase(Ease.OutBack);
        }

        /// <summary>
        /// Phase 2 (Move): flies the sprite from its current position to the final UI target.
        /// DOTween's Ease.OutCubic gives a fast start that decelerates smoothly.
        /// The sprite also scales back down to normal size during the flight.
        /// </summary>
        private void DoMoveAnimation()
        {
            // Fly to the final position with cubic deceleration
            _rectTransform.DOMove(_endPosition, _moveDuration).SetEase(Ease.OutCubic).OnComplete(OnMoveEnd);
            // Scale back to normal size with a circular ease curve
            _rectTransform.DOScale(Vector3.one, _moveDuration * _scaleDurationKoef).SetEase(Ease.OutCirc);
        }

        /// <summary>
        /// Called when the move animation completes. Fires the ON_MOVE_COMPLETE event
        /// using SafeInvoke (a utility extension that checks for null before invoking,
        /// preventing NullReferenceException if no one is listening).
        /// </summary>
        /// <summary>
        /// FIX #1: Kill DOTween animations when pooled objects are recycled.
        /// This view has up to 4 concurrent tweens (intro move + scale, then move + scale).
        /// When returned to the pool via SetActive(false), DOKill stops them all, preventing
        /// orphaned tweens from running on deactivated GameObjects.
        /// </summary>
        private void OnDisable()
        {
            _rectTransform.DOKill();
        }

        private void OnMoveEnd()
        {
            ON_MOVE_COMPLETE.SafeInvoke(this);
        }
    }
}
