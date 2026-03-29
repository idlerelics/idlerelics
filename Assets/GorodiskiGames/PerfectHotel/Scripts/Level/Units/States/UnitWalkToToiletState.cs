using Game.Level.Item;
using Game.Level.Toilet;
using UnityEngine;

namespace Game.Level.Unit
{
    public sealed class UnitWalkToCabineState : UnitWalkState
    {
        private const float _openDoorDistance = 2f;

        private ToiletController _toilet;
        private ItemToiletController _cabine;
        private static Vector3 position;

        public UnitWalkToCabineState(ToiletController toilet, ItemController item) : base(position)
        {
            _toilet = toilet;
            _cabine = item as ItemToiletController;

            _endPosition = new Vector3(_cabine.View.CustomerPosition.x, 0f, _cabine.View.CustomerPosition.z);
        }

        public override void Initialize()
        {
            base.Initialize();

            _toilet.CabinesMap[_cabine] = false;

            _timer.TICK += OnTick;
        }

        public override void OnReachDistance()
        {
            _unit.SwitchToState(new UnitInToiletCabineState(_cabine));
        }

        private void OnTick()
        {
            if (Vector3.Distance(_unit.View.transform.position, _endPosition) > _openDoorDistance) return;

            _cabine.View.OpenDoor();

            _timer.TICK -= OnTick;
        }

        public override void Dispose()
        {
            base.Dispose();

            _timer.TICK -= OnTick;
        }
    }
}