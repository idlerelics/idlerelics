using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Utilities
{
    /// <summary>
    /// A system for scheduling actions to execute after a delay.
    /// Similar to Unity's Invoke() but more flexible -- supports parameters and can be
    /// managed (reset, disposed) as a group.
    ///
    /// Usage: Call DelayAction(seconds, callback) to schedule something.
    /// Then call Tick() every frame to check if any actions are ready to fire.
    ///
    /// IDisposable provides cleanup via Dispose() -- clears all pending actions.
    /// </summary>
    public class TimerDelayer : IDisposable
    {
        /// <summary>
        /// Interface for delayed action items. Using an interface allows different
        /// generic types (DelayItem, DelayItem&lt;T&gt;, DelayItem&lt;T0,T1&gt;) to be stored
        /// in the same list.
        /// </summary>
        private interface IDelayItem
        {
            float ActionTime { get; }   // The Time.time at which this should fire
            void SafeInvoke();           // Execute the stored action
        }

        /// <summary>Delayed action with no parameters.</summary>
        private class DelayItem : IDelayItem
        {
            public float ActionTime { get; private set; }

            private Action _action;

            /// <summary>
            /// Time.time is Unity's elapsed time since the game started (in seconds).
            /// Adding the delay to Time.time gives us the absolute time when this should fire.
            /// </summary>
            public DelayItem(float delay, Action action)
            {
                ActionTime = Time.time + delay;
                _action = action;
            }

            public void SafeInvoke()
            {
                if (null != _action)
                {
                    _action.Invoke();
                }
            }
        }

        /// <summary>Delayed action with one parameter.</summary>
        private class DelayItem<T> : IDelayItem
        {
            public float ActionTime { get; private set; }

            private Action<T> _action;
            private object _argument; // Stored as object for boxing, cast back on invoke

            public DelayItem(float delay, Action<T> action, object argument)
            {
                ActionTime = Time.time + delay;
                _action = action;
                _argument = argument;

            }
            public void SafeInvoke()
            {
                if (null != _action)
                {
                    _action.Invoke((T)_argument); // Cast back to the correct type
                }
            }
        }

        /// <summary>Delayed action with two parameters.</summary>
        private class DelayItem<T0,T1> : IDelayItem
        {
            public float ActionTime { get; private set; }

            private Action<T0, T1> _action;
            private object _argument0;
            private object _argument1;

            public DelayItem(float delay, Action<T0, T1> action, object argument0, object argument1)
            {
                ActionTime = Time.time + delay;
                _action = action;
                _argument0 = argument0;
                _argument1 = argument1;

            }
            public void SafeInvoke()
            {
                if (null != _action)
                {
                    _action.Invoke((T0)_argument0, (T1)_argument1);
                }
            }
        }


        private readonly List<IDelayItem> _items; // All pending delayed actions

        public TimerDelayer()
        {
            _items = new List<IDelayItem>();
        }

        /// <summary>Clears all pending actions (IDisposable implementation).</summary>
        public void Dispose()
        {
            Reset();
        }

        /// <summary>Cancels all pending delayed actions.</summary>
        public void Reset()
        {
            _items.Clear();
        }

        /// <summary>Schedule an action to run after 'delay' seconds.</summary>
        public void DelayAction(float delay, Action action)
        {
            _items.Add(new DelayItem(delay, action));
        }

        /// <summary>Schedule an action with one parameter to run after 'delay' seconds.</summary>
        public void DelayAction<T>(float delay, Action<T> action, object arg0)
        {
            _items.Add(new DelayItem<T>(delay, action, arg0));
        }

        /// <summary>Schedule an action with two parameters to run after 'delay' seconds.</summary>
        public void DelayAction<T0, T1>(float delay, Action<T0, T1> action, object arg0, object arg1)
        {
            _items.Add(new DelayItem<T0, T1>(delay, action, arg0, arg1));
        }

        /// <summary>
        /// Call this every frame to check and fire any actions whose time has come.
        /// Returns true if there are still pending actions, false if all have fired.
        ///
        /// Note the 'i--' after RemoveAt: when we remove an item at index i,
        /// the next item shifts down to index i, so we decrement to not skip it.
        /// </summary>
        public bool Tick()
        {
            IDelayItem item;

            for (int i = 0; i < _items.Count; i++)
            {
                item = _items[i];
                if (Time.time > item.ActionTime) // Time has passed -- fire it!
                {
                    _items.RemoveAt(i--);  // Remove and adjust index
                    item.SafeInvoke();     // Execute the delayed action
                }
            }

            return _items.Count > 0; // True if more actions are still pending
        }
    }
}
