using  UnityEngine;

namespace Utilities
{
    /// <summary>
    /// Utility methods for working with Unity Colors.
    ///
    /// Unity's Color struct has four components: r (red), g (green), b (blue), a (alpha).
    /// Alpha controls transparency: 0 = fully transparent, 1 = fully opaque.
    /// </summary>
    public static class ColorUtil
    {
        /// <summary>
        /// Returns a semi-transparent version of the color (50% alpha).
        /// Useful for "graying out" or dimming UI elements that are disabled.
        /// </summary>
        public static Color GetDisabledColor(Color c)
        {
            return new Color(c.r, c.g, c.b, .5f); // Same RGB, half alpha
        }

        /// <summary>
        /// Returns a fully opaque version of the color (100% alpha).
        /// Used to restore a color after it was dimmed with GetDisabledColor.
        /// </summary>
        public static Color GetEnabledColor(Color c)
        {
            return new Color(c.r, c.g, c.b, 1f); // Same RGB, full alpha
        }

        /// <summary>
        /// Wraps text in TextMeshPro color tags for rich text rendering.
        /// Example: ColorString("Hello", Color.red) returns "&lt;color=#FF0000FF&gt;Hello&lt;/color&gt;"
        ///
        /// ColorUtility.ToHtmlStringRGBA converts a Color to a hex string (like "FF0000FF").
        /// </summary>
        public static string ColorString(string text, Color color)
        {
            return "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">" + text + "</color>";
        }
    }
}
