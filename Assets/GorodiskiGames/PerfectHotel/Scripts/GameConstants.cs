using UnityEngine;

namespace Game
{
    public static class GameConstants 
    {
        public static string CashIcon = "<sprite=\"CashIcon\" index=0>";
        public static string AdsIcon = "<sprite=\"AdsIcon\" index=0>";
        public static string ClockIcon = "<sprite=\"ClockIcon\" index=0>";

        public const string kWatchAdsTimes = "watch_ads_times";
        public const string kLoginDays = "login_days";
        public const string kTargetWatchAdsTimes = "target_watch_ads_times";
        public const string kTargetLoginDays = "target_login_days";
        public const string kLoginDate = "login_date";

        public static bool IsDebugBuild()
        {
            if (Debug.isDebugBuild)
                return true;
            return false;
        }

        public static bool IsDeveloperIPad()
        {
            bool isDeveloperDevice = IsDebugBuild();

            var identifier = SystemInfo.deviceModel;
            bool isiPad = identifier.StartsWith("iPad");

            return isDeveloperDevice && isiPad;
        }
    }
}