using UnityEngine;

namespace Game.Utils
{
	/// <summary>
	/// A collection of easing functions for smooth animations.
	///
	/// Easing functions take a value 'k' (0 to 1, representing progress through an animation)
	/// and return a modified value that creates different motion feels:
	///
	/// - Linear: constant speed (boring but predictable)
	/// - Quadratic/Cubic/etc.: polynomial curves for acceleration/deceleration
	/// - Sinusoidal: smooth wave-based motion
	/// - Elastic: springy overshoot effect
	/// - Bounce: bouncing ball effect
	/// - Back: slight overshoot then settle
	///
	/// Each type has three variants:
	/// - In: starts slow, ends fast (acceleration)
	/// - Out: starts fast, ends slow (deceleration)
	/// - InOut: slow start, fast middle, slow end (both)
	///
	/// Usage: float easedValue = Easing.Quadratic.Out(t);
	/// where t goes from 0 to 1 over the animation duration.
	/// </summary>
	public class Easing
	{
		/// <summary>Linear: no easing, constant speed. Output equals input.</summary>
		public static float Linear(float k)
		{
			return k;
		}

		/// <summary>Quadratic easing (x^2). Gentle acceleration/deceleration.</summary>
		public class Quadratic
		{
			public static float In(float k)
			{
				return k * k;
			}

			public static float Out(float k)
			{
				return k * (2f - k);
			}

			public static float InOut(float k)
			{
				if ((k *= 2f) < 1f) return 0.5f * k * k;
				return -0.5f * ((k -= 1f) * (k - 2f) - 1f);
			}

			/// <summary>
			/// Quadratic Bezier curve with a control point 'c'.
			/// A Bezier curve is a smooth curve defined by control points.
			/// </summary>
			public static float Bezier(float k, float c)
			{
				return c * 2 * k * (1 - k) + k * k;
			}
		};

		/// <summary>Cubic easing (x^3). Stronger acceleration than quadratic.</summary>
		public class Cubic
		{
			public static float In(float k)
			{
				return k * k * k;
			}

			public static float Out(float k)
			{
				return 1f + ((k -= 1f) * k * k);
			}

			public static float InOut(float k)
			{
				if ((k *= 2f) < 1f) return 0.5f * k * k * k;
				return 0.5f * ((k -= 2f) * k * k + 2f);
			}
		};

		/// <summary>Quartic easing (x^4). Even stronger acceleration.</summary>
		public class Quartic
		{
			public static float In(float k)
			{
				return k * k * k * k;
			}

			public static float Out(float k)
			{
				return 1f - ((k -= 1f) * k * k * k);
			}

			public static float InOut(float k)
			{
				if ((k *= 2f) < 1f) return 0.5f * k * k * k * k;
				return -0.5f * ((k -= 2f) * k * k * k - 2f);
			}
		};

		/// <summary>Quintic easing (x^5). Very dramatic acceleration.</summary>
		public class Quintic
		{
			public static float In(float k)
			{
				return k * k * k * k * k;
			}

			public static float Out(float k)
			{
				return 1f + ((k -= 1f) * k * k * k * k);
			}

			public static float InOut(float k)
			{
				if ((k *= 2f) < 1f) return 0.5f * k * k * k * k * k;
				return 0.5f * ((k -= 2f) * k * k * k * k + 2f);
			}
		};

		/// <summary>Sinusoidal easing using sine/cosine. Very smooth, natural-feeling.</summary>
		public class Sinusoidal
		{
			public static float In(float k)
			{
				return 1f - Mathf.Cos(k * Mathf.PI / 2f);
			}

			public static float Out(float k)
			{
				return Mathf.Sin(k * Mathf.PI / 2f);
			}

			public static float InOut(float k)
			{
				return 0.5f * (1f - Mathf.Cos(Mathf.PI * k));
			}
		};

		/// <summary>Exponential easing using powers of 2. Very sharp acceleration.</summary>
		public class Exponential
		{
			public static float In(float k)
			{
				return k == 0f ? 0f : Mathf.Pow(1024f, k - 1f);
			}

			public static float Out(float k)
			{
				return k == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * k);
			}

			public static float InOut(float k)
			{
				if (k == 0f) return 0f;
				if (k == 1f) return 1f;
				if ((k *= 2f) < 1f) return 0.5f * Mathf.Pow(1024f, k - 1f);
				return 0.5f * (-Mathf.Pow(2f, -10f * (k - 1f)) + 2f);
			}
		};

		/// <summary>Circular easing using circle equations. Smooth, like a quarter circle.</summary>
		public class Circular
		{
			public static float In(float k)
			{
				return 1f - Mathf.Sqrt(1f - k * k);
			}

			public static float Out(float k)
			{
				return Mathf.Sqrt(1f - ((k -= 1f) * k));
			}

			public static float InOut(float k)
			{
				if ((k *= 2f) < 1f) return -0.5f * (Mathf.Sqrt(1f - k * k) - 1);
				return 0.5f * (Mathf.Sqrt(1f - (k -= 2f) * k) + 1f);
			}
		};

		/// <summary>
		/// Elastic easing -- springs past the target and oscillates back.
		/// Creates a "rubber band" feel, great for UI pop-in animations.
		/// </summary>
		public class Elastic
		{
			public static float In(float k)
			{
				if (k == 0) return 0;
				if (k == 1) return 1;
				return -Mathf.Pow(2f, 10f * (k -= 1f)) * Mathf.Sin((k - 0.1f) * (2f * Mathf.PI) / 0.4f);
			}

			public static float Out(float k)
			{
				if (k == 0) return 0;
				if (k == 1) return 1;
				return Mathf.Pow(2f, -10f * k) * Mathf.Sin((k - 0.1f) * (2f * Mathf.PI) / 0.4f) + 1f;
			}

			public static float InOut(float k)
			{
				if ((k *= 2f) < 1f) return -0.5f * Mathf.Pow(2f, 10f * (k -= 1f)) * Mathf.Sin((k - 0.1f) * (2f * Mathf.PI) / 0.4f);
				return Mathf.Pow(2f, -10f * (k -= 1f)) * Mathf.Sin((k - 0.1f) * (2f * Mathf.PI) / 0.4f) * 0.5f + 1f;
			}
		};

		/// <summary>
		/// Back easing -- overshoots slightly then comes back.
		/// The 's' constant controls how much overshoot (1.70158 = ~10% overshoot).
		/// Great for buttons and UI elements that "pull back before popping."
		/// </summary>
		public class Back
		{
			static float s = 1.70158f;
			static float s2 = 2.5949095f;

			public static float In(float k)
			{
				return k * k * ((s + 1f) * k - s);
			}

			public static float Out(float k)
			{
				return (k -= 1f) * k * ((s + 1f) * k + s) + 1f;
			}

			public static float InOut(float k)
			{
				if ((k *= 2f) < 1f) return 0.5f * (k * k * ((s2 + 1f) * k - s2));
				return 0.5f * ((k -= 2f) * k * ((s2 + 1f) * k + s2) + 2f);
			}
		};

		/// <summary>
		/// Bounce easing -- simulates a bouncing ball.
		/// Uses multiple segments (like a ball hitting the ground multiple times,
		/// each bounce smaller than the last). Great for items landing.
		/// </summary>
		public class Bounce
		{
			public static float In(float k)
			{
				return 1f - Out(1f - k); // Bounce In is just Bounce Out reversed
			}

			public static float Out(float k)
			{
				// Multiple segments simulate progressively smaller bounces
				if (k < (1f / 2.75f))
				{
					return 7.5625f * k * k;
				}
				else if (k < (2f / 2.75f))
				{
					return 7.5625f * (k -= (1.5f / 2.75f)) * k + 0.75f;
				}
				else if (k < (2.5f / 2.75f))
				{
					return 7.5625f * (k -= (2.25f / 2.75f)) * k + 0.9375f;
				}
				else
				{
					return 7.5625f * (k -= (2.625f / 2.75f)) * k + 0.984375f;
				}
			}

			public static float InOut(float k)
			{
				if (k < 0.5f) return In(k * 2f) * 0.5f;
				return Out(k * 2f - 1f) * 0.5f + 0.5f;
			}
		};
	}
}
