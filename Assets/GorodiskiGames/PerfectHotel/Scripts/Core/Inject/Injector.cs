using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Injection
{
    /// <summary>
    /// The Injector scans an object for fields marked with [Inject] and automatically
    /// fills them with the correct instances from the Context (dependency container).
    ///
    /// This uses "reflection" -- a C# feature that lets code inspect itself at runtime,
    /// reading field names, types, and attributes. It's powerful but slower than normal code,
    /// so results are cached in _fieldsMap for performance.
    /// </summary>
    public sealed class Injector
    {
        // Cache: maps each Type to the array of fields that have [Inject] on them.
        // FieldInfo is a reflection class that describes a single field (its name, type, value, etc.).
        private readonly Dictionary<Type, FieldInfo[]> _fieldsMap;

        // Reference to the Context that holds all the registered objects
        private readonly Context _context;

        public Injector(Context context)
        {
            _context = context;
            _fieldsMap = new Dictionary<Type, FieldInfo[]>(100);
        }

        /// <summary>
        /// Takes an object, finds all its [Inject] fields, and fills each one
        /// with the matching object from the Context.
        /// For example, if a class has "[Inject] private Timer _timer;",
        /// this method will set _timer = context.Get(typeof(Timer)).
        /// </summary>
        public void Inject(object value)
        {
            if (null == value)
                return;

            // GetType() returns the actual runtime type of this object
            var type = value.GetType();

            // Find and cache all [Inject] fields for this type (only done once per type)
            TryToMapFields(type);

            // Loop through each [Inject] field and set its value from the Context
            var fields = _fieldsMap[type];
            foreach (var fieldInfo in fields)
            {
                // fieldInfo.FieldType = the type of the field (e.g., Timer)
                // fieldInfo.SetValue = assigns a value to that field on the given object
                fieldInfo.SetValue(value, _context.Get(fieldInfo.FieldType));
            }
        }

        /// <summary>
        /// Convenience method to retrieve an object from the Context.
        /// "where T : class" means T must be a reference type (not a value type like int).
        /// </summary>
        public T Get<T>() where T : class
        {
            return _context.Get<T>();
        }

        /// <summary>
        /// Uses reflection to find all private instance fields with the [Inject] attribute.
        /// Results are cached so reflection only runs once per type (for performance).
        /// </summary>
        private void TryToMapFields(Type type)
        {
            // If we already scanned this type, skip it
            if (_fieldsMap.ContainsKey(type))
                return;

            // BindingFlags control what fields to look for:
            // NonPublic = private/protected fields, Instance = non-static fields
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            // .Where() filters the array: keep only fields that have the [Inject] attribute
            // GetCustomAttributes checks if a field has a specific attribute
            fields = fields.Where(temp => temp.GetCustomAttributes(typeof(Inject), true).Length > 0).ToArray();

            _fieldsMap[type] = fields;
        }
    }
}
