using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Controls
{
    public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public event Action ON_FIRST_TOUCH;

        private const float _maxRadius = 60f;
        private const float _fadeSpeed = 4f;

        [SerializeField] private RectTransform _background, _handle;
        [SerializeField] private CanvasGroup _canvasGroup;

        [HideInInspector] public bool HasInput;
        [HideInInspector] public float Horizontal, Vertical;

        private Vector2 _inputDirection = Vector2.zero;
        private bool _firstTouchTriggered;
        private bool _isPointerDown;
        private float _targetAlpha;
        private bool _visibility;

        private void Awake()
        {
            _visibility = true;
            SetCanvasAlpha(0f);
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            _isPointerDown = true;
            _background.transform.position = eventData.position;

            SetTargetAlpha(1f);
            FireFirstTouch();
            OnDrag(eventData);

            HasInput = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            var position = RectTransformUtility.WorldToScreenPoint(null, _background.position);
            var radius = new Vector2(_maxRadius, _maxRadius);

            _inputDirection = (eventData.position - position) / radius;
            _inputDirection = _inputDirection.magnitude > 1 ? _inputDirection.normalized : _inputDirection;

            var anchoredPosition = _inputDirection * _maxRadius;
            SetHandlePosition(anchoredPosition);

            Horizontal = _inputDirection.x;
            Vertical = _inputDirection.y;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            HasInput = false;
            _isPointerDown = false;

            _inputDirection = Vector2.zero;

            Horizontal = 0f;
            Vertical = 0f;

            SetHandlePosition(Vector2.zero);
            SetTargetAlpha(0f);
        }

        private void Update()
        {
            var alpha = Mathf.MoveTowards(_canvasGroup.alpha, _targetAlpha, Time.deltaTime * _fadeSpeed);
            SetCanvasAlpha(alpha);

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL

            if (_isPointerDown)
                return;

            if (!Input.GetButton("Horizontal") && !Input.GetButton("Vertical"))
            {
                HasInput = false;

                _inputDirection = Vector2.zero;

                Horizontal = 0f;
                Vertical = 0f;

                _handle.anchoredPosition = Vector2.zero;

                return;
            }

            FireFirstTouch();

            HasInput = true;

            var h = Input.GetAxis("Horizontal");
            var v = Input.GetAxis("Vertical");
            _inputDirection = new Vector2(h, v);
            _inputDirection = _inputDirection.magnitude > 1 ? _inputDirection.normalized : _inputDirection;

            Horizontal = _inputDirection.x;
            Vertical = _inputDirection.y;

            var anchoredPosition = _inputDirection * _maxRadius;
            SetHandlePosition(anchoredPosition);
#endif

        }

        private void FireFirstTouch()
        {
            if (!_firstTouchTriggered)
            {
                _firstTouchTriggered = true;
                ON_FIRST_TOUCH?.Invoke();
            }
        }

        private void SetHandlePosition(Vector2 anchoredPosition)
        {
            _handle.anchoredPosition = anchoredPosition;
        }

        private void SetCanvasAlpha(float value)
        {
            _canvasGroup.alpha = value;
        }

        private void SetTargetAlpha(float value)
        {
            if (!_visibility)
                value = 0f;

            _targetAlpha = value;
        }

        public void Visibility(bool value)
        {
            _visibility = value;
        }
    }
}

