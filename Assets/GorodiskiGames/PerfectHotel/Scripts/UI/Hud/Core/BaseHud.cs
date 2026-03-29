using Core;
using UnityEngine;

namespace Game.UI.Hud
{
    /// <summary>
    /// Interface that all HUD panels must implement.
    /// Provides a way to show/hide the HUD and access its underlying GameObject.
    ///
    /// Interfaces in C# define a "contract" -- any class that implements IHud
    /// must provide an IsActive property and a GameObject property.
    /// </summary>
    public interface IHud
    {
        /// <summary>
        /// Set to true/false to show/hide this HUD panel.
        /// Only has a setter in the interface, meaning external code can set it
        /// but the getter is implementation-specific.
        /// </summary>
        bool IsActive
        {
            set;
        }

        /// <summary>
        /// Returns the Unity GameObject associated with this HUD,
        /// allowing access to transform, SetActive, etc.
        /// </summary>
        GameObject GameObject { get; }
    }

    /// <summary>
    /// Abstract base class for all HUD panels in the game.
    /// Provides common show/hide functionality by wrapping Unity's SetActive.
    ///
    /// "abstract" means this class cannot be instantiated directly -- you must
    /// create a subclass (e.g., GamePlayHudView, ShopHudView) that implements
    /// the abstract OnEnable and OnDisable methods.
    ///
    /// All HUD views in the project inherit from this class, giving them
    /// consistent activation/deactivation behavior.
    /// </summary>
    public abstract class BaseHud : MonoBehaviour, IHud
    {
        /// <summary>
        /// Controls the visibility of this HUD panel.
        /// Getting returns whether the GameObject is currently active in the scene.
        /// Setting calls SetActive to show or hide the entire HUD.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return gameObject.activeSelf;
            }
            set
            {
                gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// Exposes the underlying Unity GameObject for this HUD panel.
        /// "gameObject" (lowercase) is a built-in MonoBehaviour property that
        /// returns the GameObject this component is attached to.
        /// </summary>
        public GameObject GameObject
        {
            get
            {
                return gameObject;
            }
        }

        /// <summary>
        /// Called by Unity when this HUD's GameObject becomes active.
        /// Subclasses use this to subscribe to events (button clicks, model changes, etc.).
        /// </summary>
        protected abstract void OnEnable();

        /// <summary>
        /// Called by Unity when this HUD's GameObject becomes inactive.
        /// Subclasses use this to unsubscribe from events to prevent memory leaks
        /// and errors from callbacks firing on disabled objects.
        /// </summary>
        protected abstract void OnDisable();
    }

    /// <summary>
    /// Generic base class for HUD panels that are bound to a data model.
    /// Combines BaseHud's show/hide behavior with the Observer pattern --
    /// when the model changes, OnModelChanged is called automatically to update the UI.
    ///
    /// The type parameter T must be a subclass of Observable, which provides
    /// the AddObserver/RemoveObserver mechanism for change notifications.
    ///
    /// Implements IObserver so it can receive OnObjectChanged callbacks from
    /// the Observable model whenever data changes.
    ///
    /// Example usage: GamePlayHudView extends BaseHudWithModel&lt;GameModel&gt;
    /// to automatically update cash display whenever the GameModel changes.
    /// </summary>
    /// <typeparam name="T">The Observable model type this HUD is bound to.</typeparam>
    public abstract class BaseHudWithModel<T> : BaseHud, IObserver where T : Observable
    {
        // The data model this HUD is observing. Private to enforce access through the property.
        private T _model;

        /// <summary>
        /// The data model bound to this HUD. Setting this property:
        /// 1. Removes this HUD as an observer from the old model (if any)
        /// 2. Calls OnApplyModel to let subclasses perform setup
        /// 3. Stores the new model reference
        /// 4. Registers this HUD as an observer on the new model
        /// 5. Immediately calls OnModelChanged to sync the UI with the new data
        ///
        /// "protected get" means only this class and subclasses can read the model.
        /// "set" is public so that external code (mediators, controllers) can assign the model.
        /// </summary>
        public T Model
        {
            protected get
            {
                return _model;
            }
            set
            {
                // Unsubscribe from the old model to prevent stale callbacks
                if (null != _model)
                {
                    _model.RemoveObserver(this);
                }

                // Let subclasses react to the new model being assigned
                OnApplyModel(value);

                _model = value;

                // Subscribe to the new model and immediately update the UI
                if (null != _model)
                {
                    _model.AddObserver(this);
                    OnModelChanged(_model);
                }
            }
        }

        protected BaseHudWithModel()
        {
        }

        /// <summary>
        /// Called whenever the bound model's data changes. Subclasses override this
        /// to update their UI elements (text, images, fill bars, etc.) with the new data.
        /// </summary>
        /// <param name="model">The updated model containing the new data.</param>
        protected abstract void OnModelChanged(T model);

        /// <summary>
        /// Called when a new model is about to be assigned (before the field is set).
        /// Subclasses can override this to perform one-time setup or cleanup.
        /// </summary>
        /// <param name="model">The new model being applied (may be null).</param>
        protected virtual void OnApplyModel(T model)
        {
        }

        #region Observer implementation
        /// <summary>
        /// Called by the Observable (model) whenever it changes.
        /// This is the IObserver interface method that bridges the Observer pattern
        /// to the strongly-typed OnModelChanged method.
        ///
        /// If the observable is the expected type T, it casts and forwards directly.
        /// Otherwise, it falls back to using the stored Model reference.
        /// The "is" keyword checks the runtime type, and the cast (T) converts it.
        /// </summary>
        /// <param name="observable">The observable object that changed.</param>
        public void OnObjectChanged(Observable observable)
        {
            if (observable is T)
            {
                OnModelChanged((T)observable);
            }
            else
            {
                OnModelChanged(Model);
            }
        }
        #endregion
    }
}
