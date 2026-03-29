using System;
using UnityEngine;


namespace Utilities
{
    /// <summary>
    /// Utility for finding the intersection point of two 2D line segments.
    ///
    /// Uses the standard slope-intercept form (y = mx + c) to solve for the intersection.
    /// Handles special cases like vertical lines (infinite slope) and parallel lines.
    ///
    /// The 'checkOnInside' parameter controls whether the intersection must be
    /// within both line segments (true) or can be anywhere on the infinite lines (false).
    ///
    /// This is used for navigation and collision detection in the game world.
    /// </summary>
    public static class LineIntersection
    {
        public const float kTolerance = 0.01f; // Floating-point comparison tolerance

        /// <summary>
        /// Overload that accepts Vector2 points for convenience.
        /// Delegates to the float-parameter version.
        /// </summary>
        public static bool FindIntersection(Vector2 point1, Vector2 point2,
            float x3, float y3, float x4, float y4, bool checkOnInside, out Vector2 point, float tolerance = kTolerance)
        {
            return FindIntersection(point1.x, point1.y, point2.x, point2.y, x3, y3, x4, y4, checkOnInside, out point,
            tolerance);
        }

        /// <summary>
        /// Finds the intersection point of two line segments defined by endpoints.
        /// Line 1: (x1,y1) to (x2,y2)
        /// Line 2: (x3,y3) to (x4,y4)
        ///
        /// Returns true if an intersection exists, with the point in 'out point'.
        /// Returns false if the lines are parallel, overlapping, or don't intersect
        /// within the segments (when checkOnInside is true).
        ///
        /// The math uses the slope-intercept form:
        ///   y = mx + c  where m = slope = (y2-y1)/(x2-x1), c = y-intercept
        /// Two lines intersect where: m1*x + c1 = m2*x + c2
        /// Solving for x: x = (c1 - c2) / (m2 - m1)
        /// </summary>
        public static bool FindIntersection(float x1, float y1, float x2, float y2,
            float x3, float y3, float x4, float y4, bool checkOnInside, out Vector2 point, float tolerance = kTolerance)
        {
            // Special case: both lines are vertical and overlap (ambiguous)
            if (Math.Abs(x1 - x2) < tolerance && Math.Abs(x3 - x4) < tolerance && Math.Abs(x1 - x3) < tolerance)
            {
                point = Vector2.zero;
                return false;
            }

            // Special case: both lines are horizontal and overlap (ambiguous)
            if (Math.Abs(y1 - y2) < tolerance && Math.Abs(y3 - y4) < tolerance && Math.Abs(y1 - y3) < tolerance)
            {
                point = Vector2.zero;
                return false;
            }

            // Two vertical lines that don't overlap -- parallel, no intersection
            if (Math.Abs(x1 - x2) < tolerance && Math.Abs(x3 - x4) < tolerance)
            {
                point = Vector2.zero;
                return false;
            }

            // Two horizontal lines that don't overlap -- parallel, no intersection
            if (Math.Abs(y1 - y2) < tolerance && Math.Abs(y3 - y4) < tolerance)
            {
                point = Vector2.zero;
                return false;
            }

            float x, y;

            // Line A is vertical (x1 == x2) -- can't compute slope, use special formula
            if (Math.Abs(x1 - x2) < tolerance)
            {
                float m2 = (y4 - y3) / (x4 - x3); // Slope of line 2
                float c2 = -m2 * x3 + y3;          // Y-intercept of line 2

                // Vertical line: x is fixed, substitute into line 2's equation
                x = x1;
                y = c2 + m2 * x1;
            }
            // Line B is vertical (x3 == x4) -- same special case for the other line
            else if (Math.Abs(x3 - x4) < tolerance)
            {
                float m1 = (y2 - y1) / (x2 - x1);
                float c1 = -m1 * x1 + y1;

                x = x3;
                y = c1 + m1 * x3;
            }
            // General case: neither line is vertical
            else
            {
                float m1 = (y2 - y1) / (x2 - x1); // Slope of line 1
                float c1 = -m1 * x1 + y1;          // Y-intercept of line 1

                float m2 = (y4 - y3) / (x4 - x3); // Slope of line 2
                float c2 = -m2 * x3 + y3;          // Y-intercept of line 2

                // Solve: x = (c1 - c2) / (m2 - m1)
                x = (c1 - c2) / (m2 - m1);
                y = c2 + m2 * x;

                // Verify the solution is valid (finite and satisfies both equations)
                if (!(Math.Abs(-m1 * x + y - c1) < tolerance
                    && Math.Abs(-m2 * x + y - c2) < tolerance))
                {
                    point = Vector2.zero;
                    return false;
                }
            }

            // If checkOnInside, verify the intersection point is within both line segments
            // (not on the infinite extension of the lines)
            if (!checkOnInside || (IsInsideLine(x1, y1, x2, y2, x, y, tolerance) &&
                IsInsideLine(x3, y3, x4, y4, x, y, tolerance)))
            {
                point = new Vector2() { x = x, y = y };
                return true;
            }

            point = Vector2.zero;
            return false;

        }

        /// <summary>
        /// Returns true if point (x,y) lies within the bounding box of line segment (x1,y1)-(x2,y2).
        /// Uses tolerance 't' for floating-point comparison.
        /// </summary>
        private static bool IsInsideLine(float x1, float y1, float x2, float y2, float x, float y, float t)
        {
            return (x >= x1 - t && x <= x2 + t
                        || x >= x2 - t && x <= x1 + t)
                   && (y >= y1 - t && y <= y2 + t
                        || y >= y2 - t && y <= y1 + t);
        }
    }
}
