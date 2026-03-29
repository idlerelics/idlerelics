using System;
using System.Collections.Generic;
using UnityEngine;

namespace Injection
{
    /// <summary>
    /// A dependency injection container (like a "toolbox") that stores shared objects (services)
    /// so other parts of the game can retrieve them without needing direct references.
    ///
    /// "sealed" means no other class can inherit from this class.
    /// "IDisposable" is a C# interface that provides a Dispose() method for cleanup.
    /// </summary>
    public sealed class Context : IDisposable
    {
        // Dictionary = a collection of key-value pairs. Here the key is a Type (like "Timer")
        // and the value is the actual object instance. This maps each type to one object.
        private readonly Dictionary<Type, object> _objectsMap;

        /// <summary>
        /// Creates a new empty Context. Registers itself so other code can retrieve the Context too.
        /// The "100" is an initial capacity hint -- it pre-allocates space for up to 100 entries.
        /// </summary>
        public Context()
        {
            _objectsMap = new Dictionary<Type, object>(100);
            // typeof(Context) gets the Type object representing this class at compile time.
            _objectsMap[typeof(Context)] = this;
        }

        /// <summary>
        /// Creates a child Context that copies all registrations from a parent Context.
        /// Useful for creating scoped sub-containers (e.g., per-level services).
        /// </summary>
        public Context(Context parent)
        {
            // Copies all entries from the parent's dictionary into this new one
            _objectsMap = new Dictionary<Type, object>(parent._objectsMap);
            // Override the Context entry so it points to THIS context, not the parent
            _objectsMap[typeof(Context)] = this;
        }

        /// <summary>
        /// Cleans up all registered objects. If an object implements IDisposable,
        /// its Dispose() method is called. This prevents memory leaks.
        /// </summary>
        public void Dispose()
        {
            foreach (var item in _objectsMap)
            {
                // Skip disposing ourselves to avoid infinite recursion
                if (this == item.Value)
                    continue;

                // "is" checks if an object implements an interface or is a certain type
                if (item.Value is IDisposable)
                {
                    // "as" safely casts the object to IDisposable (returns null if it fails)
                    (item.Value as IDisposable).Dispose();
                }
            }
            _objectsMap.Clear();
        }

        /// <summary>
        /// Registers one or more objects into the container, keyed by their actual type.
        /// "params" lets you pass any number of arguments: Install(obj1, obj2, obj3).
        /// After this, other code can retrieve these objects using Get&lt;T&gt;().
        /// </summary>
        public void Install(params object[] objects)
        {
            foreach (object obj in objects)
            {
                // GetType() returns the runtime type of the object (e.g., typeof(Timer))
                _objectsMap[obj.GetType()] = obj;
            }
        }

        /// <summary>
        /// Registers an object under a specific type (useful when you want to register
        /// a concrete class under its interface type, e.g., register MyTimer as ITimer).
        /// </summary>
        public void InstallByType(object obj, Type type)
        {
            _objectsMap[type] = obj;
        }

        /// <summary>
        /// After all objects are installed, this method injects dependencies into each one.
        /// It retrieves the Injector from the container and tells it to fill [Inject] fields
        /// on every registered object.
        /// </summary>
        public void ApplyInstall()
        {
            var injector = Get<Injector>();
            foreach (object obj in _objectsMap.Values)
            {
                injector.Inject(obj);
            }
        }

        /// <summary>
        /// Removes objects from the container so they are no longer available for injection.
        /// </summary>
        public void Uninstall(params object[] objects)
        {
            foreach (object obj in objects)
            {
                _objectsMap.Remove(obj.GetType());
            }
        }

        /// <summary>
        /// FIX #4: Log missing dependencies in all builds, not just editor.
        /// Previously used #if UNITY_EDITOR, so production builds got a cryptic cast exception
        /// with no useful message. Now Debug.LogError fires in all builds (visible in device
        /// logs and crash reporters), followed by a clear KeyNotFoundException.
        /// Also uses TryGetValue for single-lookup efficiency instead of ContainsKey + indexer.
        /// </summary>
        public T Get<T>() where T : class
        {
            if (_objectsMap.TryGetValue(typeof(T), out var obj))
                return (T)obj;

            Debug.LogError("[Context] Dependency not found: " + typeof(T));
            throw new KeyNotFoundException("Not found " + typeof(T));
        }

        /// <summary>
        /// FIX #4: Non-generic version — same fix as above.
        /// </summary>
        public object Get(Type type)
        {
            if (_objectsMap.TryGetValue(type, out var obj))
                return obj;

            Debug.LogError("[Context] Dependency not found: " + type);
            throw new KeyNotFoundException("Not found " + type);
        }
    }
}
