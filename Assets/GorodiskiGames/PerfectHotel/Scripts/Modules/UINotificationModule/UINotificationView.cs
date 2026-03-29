using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Game.Modules.UINotificationModule
{
    public enum UINotificationType
    {
        None,
        AreaLocked
    }

    public sealed class UINotificationView : MonoBehaviour
    {
        public Action<UINotificationView> ON_REMOVE;

        private const float _duartionMove = 2.5f;
        private const float _initialScale = 0.5f;
        private const float _duartionScale = 0.25f;
        private const float _amplitudeScaleIn = 2f;

        [SerializeField] private TMP_Text _text;
        [SerializeField] private TMP_Text _lvlText;
        [SerializeField] private RectTransform _rectTransform;

        public void Initialize(Vector3 screenPosition, string message, int targetLvl)
        {
            _text.text = message.ToString();
            _lvlText.text = targetLvl.ToString();

            _rectTransform.localScale = Vector3.one * _initialScale;
            _rectTransform.position = screenPosition;

            float height = UnityEngine.Random.Range(100f, 150f);
            float endPositionY = _rectTransform.position.y + height;
            _rectTransform.DOMoveY(endPositionY, _duartionMove).SetEase(Ease.OutQuart).OnComplete(FireRemove);

            _rectTransform.DOScale(Vector3.one, _duartionScale).SetEase(Ease.OutQuad, _amplitudeScaleIn);
        }

        private void FireRemove()
        {
            ON_REMOVE?.Invoke(this);
        }
    }
}


