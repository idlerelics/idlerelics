using Game.Config;
using Game.Core;
using Game.UI;
using Injection;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Level.Player
{
    /// <summary>
    /// Base state that detects nearby entities and adjusts the camera angle.
    /// Both PlayerIdleState and PlayerWalkState inherit from this.
    ///
    /// Every second, it checks if any entity (room, reception, etc.) is close to the player.
    /// If so, it rotates the camera to give a better view of that entity.
    /// </summary>
    public class PlayerFindEntityState : PlayerState
    {
        [Inject] protected Timer _timer;
        [Inject] protected GameManager _gameManager;
        [Inject] protected GameView _gameView;
        [Inject] protected GameConfig _config;

        public override void Initialize()
        {
            // ONE_SECOND_TICK fires once per second (not every frame) for less frequent checks
            _timer.ONE_SECOND_TICK += OnSecondTick;
        }

        public override void Dispose()
        {
            _timer.ONE_SECOND_TICK -= OnSecondTick;
        }

        /// <summary>
        /// Called once per second. Finds the closest entity within EntityRadius and
        /// adjusts the camera rotation sign to give a better viewing angle.
        /// </summary>
        public virtual void OnSecondTick()
        {
            var entity = _gameManager.FindClosestEntity(_config.EntityRadius);

            if (entity != null)
                _gameView.CameraController.SetSign(entity.CameraAngleSign); // Rotate camera toward entity
            else _gameView.CameraController.SetSign(0); // Reset to default angle
        }
    }


    /// <summary>
    /// The player's walking state -- active when the joystick has input.
    /// Handles movement, rotation, and transitions back to idle when input stops.
    ///
    /// Movement uses the joystick direction relative to the camera's Y rotation,
    /// so "up" on the joystick always moves the player away from the camera.
    /// </summary>
    public sealed class PlayerWalkState : PlayerFindEntityState
    {
        private float _walkSpeed;    // How fast the player moves (from PlayerConfig)
        private float _rotateSpeed;  // How fast the player turns (from PlayerConfig)

        public override void Initialize()
        {
            base.Initialize(); // Subscribe to ONE_SECOND_TICK for entity detection

            // Enable the NavMeshAgent so the player respects walkable areas
            _player.View.NavMeshAgent.enabled = true;
            _player.View.NavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

            // Cache speed values from the player model
            _walkSpeed = _player.Model.WalkSpeed;
            _rotateSpeed = _player.Model.RotateSpeed;

            // Subscribe to per-frame updates for smooth movement
            _timer.TICK += OnTick;

            // Play walk animation (different if carrying inventory items)
            _player.View.Walk(_gameManager.Model.InventoryTypes.Count);
        }

        public override void Dispose()
        {
            base.Dispose();

            _timer.TICK -= OnTick;
        }

        /// <summary>
        /// Called every frame. Handles player rotation and movement based on joystick input.
        ///
        /// The math here:
        /// 1. Get the joystick's angle in degrees using Atan2 (converts X,Y to angle)
        /// 2. Add the camera's Y rotation so movement is relative to the camera view
        /// 3. Smoothly rotate the player toward the target angle using LerpAngle
        /// 4. Move the player forward at a speed that slows when turning sharply (deltaAngle)
        /// </summary>
        private void OnTick()
        {
            // Get the camera's Y rotation to make movement camera-relative
            var cameraEulerY = _gameView.CameraController.transform.localEulerAngles.y;

            // Get joystick input as a 2D vector
            var joystickVector = new Vector2(_gameView.Joystick.Horizontal, _gameView.Joystick.Vertical);
            // Convert joystick direction to an angle in degrees, offset by camera rotation
            // Mathf.Atan2 returns radians, Mathf.Rad2Deg converts to degrees
            var angle = (Mathf.Atan2(_gameView.Joystick.Horizontal, _gameView.Joystick.Vertical) * Mathf.Rad2Deg) + cameraEulerY;

            // Calculate how much the player needs to turn (0 = facing target, 1 = perpendicular)
            // This slows movement when turning sharply for a more natural feel
            var deltaAngle = Mathf.Abs(Mathf.DeltaAngle(_player.View.transform.localEulerAngles.y, angle )) / 90f;
            deltaAngle = 1 - Mathf.Clamp01(deltaAngle); // Invert: 1 = aligned, 0 = perpendicular

            // Smoothly rotate toward the target angle
            // sqrMagnitude makes rotation faster when joystick is pushed further
            angle = Mathf.LerpAngle(_player.View.transform.localEulerAngles.y, angle,
                Time.deltaTime * _rotateSpeed * joystickVector.sqrMagnitude);
            _player.View.transform.localEulerAngles = new Vector3(0f, angle, _player.View.transform.localEulerAngles.z);

            // Move the player forward (in the direction they're facing)
            Vector3 direction = _player.View.transform.forward;
            var speed = _walkSpeed * deltaAngle * joystickVector.magnitude;
            _player.View.transform.position += direction.normalized * Time.deltaTime * speed;

            // If joystick is released, go back to idle
            if (!_gameView.Joystick.HasInput)
            {
                _gameManager.Player.SwitchToState(new PlayerIdleState());
                return;
            }
        }
    }
}
