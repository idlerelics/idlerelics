using System;
using System.Collections;
using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// A custom event/listener system that ensures each listener is only registered once.
    /// Unlike C#'s built-in events (which allow duplicate subscriptions), OneListener
    /// prevents the same callback from being added twice.
    ///
    /// If an existing listener is re-added, it moves to the end of the list (latest priority).
    /// Removal uses "null marking" -- items are set to null during iteration and cleaned up after.
    /// This prevents issues when listeners add/remove other listeners during invocation.
    ///
    /// IEnumerable&lt;T&gt; allows iterating over the listeners with foreach.
    /// 'where T : class' constrains T to reference types (since we use null for removal).
    /// </summary>
    public abstract class BaseOneListener<T> : IEnumerable<T> where T : class
    {
        protected readonly List<T> _list = new List<T>();  // The listener list (may contain nulls)
        protected int _count;  // Actual number of active (non-null) listeners

        /// <summary>
        /// Adds a listener, or moves it to the end if already registered.
        /// If there's only one listener and it's being re-added, does nothing.
        /// </summary>
        public void Add(T action)
        {
            var index = _list.IndexOf(action);
            if (index == -1)
            {
                // Not found -- add it as a new listener
                _list.Add(action);
                _count++;
            }
            else
            {
                // Already exists -- move to end (null out old position, add at end)
                if (_count == 1) return; // Only listener, no need to move
                _list[index] = null;
                _list.Add(action);
            }
        }

        /// <summary>
        /// Removes a listener by setting its slot to null (lazy deletion).
        /// The null is cleaned up during the next Invoke call.
        /// </summary>
        public void Remove(T action)
        {
            var index = _list.IndexOf(action);
            if (index != -1)
            {
                _list[index] = null;
                _count--;
            }
        }

        /// <summary>Removes all listeners immediately.</summary>
        public void RemoveAll()
        {
            _list.Clear();
            _count = 0;
        }

        /// <summary>Returns true if the listener is registered (and not null-marked).</summary>
        public bool Contains(T action)
        {
            return _list.Contains(action);
        }

        /// <summary>The number of active (non-null) listeners.</summary>
        public int Count { get { return _count; } }

        // IEnumerable implementation for foreach support
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }

    /// <summary>
    /// OneListener for parameterless Actions.
    /// Invokes all registered listeners and then cleans up any null-marked slots.
    ///
    /// The cleanup happens AFTER all listeners have been called, which is important:
    /// a listener might remove itself (or others) during its callback, and we don't
    /// want to skip or double-invoke anyone.
    /// </summary>
    public sealed class OneListener : BaseOneListener<Action>
    {
        public void Invoke()
        {
            if (_count == 0)
                return;

            // Invoke all non-null listeners
            // Math.Min guards against listeners being removed during iteration
            int length = _list.Count;
            for (int i = 0; i < Math.Min(length, _list.Count); i++)
            {
                var current = _list[i];
                if (current != null)
                {
                    current.Invoke();
                }
            }

            // If no removals happened, skip cleanup
            if (_count == _list.Count)
                return;

            // Clean up null slots (iterate backwards to avoid index shifting issues)
            for (int i = _list.Count - 1; i >= 0; i--)
            {
                if (null == _list[i])
                {
                    _list.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>OneListener for Actions with one parameter.</summary>
    public sealed class OneListener<T> : BaseOneListener<Action<T>>
    {
        public void Invoke(T value)
        {
            if (_count == 0)
                return;

            int length = _list.Count;
            for (int i = 0; i < Math.Min(length, _list.Count); i++)
            {
                var current = _list[i];
                if (current != null)
                {
                    current.Invoke(value);
                }
            }

            if (_count == _list.Count)
                return;

            for (int i = _list.Count - 1; i >= 0; i--)
            {
                if (null == _list[i])
                {
                    _list.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>OneListener for Actions with two parameters.</summary>
    public sealed class OneListener<T1, T2> : BaseOneListener<Action<T1, T2>>
    {
        public void Invoke(T1 value1, T2 value2)
        {
            if (_count == 0)
                return;

            int length = _list.Count;
            for (int i = 0; i < Math.Min(length, _list.Count); i++)
            {
                var current = _list[i];
                if (current != null)
                {
                    current.Invoke(value1, value2);
                }
            }

            if (_count == _list.Count)
                return;

            for (int i = _list.Count - 1; i >= 0; i--)
            {
                if (null == _list[i])
                {
                    _list.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>OneListener for Actions with three parameters.</summary>
    public sealed class OneListener<T1, T2, T3> : BaseOneListener<Action<T1, T2, T3>>
    {
        public void Invoke(T1 value1, T2 value2, T3 value3)
        {
            if (_count == 0)
                return;

            int length = _list.Count;
            for (int i = 0; i < Math.Min(length, _list.Count); i++)
            {
                var current = _list[i];
                if (current != null)
                {
                    current.Invoke(value1, value2, value3);
                }
            }

            if (_count == _list.Count)
                return;

            for (int i = _list.Count - 1; i >= 0; i--)
            {
                if (null == _list[i])
                {
                    _list.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>OneListener for Actions with four parameters.</summary>
    public sealed class OneListener<T1, T2, T3, T4> : BaseOneListener<Action<T1, T2, T3, T4>>
    {
        public void Invoke(T1 value1, T2 value2, T3 value3, T4 value4)
        {
            if (_count == 0)
                return;

            int length = _list.Count;
            for (int i = 0; i < Math.Min(length, _list.Count); i++)
            {
                var current = _list[i];
                if (current != null)
                {
                    current.Invoke(value1, value2, value3, value4);
                }
            }

            if (_count == _list.Count)
                return;

            for (int i = _list.Count - 1; i >= 0; i--)
            {
                if (null == _list[i])
                {
                    _list.RemoveAt(i);
                }
            }
        }
    }
}
