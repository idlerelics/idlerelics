using System.Collections.Generic;
using Game.Level.Area;
using Game.Level.Cleaner;
using Game.Level.Elevator;
using Game.Level.Room;
using Injection;
using UnityEngine;

namespace Game.Level.Entity
{
    /// <summary>
    /// Sets up all the main game entities when a level loads: areas (floors),
    /// rooms, elevators, and cleaners. Think of it as the "level builder" that
    /// reads views placed in the Unity scene and creates their controllers.
    ///
    /// This follows the MVC pattern (Model-View-Controller):
    ///   - View = the visible GameObject in the scene (placed in the Unity Editor)
    ///   - Controller = the logic/brain (created here in code)
    ///   - Model = the data (created inside the controller)
    /// </summary>
    public sealed class EntityModule : Module<EntityModuleView>
    {
        [Inject] private Context _context;           // DI context for creating child objects
        [Inject] private GameManager _gameManager;   // Central game manager
        [Inject] private LevelView _levelView;       // The visual representation of the entire level

        private readonly List<CleanerController> _cleaners; // All cleaner NPCs in this level

        public EntityModule(EntityModuleView view) : base(view)
        {
            _cleaners = new List<CleanerController>();
        }

        /// <summary>
        /// Called once when the module is set up. Creates controllers for all
        /// areas, rooms, elevators, and cleaners found in the scene.
        /// </summary>
        public override void Initialize()
        {
            SetAreas();
            SetRooms();
            SetElevators();
            SetCleaners();
        }

        /// <summary>
        /// Cleans up all entities when this module is destroyed.
        /// Calls Dispose() on each entity to free resources and unsubscribe from events.
        /// Always clean up to avoid memory leaks!
        /// </summary>
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

        /// <summary>
        /// Creates AreaControllers from AreaViews placed in the scene.
        /// Areas represent floors/sections of the hotel. Each area has a target level
        /// that determines when it unlocks.
        /// </summary>
        private void SetAreas()
        {
            foreach (var view in _view.AreaViews)
            {
                var area = new AreaController(view, _context);
                var lvl = view.Config.TargetLvl;
                _gameManager.AreasMap.Add(lvl, area);       // Register area by its target level
                _levelView.AddLvl(area.View.Config.Number); // Tell the level view about this floor
            }

            // After all areas are created, initialize each one (triggers their state machine)
            foreach (var area in _gameManager.AreasMap.Values)
            {
                area.SwitchToState(new AreaInitializeState());
            }
        }

        /// <summary>
        /// Creates RoomControllers from RoomViews in the scene.
        /// Each room is registered with the GameManager so other systems can find it.
        /// </summary>
        private void SetRooms()
        {
            foreach (var view in _view.RoomViews)
            {
                var room = new RoomController(view, _context);
                _levelView.AddReward(room.Model.Area, room.GetTotalReward()); // Track total possible rewards
                _gameManager.Rooms.Add(room);
                _gameManager.Entities.Add(room); // Also add to the generic entities list
            }
        }

        /// <summary>Creates CleanerControllers (NPC cleaners) from CleanerViews in the scene.</summary>
        private void SetCleaners()
        {
            foreach (var view in _view.CleanerViews)
            {
                var cleaner = new CleanerController(view, _context);
                _cleaners.Add(cleaner);
            }
        }

        /// <summary>
        /// Creates the elevator controller. Only the first ElevatorView is used;
        /// any extras are destroyed. GameObject.Destroy() removes a GameObject from the scene.
        /// </summary>
        private void SetElevators()
        {
            int index = 0;
            foreach (var view in _view.ElevatorViews)
            {
                if (index == 0) _gameManager.Elevator = new ElevatorController(view, _context);
                else GameObject.Destroy(view.gameObject); // Remove duplicate elevator views
                index++;
            }
        }
    }
}
