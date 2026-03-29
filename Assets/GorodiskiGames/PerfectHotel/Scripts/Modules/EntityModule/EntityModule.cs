using System.Collections.Generic;
using Game.Level.Area;
using Game.Level.Cleaner;
using Game.Level.Elevator;
using Game.Level.Room;
using Injection;
using UnityEngine;

namespace Game.Level.Entity
{
    public sealed class EntityModule : Module<EntityModuleView>
    {
        [Inject] private Context _context;
        [Inject] private GameManager _gameManager;
        [Inject] private LevelView _levelView;

        private readonly List<CleanerController> _cleaners;

        public EntityModule(EntityModuleView view) : base(view)
        {
            _cleaners = new List<CleanerController>();
        }

        public override void Initialize()
        {
            SetAreas();
            SetRooms();
            SetElevators();
            SetCleaners();
        }

        public override void Dispose()
        {
            foreach (var area in _gameManager.AreasMap.Values)
            {
                area.Dispose();
            }
            _gameManager.AreasMap.Clear();

            foreach (var room in _gameManager.Rooms)
            {
                room.Dispose();
            }
            _gameManager.Rooms.Clear();

            foreach (var cleaner in _cleaners)
            {
                cleaner.Dispose();
            }
            _cleaners.Clear();

            _gameManager.Elevator.Dispose();
            _gameManager.Entities.Clear();
        }

        private void SetAreas()
        {
            foreach (var view in _view.AreaViews)
            {
                var area = new AreaController(view, _context);
                var lvl = view.Config.TargetLvl;
                _gameManager.AreasMap.Add(lvl, area);
                _levelView.AddLvl(area.View.Config.Number);
            }

            foreach (var area in _gameManager.AreasMap.Values)
            {
                area.SwitchToState(new AreaInitializeState());
            }
        }

        private void SetRooms()
        {
            foreach (var view in _view.RoomViews)
            {
                var room = new RoomController(view, _context);
                _levelView.AddReward(room.Model.Area, room.GetTotalReward());
                _gameManager.Rooms.Add(room);
                _gameManager.Entities.Add(room);
            }
        }

        private void SetCleaners()
        {
            foreach (var view in _view.CleanerViews)
            {
                var cleaner = new CleanerController(view, _context);
                _cleaners.Add(cleaner);
            }
        }

        private void SetElevators()
        {
            int index = 0;
            foreach (var view in _view.ElevatorViews)
            {
                if (index == 0) _gameManager.Elevator = new ElevatorController(view, _context);
                else GameObject.Destroy(view.gameObject);
                index++;
            }
        }
    }
}
