using System;

namespace Utilities
{
    /// <summary>
    /// Extension methods for safely invoking Action delegates.
    ///
    /// In C#, calling an event/delegate that has no subscribers (is null) causes a
    /// NullReferenceException crash. SafeInvoke checks for null before invoking,
    /// preventing these crashes.
    ///
    /// "Extension methods" are a C# feature that lets you add methods to existing types
    /// without modifying them. The 'this' keyword before the first parameter makes it
    /// callable as if it were a method on that type:
    ///   myAction.SafeInvoke();  // instead of: ActionExtentions.SafeInvoke(myAction);
    ///
    /// Modern C# has the ?. (null-conditional) operator: myAction?.Invoke();
    /// But SafeInvoke also provides overloads with exception handling (try/catch).
    /// </summary>
    public static class ActionExtentions
    {
        /// <summary>Safely invokes an Action, checking for null first.</summary>
        public static void SafeInvoke(this Action invocationTarget)
        {
            if (null != invocationTarget)
            {
                invocationTarget.Invoke();
            }
        }

        /// <summary>Safely invokes an Action with one parameter.</summary>
        public static void SafeInvoke<T>(this Action<T> invocationTarget, T arg)
        {
            if (null != invocationTarget)
            {
                invocationTarget.Invoke(arg);
            }
        }

        /// <summary>Safely invokes an Action with two parameters.</summary>
        public static void SafeInvoke<T1, T2>(this Action<T1, T2> invocationTarget, T1 arg1, T2 arg2)
        {
            if (null != invocationTarget)
            {
                invocationTarget.Invoke(arg1, arg2);
            }
        }

        /// <summary>Safely invokes an Action with three parameters.</summary>
        public static void SafeInvoke<T1, T2, T3>(this Action<T1, T2, T3> invocationTarget, T1 arg1, T2 arg2, T3 arg3)
        {
            if (null != invocationTarget)
            {
                invocationTarget.Invoke(arg1, arg2, arg3);
            }
        }

        /// <summary>Safely invokes an Action with four parameters.</summary>
        public static void SafeInvoke<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> invocationTarget, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (null != invocationTarget)
            {
                invocationTarget.Invoke(arg1, arg2, arg3, arg4);
            }
        }

        /// <summary>
        /// Safely invokes an Action with exception handling.
        /// If the action throws, the exception is caught and passed to the exceptionAction
        /// callback instead of crashing the application.
        /// </summary>
        public static void SafeInvoke(this Action invocationTarget, Action<Exception> exceptionAction)
        {
            if (null != invocationTarget)
            {
                try
                {
                    invocationTarget.Invoke();
                }
                catch (Exception exception)
                {
                    exceptionAction(exception);
                }
            }
        }

        /// <summary>Safely invokes with one parameter and exception handling.</summary>
        public static void SafeInvoke<T>(this Action<T> invocationTarget, T arg, Action<Exception> exceptionAction)
        {
            if (null != invocationTarget)
            {
                try
                {
                    invocationTarget.Invoke(arg);
                }
                catch (Exception exception)
                {
                    exceptionAction(exception);
                }
            }
        }

        /// <summary>Safely invokes with two parameters and exception handling.</summary>
        public static void SafeInvoke<T1, T2>(this Action<T1, T2> invocationTarget, T1 arg1, T2 arg2, Action<Exception> exceptionAction)
        {
            if (null != invocationTarget)
            {
                try
                {
                    invocationTarget.Invoke(arg1, arg2);
                }
                catch (Exception exception)
                {
                    exceptionAction(exception);
                }
            }
        }

        /// <summary>Safely invokes with three parameters and exception handling.</summary>
        public static void SafeInvoke<T1, T2, T3>(this Action<T1, T2, T3> invocationTarget, T1 arg1, T2 arg2, T3 arg3, Action<Exception> exceptionAction)
        {
            if (null != invocationTarget)
            {
                try
                {
                    invocationTarget.Invoke(arg1, arg2, arg3);
                }
                catch (Exception exception)
                {
                    exceptionAction(exception);
                }
            }
        }
    }
}
