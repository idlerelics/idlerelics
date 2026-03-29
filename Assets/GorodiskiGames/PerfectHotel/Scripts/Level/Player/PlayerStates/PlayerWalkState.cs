using Game.Config;
using Game.Core;
using Game.UI;
using Injection;
using UnityEngine;

namespace Game.Level.Player
{
    public class PlayerFindEntityState : PlayerState
    {
        [Inject] protected Timer _timer;
        [Inject] protected GameManager _gameManager;
        [Inject] protected GameView _gameView;
        [Inject] protected GameConfig _config;

        public override void Initialize()
        {
            _timer.ONE_SECOND_TICK += OnSecondTick;
        }

        public override void Dispose()
        {
            _timer.ONE_SECOND_TICK -= OnSecondTick;
        }

        public virtual void OnSecondTick()
        {
            var entity = _gameManager.FindClosestEntity(_config.EntityRadius);

            if (entity != null)
                _gameView.CameraController.SetSign(entity.CameraAngleSign);
            else _gameView.CameraController.SetSign(0);
        }
    }


    public sealed class PlayerWalkState : PlayerFindEntityState
    {
        private float _walkSpeed;
        private float _rotateSpeed;

        public override void Initialize()
        {
            base.Initialize();

            _player.View.NavMeshAgent.enabled = true;

            _walkSpeed = _player.Model.WalkSpeed;
            _rotateSpeed = _player.Model.RotateSpeed;

            _timer.TICK += OnTick;

            _player.View.Walk(_gameManager.Model.InventoryTypes.Count);
        }

        public override void Dispose()
        {
            base.Dispose();

            _timer.TICK -= OnTick;
        }

        private void OnTick()
        {
            var cameraEulerY = _gameView.CameraController.transform.localEulerAngles.y;

            var joystickVector = new Vector2(_gameView.Joystick.Horizontal, _gameView.Joystick.Vertical);
            var angle = (Mathf.Atan2(_gameView.Joystick.Horizontal, _gameView.Joystick.Vertical) * Mathf.Rad2Deg) + cameraEulerY;

            var deltaAngle = Mathf.Abs(Mathf.DeltaAngle(_player.View.transform.localEulerAngles.y, angle )) / 90f;
            deltaAngle = 1 - Mathf.Clamp01(deltaAngle);

            angle = Mathf.LerpAngle(_player.View.transform.localEulerAngles.y, angle,
                Time.deltaTime * _rotateSpeed * joystickVector.sqrMagnitude);
            _player.View.transform.localEulerAngles = new Vector3(0f, angle, _player.View.transform.localEulerAngles.z);

            Vector3 direction = _player.View.transform.forward;
            var speed = _walkSpeed * deltaAngle * joystickVector.magnitude;
            _player.View.transform.position += direction.normalized * Time.deltaTime * speed;

            if (!_gameView.Joystick.HasInput)
            {
                _gameManager.Player.SwitchToState(new PlayerIdleState());
                return;
            }
        }
    }
}