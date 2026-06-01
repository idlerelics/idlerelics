using UnityEngine;
using UnityEngine.AI;

namespace Game.Level.Unit
{
    /// <summary>
    /// Drives a billboarded SpriteRenderer to display an 8-direction, N-frame-per-direction
    /// sprite sheet character. Replaces the rigged 3D mesh for the 2.5D approach.
    /// </summary>
    [DefaultExecutionOrder(10000)]
    public sealed class SpriteCharacterView : MonoBehaviour
    {
        // Direction indices match sprite-sheet row order (top → bottom):
        // 0=S, 1=SW, 2=W, 3=NW, 4=N, 5=NE, 6=E, 7=SE.
        // Mapped from a camera-relative yaw angle.
        private const int DirectionCount = 8;

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private NavMeshAgent _navMeshAgent;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Camera _camera;

        [SerializeField] private Sprite[] _walkFrames; // length = 8 * framesPerDirection, row-major
        [SerializeField] private int _framesPerDirection = 6;
        [SerializeField] private float _walkFps = 12f;
        [SerializeField] private float _minMoveSpeed = 0.05f;
        [SerializeField] private int _idleFrameIndex = 0;
        [SerializeField] private int _initialDirection = 0;

        // Optional per-direction idle animations. When the character stops moving while
        // facing a given direction, the corresponding idle clip plays in a loop. If a
        // direction's array is empty, we fall back to frame `_idleFrameIndex` of the walk
        // cycle for that direction.
        [SerializeField] private Sprite[] _idleFramesFront; // direction 0 = S (toward camera)
        [SerializeField] private float _idleFps = 10f;

        private Transform _cameraTransform;
        private Vector3 _lastPosition;
        private int _currentDirection;
        private float _frameTimer;
        private int _currentFrame;
        private float _idleFrameTimer;
        private int _idleCurrentFrame;
        private bool _isMoving;

        private void Awake()
        {
            _currentDirection = Mathf.Clamp(_initialDirection, 0, DirectionCount - 1);
            ResolveCameraTransform();
        }

        private void OnEnable()
        {
            ResolveCameraTransform();
            _lastPosition = transform.position;
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null)
            {
                ResolveCameraTransform();
                if (_cameraTransform == null)
                    return;
            }

            BillboardYawOnly();

            Vector3 worldVelocity = GetWorldVelocity();
            _isMoving = worldVelocity.sqrMagnitude > (_minMoveSpeed * _minMoveSpeed);
            if (_isMoving)
                _currentDirection = ResolveDirectionIndex(worldVelocity);

            AdvanceFrame();
            ApplyCurrentSprite();
        }

        private void BillboardYawOnly()
        {
            // SpriteRenderer's visible face points along +Z; we want that face toward
            // the camera, so transform.forward must point AWAY from the camera (i.e.
            // along the camera's view direction back-projected onto the ground plane).
            Vector3 spriteForward = -_cameraTransform.forward;
            spriteForward.y = 0f;
            if (spriteForward.sqrMagnitude < 0.0001f)
                return;
            transform.rotation = Quaternion.LookRotation(spriteForward, Vector3.up);
        }

        private Vector3 GetWorldVelocity()
        {
            // Transform-delta is the most reliable signal because it picks up movement no
            // matter how the player is driven (NavMeshAgent.SetDestination, transform writes
            // from joystick code, kinematic Rigidbody.MovePosition, etc).
            Vector3 current = transform.position;
            Vector3 delta = (current - _lastPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
            _lastPosition = current;

            // If NavMeshAgent is doing the moving and it reports a higher velocity (e.g. on
            // first frame before the transform has actually moved yet), prefer that.
            if (_navMeshAgent != null && _navMeshAgent.enabled)
            {
                var agentVel = _navMeshAgent.velocity;
                if (agentVel.sqrMagnitude > delta.sqrMagnitude)
                    return agentVel;
            }

            if (delta.sqrMagnitude > 0.0001f)
                return delta;

            if (_rigidbody != null)
                return _rigidbody.linearVelocity;

            return Vector3.zero;
        }

        private int ResolveDirectionIndex(Vector3 worldVelocity)
        {
            // Project velocity onto camera-relative ground axes.
            Vector3 camForward = _cameraTransform.forward;
            camForward.y = 0f;
            camForward.Normalize();
            // Unity left-handed: Right = Cross(Forward, Up). For Forward=(fx,0,fz),
            // that's (-fz, 0, fx).
            Vector3 camRight = new Vector3(-camForward.z, 0f, camForward.x);

            float forwardDot = Vector3.Dot(worldVelocity, camForward);
            float rightDot = Vector3.Dot(worldVelocity, camRight);

            // Angle measured clockwise from "toward camera" (S).
            // atan2(right, -forward) → 0 means moving toward camera = S.
            float angleDeg = Mathf.Atan2(rightDot, -forwardDot) * Mathf.Rad2Deg;
            if (angleDeg < 0f) angleDeg += 360f;

            // Bin into 8 sectors of 45° each, centered on each cardinal/ordinal direction.
            int index = Mathf.RoundToInt(angleDeg / 45f) % DirectionCount;
            return index;
        }

        private void AdvanceFrame()
        {
            if (!_isMoving)
            {
                _currentFrame = _idleFrameIndex;
                _frameTimer = 0f;

                Sprite[] idleClip = GetIdleClipForDirection(_currentDirection);
                if (idleClip != null && idleClip.Length > 0)
                {
                    _idleFrameTimer += Time.deltaTime;
                    float idleFrameDuration = 1f / Mathf.Max(_idleFps, 0.01f);
                    while (_idleFrameTimer >= idleFrameDuration)
                    {
                        _idleFrameTimer -= idleFrameDuration;
                        _idleCurrentFrame = (_idleCurrentFrame + 1) % idleClip.Length;
                    }
                }
                else
                {
                    _idleFrameTimer = 0f;
                    _idleCurrentFrame = 0;
                }
                return;
            }

            // Walking — reset idle anim so we restart cleanly next time we stop.
            _idleFrameTimer = 0f;
            _idleCurrentFrame = 0;

            _frameTimer += Time.deltaTime;
            float frameDuration = 1f / Mathf.Max(_walkFps, 0.01f);
            while (_frameTimer >= frameDuration)
            {
                _frameTimer -= frameDuration;
                _currentFrame = (_currentFrame + 1) % _framesPerDirection;
            }
        }

        private Sprite[] GetIdleClipForDirection(int direction)
        {
            // Only "facing forward" (S, direction 0) has its own idle clip today; other
            // directions fall through to the walk-frame-0 fallback in ApplyCurrentSprite.
            if (direction == 0) return _idleFramesFront;
            return null;
        }

        private void ApplyCurrentSprite()
        {
            if (_spriteRenderer == null) return;

            if (!_isMoving)
            {
                Sprite[] idleClip = GetIdleClipForDirection(_currentDirection);
                if (idleClip != null && idleClip.Length > 0)
                {
                    int idx = Mathf.Clamp(_idleCurrentFrame, 0, idleClip.Length - 1);
                    var idleSprite = idleClip[idx];
                    if (idleSprite != null)
                    {
                        _spriteRenderer.sprite = idleSprite;
                        return;
                    }
                }
            }

            if (_walkFrames == null || _walkFrames.Length == 0) return;

            int spriteIndex = _currentDirection * _framesPerDirection + _currentFrame;
            if (spriteIndex < 0 || spriteIndex >= _walkFrames.Length)
                return;

            var sprite = _walkFrames[spriteIndex];
            if (sprite != null)
                _spriteRenderer.sprite = sprite;
        }

        private void ResolveCameraTransform()
        {
            if (_camera != null)
            {
                _cameraTransform = _camera.transform;
                return;
            }
            var main = Camera.main;
            _cameraTransform = main != null ? main.transform : null;
        }

        public void SetCamera(Camera camera)
        {
            _camera = camera;
            _cameraTransform = camera != null ? camera.transform : null;
        }
    }
}
