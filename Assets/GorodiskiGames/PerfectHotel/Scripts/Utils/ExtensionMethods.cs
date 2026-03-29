using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Utilities
{
    /// <summary>
    /// Utility extension methods for collections (lists, arrays, etc.).
    ///
    /// Extension methods add new functionality to existing types without modifying them.
    /// The 'this' keyword before the first parameter makes these callable as instance methods:
    ///   myList.GetRandom()  instead of  ExtensionMethods.GetRandom(myList)
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns a random element from the collection using Unity's Random.
        /// Returns default(T) (null for objects, 0 for numbers) if the collection is empty.
        ///
        /// 'T' is a generic type parameter -- this method works with any type of collection:
        /// List&lt;string&gt;, int[], IEnumerable&lt;Color&gt;, etc.
        /// </summary>
        public static T GetRandom<T>(this IEnumerable<T> collection)
        {
            var count = collection.Count();
            if (count != 0)
            {
                // UnityEngine.Random.Range(min, max) returns a random int from min (inclusive) to max (exclusive)
                return collection.ElementAt(UnityEngine.Random.Range(0, count));
            }
            return default(T);
        }

        /// <summary>
        /// Returns a random element using a System.Random instance.
        /// Useful when you need reproducible randomness (by seeding the Random).
        /// </summary>
        public static T GetRandom<T>(this IEnumerable<T> collection, Random random)
        {
            var count = collection.Count();
            if (count != 0)
            {
                return collection.ElementAt(random.Next(0, count));
            }
            return default(T);
        }

        /// <summary>
        /// Returns the last N elements from a collection.
        /// Uses a Queue as a sliding window -- as elements are added, old ones
        /// are removed when the queue exceeds the desired count.
        ///
        /// This is useful because LINQ's built-in methods don't have a TakeLast
        /// (it was added in .NET Core but not available in Unity's older .NET).
        /// </summary>
        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            Queue<T> lastElements = new Queue<T>();
            foreach (T element in source)
            {
                lastElements.Enqueue(element);          // Add to the queue
                if (lastElements.Count > count)
                {
                    lastElements.Dequeue();             // Remove oldest if over limit
                }
            }

            return lastElements;
        }
    }
}
