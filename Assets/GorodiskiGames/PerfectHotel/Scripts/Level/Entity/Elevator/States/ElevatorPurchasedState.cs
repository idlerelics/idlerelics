using Injection;
using UnityEngine;

namespace Game.Level.Elevator
{
    public sealed class ElevatorPurchasedState : ElevatorState
    {
        private const float _openDoorDistance = 5f;

        [Inject] private GameManager _gameManager;

        public override void Initialize()
        {
            _elevator.View.HudView.gameObject.SetActive(false);
            _elevator.View.MeshesHolder.SetActive(true);

            OnLvlChanged(_gameManager.Model.LoadLvl());

            _gameManager.AddItem(_elevator.ShowHudItem);

            _gameManager.LEVEL_CHANGED += OnLvlChanged;
            _timer.ONE_SECOND_TICK += OnSecondTick;
        }

        public override void Dispose()
        {
            _gameManager.LEVEL_CHANGED -= OnLvlChanged;
            _timer.ONE_SECOND_TICK -= OnSecondTick;
        }

        private void OnLvlChanged(int lvl)
        {
            _elevator.View.OutsideWalls.MeshesVisibilityLvl(lvl);
        }

        private void OnSecondTick()
        {
            float distance = Vector3.Distance(_gameManager.Player.View.transform.position, _elevator.View.transform.position);

            if (distance < _openDoorDistance) _elevator.View.OpenDoor();
            else _elevator.View.CloseDoor();
        }
    }
}

