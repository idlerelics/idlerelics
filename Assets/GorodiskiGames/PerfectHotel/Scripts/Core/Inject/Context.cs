using System;
using System.Collections.Generic;

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
        /// Retrieves an object of type T from the container.
        ///
        /// This uses "generics" -- the &lt;T&gt; is a placeholder for any type you specify when calling,
        /// e.g., Get&lt;Timer&gt;() returns a Timer.
        /// "where T : class" is a constraint meaning T must be a reference type (not int, float, etc.).
        /// </summary>
        public T Get<T>() where T : class
        {
            // #if UNITY_EDITOR means this code only runs inside the Unity Editor (not in builds).
            // It adds a helpful error message during development.
#if UNITY_EDITOR
            if (!_objectsMap.ContainsKey(typeof(T)))
            {
                throw new KeyNotFoundException("Not found " + typeof(T));
            }
#endif

            // (T) is a "cast" -- it converts the stored object back to the expected type T
            return (T)_objectsMap[typeof(T)];
        }

        /// <summary>
        /// Non-generic version of Get. Takes a Type parameter instead of a generic &lt;T&gt;.
        /// Returns "object" because the exact type isn't known at compile time.
        /// </summary>
        public object Get(Type type)
        {
#if UNITY_EDITOR
            if (!_objectsMap.ContainsKey(type))
            {
                throw new KeyNotFoundException("Not found " + type);
            }
#endif
            return _objectsMap[type];
        }
    }
}
