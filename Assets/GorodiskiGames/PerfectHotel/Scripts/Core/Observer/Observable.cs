using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Core
{
    /// <summary>
    /// INTERFACE: An interface is like a contract/promise. Any class that implements IObservable
    /// MUST provide these 3 methods. Interfaces define WHAT a class can do, not HOW it does it.
    ///
    /// This is part of the "Observer Pattern" -- a design pattern where an object (Observable)
    /// notifies a list of watchers (Observers) whenever something changes.
    /// Think of it like a YouTube channel: subscribers get notified when a new video is posted.
    /// </summary>
    public interface IObservable
    {
        void SetChanged();
        void AddObserver(IObserver observer);
        void RemoveObserver(IObserver observer);
    }

    /// <summary>
    /// Base class for any data/model object that needs to notify others when it changes.
    ///
    /// "abstract" means you CANNOT create an Observable directly (new Observable() won't work).
    /// You must create a subclass (e.g., "class PlayerData : Observable") that extends it.
    /// Abstract classes can contain both implemented methods AND abstract methods (with no body).
    ///
    /// [Serializable] tells C# this class can be converted to/from bytes (for saving/loading).
    /// </summary>
    [Serializable]
    public abstract class Observable : IObservable
    {
        // [NonSerialized] means this field is skipped when saving -- we don't want to save
        // the list of observers, only the actual data. "readonly" means it can only be set
        // in the constructor.
        [NonSerialized]
        private readonly List<IObserver> _observers;

        // Keeps track of how many active (non-null) observers are registered
        private int _count;

        /// <summary>
        /// "protected" means only this class and its children can call this constructor.
        /// </summary>
        protected Observable()
        {
            _observers = new List<IObserver>();
        }

        /// <summary>
        /// Whether this object has unsaved/uncommitted changes.
        /// [XmlIgnore] means this property is skipped during XML serialization (saving to XML).
        /// "get; private set;" means anyone can read it, but only this class can change it.
        /// </summary>
        [XmlIgnore]
        public bool IsChanged
        {
            get;
            private set;
        }

        /// <summary>
        /// Marks this object as changed AND notifies all observers immediately.
        /// Each observer's OnObjectChanged() method will be called.
        /// </summary>
        public void SetChanged()
        {
            IsChanged = true;

            // No observers? Nothing to notify.
            if (_count == 0)
                return;

            // Notify all current observers. Math.Min protects against the list changing
            // during iteration (an observer might remove itself when notified).
            int length = _observers.Count;
            for (int i = 0; i < Math.Min(length, _observers.Count); i++)
            {
                var current = _observers[i];
                if (current != null)
                {
                    // "this" refers to the current Observable instance -- we pass it so
                    // the observer knows WHICH object changed
                    current.OnObjectChanged(this);
                }
            }

            // Clean up: remove any null entries (observers that were removed during notification)
            if (_count == _observers.Count)
                return;

            // Loop backwards so removing items doesn't mess up the loop index
            for (int i = _observers.Count - 1; i >= 0; i--)
            {
                if (null == _observers[i])
                {
                    _observers.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Resets the IsChanged flag. Call this after you've handled the change.
        /// </summary>
        public void Commit()
        {
            IsChanged = false;
        }

        /// <summary>
        /// Convenience method: notifies observers AND immediately resets the changed flag.
        /// Useful for one-shot notifications where you don't need to track the changed state.
        /// </summary>
        public void SetChangedAndCommit()
        {
            SetChanged();
            Commit();
        }

        /// <summary>
        /// Subscribes an observer to receive change notifications from this object.
        /// If the observer is already subscribed, it moves it to the end of the list
        /// (ensuring it's called last).
        /// </summary>
        public void AddObserver(IObserver observer)
        {
            var index = _observers.IndexOf(observer);
            if (index == -1)
            {
                // New observer -- add it to the list
                _observers.Add(observer);
                _count++;
                OnObserversChanged(_count);
            }
            else
            {
                // Already exists -- null out old position and re-add at end
                if (_count == 1) return;
                _observers[index] = null;
                _observers.Add(observer);
            }
        }

        /// <summary>
        /// Unsubscribes an observer so it no longer receives change notifications.
        /// Sets the slot to null instead of removing immediately (safe during iteration).
        /// </summary>
        public void RemoveObserver(IObserver observer)
        {
            var index = _observers.IndexOf(observer);
            if (index != -1)
            {
                _observers[index] = null;
                _count--;
                OnObserversChanged(_count);
            }
        }

        /// <summary>
        /// Removes ALL observers from this object.
        /// </summary>
        public void Clear()
        {
            _observers.Clear();
            _count = 0;
            OnObserversChanged(_count);
        }

        /// <summary>
        /// Called whenever the observer count changes. "virtual" means subclasses can
        /// override this method to add custom behavior (e.g., start/stop updates based
        /// on whether anyone is watching).
        /// </summary>
        protected virtual void OnObserversChanged(int count)
        {
        }
    }
}
