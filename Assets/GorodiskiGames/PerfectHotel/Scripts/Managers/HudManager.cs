using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.UI;
using Game.UI;
using Injection;

namespace Game.Managers
{
    /// <summary>
    /// Manages HUD (Heads-Up Display) screens in the game, such as menus, popups, and overlays.
    /// It handles showing and hiding UI panels using the Mediator pattern.
    /// A "Mediator" connects game logic to a visual UI element (the "View").
    ///
    /// This manager supports two kinds of HUDs:
    /// - "Single" HUD: only one can be open at a time (e.g., a main menu).
    /// - "Additional" HUDs: multiple can be open simultaneously (e.g., popups, tooltips).
    /// </summary>
    public sealed class HudManager
    {
        // [Inject] is a "dependency injection" attribute. It tells the Injector framework
        // to automatically fill in this field with the correct instance.
        // This avoids manually passing references around -- the framework does it for you.
        [Inject] private GameView _gameView;    // Reference to the main game view that holds all HUD views
        [Inject] private Injector _injector;    // The injector that wires up dependencies automatically

        private Mediator _openedHud;                // The currently open single HUD (or null if none)
        private readonly List<Mediator> _additionalHuds; // List of all currently open additional HUDs

        /// <summary>
        /// Constructor: initializes the list that tracks additional (overlay) HUDs.
        /// </summary>
        public HudManager()
        {
            _additionalHuds = new List<Mediator>();
        }

        /// <summary>
        /// Shows a single HUD of type T. If another single HUD is already open, it is hidden first.
        ///
        /// This is a "generic method" -- the &lt;T&gt; means you specify which Mediator type to show
        /// when calling it, e.g.: ShowSingle&lt;MainMenuMediator&gt;().
        ///
        /// 'where T : Mediator' is a "constraint" -- it means T must be a Mediator or a subclass of it.
        ///
        /// 'object[] args = null' is an optional parameter with a default value of null.
        /// </summary>
        public T ShowSingle<T>(object[] args = null) where T : Mediator
        {
            // If a single HUD is already showing, hide it first (only one at a time)
            if (null != _openedHud)
            {
                HideSingle();
            }

            // Activator.CreateInstance creates a new object of type T at runtime.
            // This is called "reflection" -- creating objects from their Type rather than using 'new'.
            _openedHud = (Mediator)Activator.CreateInstance(typeof(T), args);

            // Inject dependencies into the new mediator (fills in its [Inject] fields)
            _injector.Inject(_openedHud);

            // Find the matching View from all available HUD views using LINQ's FirstOrDefault.
            // LINQ (Language Integrated Query) provides handy methods for searching collections.
            var hudType = _openedHud.ViewType;
            var hudView = _gameView.AllHuds().FirstOrDefault(temp => temp.GetType() == hudType);

            // Connect the mediator to its view and show it
            _openedHud.Mediate(hudView);
            _openedHud.InternalShow();

            // Cast back to type T so the caller gets the specific type they requested
            return (T)_openedHud;
        }

        /// <summary>
        /// Hides the currently open single HUD, if any.
        /// </summary>
        public void HideSingle()
        {
            if (null == _openedHud)
                return; // Nothing to hide

            _openedHud.InternalHide();  // Tell the mediator to hide its view
            _openedHud.Unmediate();     // Disconnect the mediator from its view
            _openedHud = null;          // Clear the reference
        }

        /// <summary>
        /// Shows an additional (overlay) HUD that can coexist with other HUDs.
        /// Unlike ShowSingle, this does NOT close any existing HUDs.
        /// Multiple additional HUDs can be open at the same time.
        /// </summary>
        public T ShowAdditional<T>(object[] args = null) where T : Mediator
        {
            var hud = (Mediator)Activator.CreateInstance(typeof(T), args);
            _injector.Inject(hud);
            var hudType = hud.ViewType;
            var hudView = _gameView.AllHuds().FirstOrDefault(temp => temp.GetType() == hudType);
            hud.Mediate(hudView);
            hud.InternalShow();

            _additionalHuds.Add(hud); // Track it in our list so we can hide it later
            return (T)hud;
        }

        /// <summary>
        /// Hides and removes all additional HUDs of type T.
        /// Iterates backwards through the list to safely remove items while looping.
        /// (Removing items while looping forward would skip elements or cause errors.)
        /// </summary>
        public void HideAdditional<T>()
        {
            for (int i = _additionalHuds.Count - 1; i >= 0; i--) // Loop backwards for safe removal
            {
                var hud = _additionalHuds[i];

                // 'is' checks if an object is a certain type. Here we skip HUDs that are NOT type T.
                if (!(hud is T))
                    continue;

                hud.InternalHide();
                hud.Unmediate();
                _additionalHuds.RemoveAt(i); // Remove from list by index
            }
        }
    }
}