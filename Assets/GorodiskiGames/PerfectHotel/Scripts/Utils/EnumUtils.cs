using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities
{
    /// <summary>
    /// Helper methods for working with C# enums.
    ///
    /// Enums are named sets of constants (like ItemType.Clean, ItemType.BuyUpdate).
    /// These utilities provide generic methods for counting, listing, and parsing enum values.
    ///
    /// 'Generic methods' (with &lt;T&gt;) work with any enum type, so you don't need
    /// to write separate methods for ItemType, InventoryType, etc.
    /// </summary>
    public class EnumUtils
    {
        // Cache dictionary: stores the count of each enum type so we only calculate it once.
        // 'Type' is C#'s representation of a type at runtime (typeof(ItemType), typeof(InventoryType), etc.).
        private static readonly Dictionary<Type, int> EnumCount = new Dictionary<Type, int>();

        /// <summary>
        /// Returns how many values an enum type has.
        /// Caches the result to avoid repeated reflection calls (which are slow).
        ///
        /// typeof(T) gets the Type object for T at runtime.
        /// Enum.GetValues returns an array of all values defined in the enum.
        /// </summary>
        public static int GetCount<T>()
        {
            int result = 0;
            var type = typeof(T);
            if (type.IsEnum)
            {
                // TryGetValue returns true if the key exists, avoiding duplicate lookups
                if (!EnumCount.TryGetValue(type, out result))
                {
                    result = Enum.GetValues(type).Length;
                    EnumCount.Add(type, result); // Cache for next time
                }
            }
            return result;
        }

        /// <summary>
        /// Returns all values of an enum type as a typed array.
        /// Example: EnumUtils.GetValues&lt;ItemType&gt;() returns [Clean, ReceptionDesk, BuyUpdate, ShowHud]
        ///
        /// .Cast&lt;TEnum&gt;() converts each element from object to the enum type.
        /// .ToArray() materializes the result into an array.
        /// </summary>
        public static TEnum[] GetValues<TEnum>()
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray<TEnum>();
        }

        /// <summary>
        /// Parses a value (usually a string) into an enum value.
        /// Example: EnumUtils.Parse&lt;ItemType&gt;("Clean") returns ItemType.Clean
        /// Throws an exception if the value doesn't match any enum member.
        /// </summary>
        public static TEnum Parse<TEnum>(object value)
        {
            return (TEnum)Enum.Parse(typeof(TEnum), value.ToString());
        }

        /// <summary>
        /// Tries to parse a value into an enum, returning true/false instead of throwing.
        /// 'out' parameter passes the result back to the caller.
        /// Safer than Parse when the input might not be a valid enum value.
        /// </summary>
        public static bool TryParse<TEnum>(object value, out TEnum result)
        {
            if (Enum.IsDefined(typeof(TEnum), value))
            {
                result = (TEnum)Enum.Parse(typeof(TEnum), value.ToString());
                return true;
            }

            result = default(TEnum);
            return false;
        }
    }
}
