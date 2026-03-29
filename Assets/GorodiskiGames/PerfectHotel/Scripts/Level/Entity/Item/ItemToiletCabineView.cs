using System.Collections;
using Game.Level.Unit;
using UnityEngine;

namespace Game.Level.Item
{
    public sealed class ItemToiletCabineView : ItemAimView
    {
        private const float _closeDoorDelay = 2f;
        private const float _opneAngle = 110f;

        [SerializeField] private float _unitAngle;
        [SerializeField] private AnimationType _unitAnimationType;
        [SerializeField] private Transform _door;

        public float UnitAngle => _unitAngle;
        public AnimationType UnitAnimationType => _unitAnimationType;


        internal void OpenDoor()
        {
            if (_door == null) return;
            _door.localEulerAngles = new Vector3(0f, _opneAngle, 0f);
        }

        internal void CloseDoor()
        {
            if (_door == null) return;
            _door.localEulerAngles = Vector3.zero;
        }

        internal void CloseDoorWithDelay()
        {
            StartCoroutine(WaitAndCloseDoor());
        }

        private IEnumerator WaitAndCloseDoor()
        {
            yield return new WaitForSeconds(_closeDoorDelay);
            CloseDoor();
        }
    }
}
