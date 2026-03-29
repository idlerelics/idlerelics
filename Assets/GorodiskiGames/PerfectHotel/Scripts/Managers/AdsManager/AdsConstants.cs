namespace Game
{
    /// <summary>
    /// Ad unit IDs for Google AdMob and Unity Ads.
    ///
    /// Each ad network requires unique IDs for each ad format (banner, interstitial, rewarded)
    /// and each platform (iOS, Android). Test IDs are provided by AdMob for development --
    /// they serve test ads that don't generate revenue or violate ad policies.
    ///
    /// IMPORTANT: Production IDs (non-Test) are empty strings here and need to be filled in
    /// with real IDs from your AdMob/Unity Ads dashboard before release.
    /// </summary>
    public static class AdsConstants
    {
        // ---- AdMob iOS Test IDs (safe to use during development) ----
        public const string AdMobInterstitialIDiOSTest = "ca-app-pub-3940256099942544/4411468910";
        public const string AdMobBannerIDiOSTest = "ca-app-pub-3940256099942544/2934735716";
        public const string AdMobRewardedIDiOSTest = "ca-app-pub-3940256099942544/1712485313";

        // ---- AdMob Android Test IDs ----
        public const string AdMobInterstitialIDAndroidTest = "ca-app-pub-3940256099942544/1033173712";
        public const string AdMobBannerIDAndroidTest = "ca-app-pub-3940256099942544/6300978111";
        public const string AdMobRewardedIDAndroidTest = "ca-app-pub-3940256099942544/5224354917";

        // ---- AdMob iOS Production IDs (fill in from AdMob dashboard) ----
        public const string AdMobBannerIDiOS = "";
        public const string AdMobInterstitialIDiOS = "";
        public const string AdMobRewardedIDiOS = "";

        // ---- AdMob Android Production IDs (fill in from AdMob dashboard) ----
        public const string AdMobBannerIDAndroid = "";
        public const string AdMobInterstitialIDAndroid = "";
        public const string AdMobRewardedIDAndroid = "";

        // ---- Unity Ads Game IDs (fill in from Unity Dashboard) ----
        public const string UnityAdsGameIDiOS = "";
        public const string UnityAdsGameIDAndroid = "";

        // ---- Unity Ads iOS IDs ----
        public const string UnityAdsBannerIDiOS = "";
        public const string UnityAdsInterstitialIDiOS = "";
        public const string UnityAdsRewardedIDiOS = "";

        // ---- Unity Ads Android IDs ----
        public const string UnityAdsBannerIDAndroid = "";
        public const string UnityAdsInterstitialIDAndroid = "";
        public const string UnityAdsRewardedIDAndroid = "";
    }
}
