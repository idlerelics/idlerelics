using System;
using DG.Tweening; // DOTween library -- used for smooth animations (tweens)
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Controls the game camera: follows the player, handles zoom in/out, and rotation.
    ///
    /// MonoBehaviour is the base class for all Unity scripts attached to GameObjects.
    /// It gives you access to lifecycle methods like Awake(), Update(), OnEnable(), etc.
    /// Unity calls these methods automatically at specific times:
    ///   - Awake() runs once when the GameObject is first created
    ///   - OnEnable() runs each time the GameObject becomes active
    ///   - Update() runs once every frame (60+ times per second)
    /// </summary>
    public sealed class CameraController : MonoBehaviour
    {
        // "const" values are fixed at compile time and never change.
        private const float _angleY = 35f;           // The Y rotation angle for the camera
        private const float _speedPosition = 5f;     // How fast the camera follows the target
        private const float _speedRotation = 3f;     // How fast the camera rotates
        private const float _zPositionDelta = 5f;    // How far the camera moves forward when zooming in
        private const float _zoomDuration = 1f;      // How long the zoom animation takes (in seconds)
        private const float _duration = 2f;          // How long to wait before auto-zooming back out

        // [SerializeField] exposes this private field in the Unity Inspector,
        // so you can drag your Camera component onto it in the Editor.
        [SerializeField] private Camera _camera;

        private float _timer;                // Counts time for the auto-zoom-out delay
        private float _zInitialPosition;     // The camera's starting Z position (for resetting zoom)
        private bool _isAutoZoomOut;          // If true, the camera will auto-zoom-out after a delay
        private Transform _player;           // Reference to the player's Transform
        private Transform _target;           // The current target the camera is following

        public Camera Camera => _camera;     // Public access to the Camera component
        private int _sign;                   // Controls rotation direction (-1, 0, or 1)

        /// <summary>
        /// Awake is called once when this script is first loaded.
        /// Saves the camera's initial Z position so we can return to it after zooming.
        /// </summary>
        private void Awake()
        {
            _zInitialPosition = _camera.transform.localPosition.z;
            _sign = 0;
        }

        /// <summary>
        /// OnEnable is called each time this GameObject becomes active.
        /// Resets the camera position to the origin.
        /// </summary>
        private void OnEnable()
        {
            transform.position = Vector3.zero;
        }

        /// <summary>
        /// Update is called once per frame by Unity.
        /// Smoothly moves the camera toward the target using Lerp (Linear Interpolation).
        /// Lerp blends between two values -- here it creates smooth "easing" movement.
        /// Time.deltaTime is the time since the last frame, making movement frame-rate independent.
        /// </summary>
        private void Update()
        {
            // Smoothly move camera position toward the target
            transform.position = Vector3.Lerp(transform.position, _target.position, Time.deltaTime * _speedPosition);
            // Smoothly rotate camera toward the desired angle
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.rotation.x, _angleY * _sign, transform.rotation.z), Time.deltaTime * _speedRotation);

            // If auto-zoom-out is not active, skip the rest
            if (!_isAutoZoomOut)
                return;

            // Count up the timer; when enough time has passed, zoom back out
            _timer += Time.deltaTime;
            if (_timer >= _duration)
            {
                _timer = 0f;
                _isAutoZoomOut = false;

                _target = _player; // Go back to following the player

                ZoomOut();
            }
        }

        /// <summary>Sets a new target for the camera to follow.</summary>
        public void SetTarget(Transform target)
        {
            _target = target;
        }

        /// <summary>
        /// Zooms the camera in by moving it forward along the Z axis.
        /// Uses DOTween's DOLocalMoveZ for a smooth animated transition.
        /// DOKill() stops any existing tween on this transform to prevent conflicts.
        /// </summary>
        public void ZoomIn(bool isAutoZoomOut)
        {
            _camera.transform.DOKill();
            _camera.transform.DOLocalMoveZ(_zInitialPosition + _zPositionDelta, _zoomDuration);
            _isAutoZoomOut = isAutoZoomOut;
        }

        /// <summary>Sets the player reference and makes the camera follow them.</summary>
        public void SetPlayer(Transform transform)
        {
            _player = transform;
            SetTarget(_player);
        }

        /// <summary>Zooms the camera back out to its original Z position.</summary>
        public void ZoomOut()
        {
            _camera.transform.DOKill();
            _camera.transform.DOLocalMoveZ(_zInitialPosition, _zoomDuration);
        }

        /// <summary>Sets the rotation direction: -1 (left), 0 (center), or 1 (right).</summary>
        public void SetSign(int sign)
        {
            _sign = sign;
        }

        /// <summary>Instantly moves the camera to the given position (no smooth transition).</summary>
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }
    }
}
