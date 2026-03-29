using System;
using DG.Tweening;
using UnityEngine;
using Utilities;

namespace Game.UI.Hud
{
    public sealed class SpriteView : MonoBehaviour
    {
        private const float _introDuration = 0.75f;
        private const float _moveDuration = 1f;
        private const float _scaleDurationKoef = 0.5f;
        private const float _maxScale = 2.5f;

        public event Action<SpriteView> ON_MOVE_COMPLETE;

        [SerializeField] private RectTransform _rectTransform;

        public RectTransform Transform => _rectTransform;

        private Vector3 _endPosition;

        public void DoIntroAnimation(Vector3 endIntroPosition, Vector3 endPosition)
        {
            _endPosition = endPosition;

            _rectTransform.DOMove(endIntroPosition, _introDuration).SetEase(Ease.OutSine).OnComplete(DoMoveAnimation);
            _rectTransform.DOScale(Vector3.one * _maxScale, _introDuration * _scaleDurationKoef).SetEase(Ease.OutBack);
        }

        private void DoMoveAnimation()
        {
            _rectTransform.DOMove(_endPosition, _moveDuration).SetEase(Ease.OutCubic).OnComplete(OnMoveEnd);
            _rectTransform.DOScale(Vector3.one, _moveDuration * _scaleDurationKoef).SetEase(Ease.OutCirc);
        }

        private void OnMoveEnd()
        {
            ON_MOVE_COMPLETE.SafeInvoke(this);
        }
    }
}

