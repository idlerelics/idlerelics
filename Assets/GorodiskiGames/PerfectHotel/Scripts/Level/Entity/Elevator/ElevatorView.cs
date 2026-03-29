using Game.Config;
using Game.Level.Item;
using Game.Level.Place;
using UnityEngine;

namespace Game.Level.Elevator
{
    public sealed class ElevatorView : PlaceWithWallsView
    {
        private const float _doorOpenXposition = 0.85f;

        [SerializeField] private ElevatorConfig _config;
        [SerializeField] private ItemView _showHudItemView;
        [SerializeField] private Transform _leftDoor;
        [SerializeField] private Transform _rightDoor;

        public ElevatorConfig Config => _config;
        public ItemView ShowHudItemView => _showHudItemView;

        private Vector3 _leftDoorClosePosition;
        private Vector3 _rightDoorClosePosition;

        public override void Awake()
        {
            base.Awake();

            _leftDoorClosePosition = _leftDoor.localPosition;
            _rightDoorClosePosition = _rightDoor.localPosition;
        }

        internal void CloseDoor()
        {
            _leftDoor.localPosition = new Vector3(0f, _leftDoorClosePosition.y, _leftDoorClosePosition.z);
            _rightDoor.localPosition = new Vector3(0f, _rightDoorClosePosition.y, _rightDoorClosePosition.z);
        }

        internal void OpenDoor()
        {
            _leftDoor.localPosition = new Vector3(-_doorOpenXposition, _leftDoorClosePosition.y, _leftDoorClosePosition.z);
            _rightDoor.localPosition = new Vector3(_doorOpenXposition, _rightDoorClosePosition.y, _rightDoorClosePosition.z);
        }
    }
}
