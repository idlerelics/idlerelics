using System;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// A custom timer that tracks time, delta time, and time scale for the game.
    /// It fires events (TICK, POST_TICK, FIXED_TICK, ONE_SECOND_TICK) that other
    /// systems can subscribe to in order to update themselves each frame.
    ///
    /// "sealed" means no class can inherit from Timer.
    ///
    /// This is NOT a MonoBehaviour -- something else (likely a MonoBehaviour) must call
    /// Update(), LateUpdate(), and FixedUpdate() on this Timer each frame.
    /// </summary>
    public sealed class Timer
    {
        // OneListener is a custom safe event list (see OneListener.cs).
        // "readonly" means the reference can only be set here or in the constructor.
        private readonly OneListener _tickListener = new OneListener();

        /// <summary>
        /// Fires every frame during Update. Subscribe to do per-frame logic:
        ///   timer.TICK += MyUpdateMethod;
        ///
        /// "event Action" is C#'s built-in delegate type for methods with no parameters
        /// and no return value. The custom add/remove accessors use OneListener for safe
        /// subscription management (prevents duplicate subscriptions).
        /// </summary>
        public event Action TICK
        {
            add { _tickListener.Add(value); }
            remove { _tickListener.Remove(value); }
        }

        private readonly OneListener _postTickListener = new OneListener();

        /// <summary>
        /// Fires every frame during LateUpdate (after all Update calls are done).
        /// Useful for things like camera follow that should happen after movement.
        /// </summary>
        public event Action POST_TICK
        {
            add { _postTickListener.Add(value); }
            remove { _postTickListener.Remove(value); }
        }

        private readonly OneListener _fixedTickListener = new OneListener();

        /// <summary>
        /// Fires during FixedUpdate (at a fixed time interval, used for physics).
        /// </summary>
        public event Action FIXED_TICK
        {
            add { _fixedTickListener.Add(value); }
            remove { _fixedTickListener.Remove(value); }
        }

        private readonly OneListener _oneSecondTickListener = new OneListener();

        /// <summary>
        /// Fires approximately once per second. Useful for UI countdowns, periodic saves, etc.
        /// </summary>
        public event Action ONE_SECOND_TICK
        {
            add { _oneSecondTickListener.Add(value); }
            remove { _oneSecondTickListener.Remove(value); }
        }

        private float _unscaledTime;   // Total elapsed time ignoring TimeScale
        private float _lastTime;        // The time value from the previous frame
        private float _deltaTime;       // Time elapsed since last frame (scaled by TimeScale)
        private float _scaleTime;       // Speed multiplier for time (1 = normal, 0.5 = half speed)
        private float _time;            // Total elapsed time (affected by TimeScale)

        public Timer()
        {
            _lastTime = GetTime();
            _scaleTime = 1f;    // 1f means "1.0 as a float" -- the "f" suffix makes it a float
            _deltaTime = 0f;
            _time = 0f;
        }

        // Properties: these provide controlled access to private fields.
        // "get { return _time; }" is a read-only property (no set).
        public float Time { get { return _time; } }
        public float DeltaTime { get { return _deltaTime; } }

        // TimeScale has both get AND set. Math.Max(0f, value) prevents negative time scale.
        public float TimeScale { get { return _scaleTime; } set { _scaleTime = Math.Max(0f, value); } }
        public float UnscaladeTime { get { return _unscaledTime; } }

        /// <summary>
        /// Must be called every frame (usually from a MonoBehaviour's Update method).
        /// Calculates delta time, updates total time, and fires TICK and ONE_SECOND_TICK events.
        /// </summary>
        public void Update()
        {
            var now = GetTime();
            var delta = now - _lastTime;            // Raw time since last frame
            _unscaledTime += delta;                  // Accumulate unscaled time
            _deltaTime = delta * TimeScale;          // Apply time scale to delta
            _time += _deltaTime;                     // Accumulate scaled time

            // Check if we crossed a second boundary (e.g., 2.9 -> 3.1)
            // Mathf.Floor rounds down to the nearest integer
            bool isNewSecondTick = Mathf.Floor(now) > Mathf.Floor(_lastTime);

            _lastTime = now;

            // Notify all TICK subscribers
            _tickListener.Invoke();

            // Notify ONE_SECOND_TICK subscribers (only once per second)
            if (isNewSecondTick)
            {
                _oneSecondTickListener.Invoke();
            }
        }

        /// <summary>
        /// Must be called every frame from LateUpdate. Fires the POST_TICK event.
        /// </summary>
        public void LateUpdate()
        {
            _postTickListener.Invoke();
        }

        /// <summary>
        /// Must be called from FixedUpdate. Fires the FIXED_TICK event.
        /// </summary>
        public void FixedUpdate()
        {
            _fixedTickListener.Invoke();
        }

        /// <summary>
        /// Returns the current system time in seconds.
        /// Environment.TickCount gives milliseconds since the system started;
        /// dividing by 1000f converts to seconds.
        /// </summary>
        private float GetTime()
        {
            return Environment.TickCount / 1000f;
        }
    }
}
