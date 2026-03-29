using System;

namespace Injection
{
    /// <summary>
    /// A custom C# "attribute" used to mark fields that should be automatically filled in
    /// by the dependency injection system (the Injector class).
    ///
    /// WHAT IS AN ATTRIBUTE?
    /// Attributes are like tags/labels you put on code (fields, methods, classes) using [SquareBrackets].
    /// They don't change the code's behavior on their own, but other code can read them using
    /// "reflection" and act on them. Here, the Injector reads [Inject] to know which fields to fill.
    ///
    /// HOW TO USE:
    /// Put [Inject] above a private field, and the Injector will automatically assign it:
    ///   [Inject] private Timer _timer;   // The Injector will fill this with the Timer from the Context
    ///
    /// WHAT THE AttributeUsage PARAMETERS MEAN:
    /// - AttributeTargets.Field | Property | Method | Constructor = where this attribute can be placed
    /// - AllowMultiple = false = you can only put one [Inject] per field
    /// - Inherited = true = child classes also inherit the [Inject] tag from parents
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = true)]
    public class Inject : Attribute
    {
        // This class is intentionally empty -- it's just a marker/label.
        // The Injector class looks for fields tagged with [Inject] and fills them in.
    }
}
