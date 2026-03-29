using UnityEngine;

namespace Game
{
    /// <summary>
    /// Global constants and helper methods used across the game.
    ///
    /// 'static' class means you cannot create instances -- all members are accessed
    /// directly on the class name: GameConstants.CashIcon, GameConstants.IsDebugBuild(), etc.
    ///
    /// The sprite strings use TextMeshPro's rich text syntax to embed inline icons
    /// in text. The format "&lt;sprite="AtlasName" index=0&gt;" inserts a sprite from a TMP sprite asset.
    /// </summary>
    public static class GameConstants
    {
        // Inline sprite icons for use in TextMeshPro text fields
        public static string CashIcon = "<sprite=\"CashIcon\" index=0>";    // Dollar/coin icon
        public static string AdsIcon = "<sprite=\"AdsIcon\" index=0>";      // Advertisement icon
        public static string ClockIcon = "<sprite=\"ClockIcon\" index=0>";  // Clock/timer icon

        // PlayerPrefs keys for persistent storage (survive between game sessions)
        // 'const' means these are compile-time constants that can never change
        // The 'k' prefix is a naming convention for "konstant" (constant)
        public const string kWatchAdsTimes = "watch_ads_times";           // How many ads the player has watched
        public const string kLoginDays = "login_days";                    // Consecutive login days
        public const string kTargetWatchAdsTimes = "target_watch_ads_times"; // Target ad watches for rewards
        public const string kTargetLoginDays = "target_login_days";       // Target login days for rewards
        public const string kLoginDate = "login_date";                    // Last login date string

        /// <summary>
        /// Returns true if this is a development/debug build (not a release build).
        /// Debug.isDebugBuild is true when "Development Build" is checked in Unity's Build Settings.
        /// </summary>
        public static bool IsDebugBuild()
        {
            if (Debug.isDebugBuild)
                return true;
            return false;
        }

        /// <summary>
        /// Returns true if running on a developer's iPad (debug build + iPad device).
        /// Used to show developer-only tools/UI on specific test devices.
        /// SystemInfo.deviceModel returns strings like "iPad13,1" on iOS devices.
        /// </summary>
        public static bool IsDeveloperIPad()
        {
            bool isDeveloperDevice = IsDebugBuild();

            var identifier = SystemInfo.deviceModel;
            bool isiPad = identifier.StartsWith("iPad");

            return isDeveloperDevice && isiPad;
        }
    }
}
