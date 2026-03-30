using System;
using Game.UI.Hud;

namespace Game.Core.UI
{
    /// <summary>
    /// Base class for the Mediator pattern -- connects logic to UI views.
    ///
    /// The Mediator pattern separates UI visuals (Views) from UI logic (Mediators).
    /// A View only knows how to display things. A Mediator knows WHAT to display
    /// and HOW to respond to user actions. This makes it easy to swap views
    /// without changing the logic, and vice versa.
    ///
    /// 'abstract' means this class can't be used directly -- you must create
    /// a subclass that implements the abstract methods (Mediate, Unmediate, etc.).
    /// </summary>
    public abstract class Mediator
    {
        /// <summary>The Type of view this mediator works with (used for matching).</summary>
        public abstract Type ViewType { get; }

        /// <summary>Connects this mediator to a view instance.</summary>
        public abstract void Mediate(object view);
        /// <summary>Disconnects this mediator from its view.</summary>
        public abstract void Unmediate();

        /// <summary>Called when the HUD should be shown.</summary>
        public abstract void InternalShow();
        /// <summary>Called when the HUD should be hidden.</summary>
        public abstract void InternalHide();
    }

    /// <summary>
    /// Generic mediator that works with a specific view type T.
    /// 'where T : IHud' is a "generic constraint" -- it ensures T must implement IHud.
    ///
    /// The generic type parameter T is filled in by subclasses:
    ///   class MyMediator : Mediator&lt;MyHudView&gt; { ... }
    ///
    /// 'sealed override' means the method overrides the parent AND cannot be
    /// overridden again by further subclasses. This ensures the core mediation
    /// logic stays consistent while subclasses customize Show() and Hide().
    /// </summary>
    public abstract class Mediator<T> : Mediator where T : IHud
    {
        private bool _isShowed;   // Tracks whether the view is currently visible
        protected T _view;        // The connected view (protected so subclasses can access it)

        /// <summary>Returns typeof(T) -- used by HudManager to match mediators to views.</summary>
        public override Type ViewType => typeof(T);
        /// <summary>Public access to the connected view.</summary>
        public T View => _view;

        /// <summary>
        /// Connects this mediator to a view. The 'object' parameter is cast to T.
        /// 'sealed' prevents subclasses from changing this core connection logic.
        /// </summary>
        public sealed override void Mediate(object view)
        {
            _view = (T) view;    // Cast the generic object to the specific view type
            _isShowed = false;
        }

        /// <summary>
        /// Disconnects the mediator from its view.
        /// If the view is currently shown, hides it first to clean up.
        /// 'default(T)' returns null for reference types, clearing the reference.
        /// </summary>
        public sealed override void Unmediate()
        {
            if (_isShowed)
            {
                Hide();
            }
            _view = default(T);
        }

        /// <summary>Activates the view and calls the subclass's Show() method.</summary>
        public sealed override void InternalShow()
        {
            _view.IsActive = true;
            _isShowed = true;
            Show();
        }

        /// <summary>Deactivates the view and calls the subclass's Hide() method.</summary>
        public sealed override void InternalHide()
        {
            _view.IsActive = false;
            _isShowed = false;
            Hide();
        }

        /// <summary>Subclasses implement this to set up event subscriptions, update UI, etc.</summary>
        protected abstract void Show();
        /// <summary>Subclasses implement this to unsubscribe from events, clean up, etc.</summary>
        protected abstract void Hide();
    }
}
