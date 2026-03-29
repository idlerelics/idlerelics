using System;
using Game.Config;
using Game.Core;
using Injection;
using Utilities;



namespace Game.Managers
{
    // An 'enum' (short for "enumeration") defines a fixed set of named options.
    // Instead of using magic strings like "Fake" or "AdMob", enums give you
    // type-safe, autocomplete-friendly named constants.
    /// <summary>
    /// The available ad provider backends. Fake is used for testing without real ads.
    /// </summary>
    public enum AdsProviderType
    {
        Fake,   // A mock/fake ad provider for testing (no real ads shown)
        AdMob   // Google AdMob -- a real ad network for monetization
    }

    /// <summary>
    /// Defines where/why an ad is being shown. Helps track which game feature triggered the ad.
    /// </summary>
    public enum AdsPlacement
    {
        Default,                  // General/default ad placement
        CashForAds,              // Player watches an ad to earn cash
        CashForAdsEquipmentHud,  // Player watches an ad for cash from the equipment screen
        EquipmentForAds          // Player watches an ad to get equipment
    }

    /// <summary>
    /// Manages all advertising in the game (banner ads, interstitial ads, rewarded video ads).
    ///
    /// Implements 'IDisposable' -- this is a C# interface that promises the class has a Dispose()
    /// method for cleanup. It is a standard pattern for classes that hold resources (like ad connections)
    /// that need to be properly released when no longer needed.
    ///
    /// This class uses the "Proxy" pattern: it delegates actual ad logic to a BaseAdsProxy,
    /// which can be either a real ad provider (GoogleAdMobProxy) or a fake one (FakeAdsProxy).
    /// </summary>
    public sealed class AdsManager : IDisposable
    {
        // 'Action' is a C# delegate type -- essentially a variable that holds a reference to a method.
        // Other classes can "subscribe" to these Actions to be notified when something happens.
        // This is called the "Observer" or "Event" pattern.
        // Example: someObject.ON_REWARDED_WATCHED += MyRewardMethod;
        public Action ON_INTERSTITIAL_SHOW;           // Fired when an interstitial ad starts showing
        public Action ON_INTERSTITIAL_WATCHED;        // Fired when an interstitial ad finishes
        public Action ON_INTERSTITIAL_LOADED;         // Fired when an interstitial ad is loaded and ready
        public Action ON_INTERSTITIAL_FAILED_TO_LOAD; // Fired when an interstitial ad fails to load

        public Action ON_REWARDED_WATCHED;            // Fired when a rewarded video ad finishes

        [Inject] private Timer _timer; // Injected timer used by the ad proxy for delays/cooldowns

        private BaseAdsProxy _adsProxy; // The actual ad provider implementation (real or fake)
        private GameConfig _config;     // Game configuration containing ad provider settings

        private bool _isNoAds; // True if the player purchased "Remove Ads" (disables interstitials & banners)

        /// <summary>
        /// Sets up the ad system. Chooses the right ad provider based on the platform.
        /// </summary>
        /// <param name="isNoAds">Whether the player has purchased the "no ads" option</param>
        /// <param name="config">The game configuration with ad provider settings</param>
        public void Initialize(bool isNoAds, GameConfig config)
        {
            _isNoAds = isNoAds;
            _config = config;

            // '#if UNITY_EDITOR' is a "preprocessor directive" -- it runs different code depending
            // on the platform. Code inside #if/#elif/#else/#endif is included or excluded at compile time.
            // UNITY_EDITOR: running inside the Unity Editor (for development/testing)
            // UNITY_WEBGL / UNITY_STANDALONE: running as a web or desktop build
            // The 'else' block covers mobile platforms (iOS, Android)
#if UNITY_EDITOR
            var provider = _config.AdsProviderEditor;       // Use the editor-configured provider
#elif UNITY_WEBGL || UNITY_STANDALONE
            var provider = AdsProviderType.Fake;            // Web/desktop always use fake ads
#else
            var provider = _config.AdsProviderMobile;       // Mobile uses the mobile-configured provider
#endif

            // Create the appropriate ad proxy based on the selected provider
            if (provider == AdsProviderType.AdMob)
                _adsProxy = new GoogleAdMobProxy(_timer);   // Real Google AdMob ads
            else
                _adsProxy = new FakeAdsProxy(_timer);       // Fake ads for testing

            // '+=' subscribes to events -- when the proxy fires these events, our methods get called.
            // This is how the AdsManager listens for ad lifecycle events from the proxy.
            _adsProxy.ON_REWARDED_WATCHED += OnRewardedWatched;

            _adsProxy.ON_INTERSTITIAL_WATCHED += OnInterstitialWatched;
            _adsProxy.ON_INTERSTITIAL_SHOW += OnInterstitialShow;

            _adsProxy.INITIALIZED += OnInitialized;
            _adsProxy.Initialize();
        }

        /// <summary>
        /// Cleans up the ad system: unloads banners, unsubscribes from events, and disposes the proxy.
        /// '-=' unsubscribes from events -- the opposite of '+='.
        /// Always unsubscribe to prevent memory leaks and errors from calling destroyed objects.
        /// </summary>
        public void Dispose()
        {
            _adsProxy.UnloadBanner();

            _adsProxy.ON_REWARDED_WATCHED -= OnRewardedWatched;

            _adsProxy.ON_INTERSTITIAL_WATCHED -= OnInterstitialWatched;
            _adsProxy.ON_INTERSTITIAL_SHOW -= OnInterstitialShow;

            _adsProxy.INITIALIZED -= OnInitialized;
            _adsProxy.Dispose();
        }

        /// <summary>
        /// Called when the ad SDK finishes initializing.
        /// Pre-loads all ad types so they are ready to show quickly when needed.
        /// </summary>
        private void OnInitialized()
        {
            _adsProxy.LoadRewarded();     // Pre-load a rewarded video ad
            _adsProxy.LoadInterstitial(); // Pre-load an interstitial (full-screen) ad
            _adsProxy.LoadBanner();       // Pre-load a banner ad

            ShowBanner(); // Show the banner ad immediately
        }

        /// <summary>
        /// Shows a full-screen interstitial ad (the kind that covers the whole screen between actions).
        /// Skipped if the player has purchased "no ads".
        /// Wrapped in try/catch to prevent the game from crashing if the ad system fails.
        /// </summary>
        public void ShowInterstitial()
        {
            if (_isNoAds)
                return; // Player paid to remove ads, so skip

            // 'try/catch' is error handling: if the code in 'try' throws an error (exception),
            // the 'catch' block runs instead of crashing the game.
            try
            {
                _adsProxy.ShowInterstitial();
            }
            catch (Exception exception)
            {
                Log.Exception(exception); // Log the error for debugging
            }
        }

        /// <summary>
        /// Shows a rewarded video ad (player chooses to watch in exchange for a reward).
        /// Note: rewarded ads are NOT blocked by _isNoAds because the player opts in voluntarily.
        /// </summary>
        public void ShowRewarded()
        {
            try
            {
                _adsProxy.ShowRewarded();
            }
            catch (Exception exception)
            {
                Log.Exception(exception);
            }
        }

        /// <summary>
        /// Shows a small banner ad (usually at the top or bottom of the screen).
        /// Skipped if the player has purchased "no ads".
        /// </summary>
        public void ShowBanner()
        {
            if (_isNoAds)
                return;

            try
            {
                _adsProxy.ShowBanner();
            }
            catch (Exception exception)
            {
                Log.Exception(exception);
            }
        }

        /// <summary>
        /// Hides the banner ad. Can be called even with "no ads" active (e.g., during transitions).
        /// </summary>
        public void HideBanner()
        {
            try
            {
                _adsProxy.HideBanner();
            }
            catch (Exception exception)
            {
                Log.Exception(exception);
            }
        }

        /// <summary>
        /// Callback: triggered when a rewarded video ad has been fully watched.
        /// SafeInvoke is a helper that calls the Action only if it has subscribers (prevents null errors).
        /// </summary>
        private void OnRewardedWatched()
        {
            Log.Info($"Rewarded watched");
            ON_REWARDED_WATCHED.SafeInvoke(); // Notify all subscribers that the reward was earned
        }

        /// <summary>
        /// Callback: triggered when an interstitial ad has been fully watched/dismissed.
        /// </summary>
        private void OnInterstitialWatched()
        {
            Log.Info($"Interstitial watched");
            ON_INTERSTITIAL_WATCHED.SafeInvoke();
        }

        /// <summary>
        /// Callback: triggered when an interstitial ad starts showing on screen.
        /// </summary>
        private void OnInterstitialShow()
        {
            Log.Info($"Interstitial show");
            ON_INTERSTITIAL_SHOW.SafeInvoke();
        }

        /// <summary>
        /// Activates the "no ads" mode (e.g., after the player purchases ad removal).
        /// Immediately hides any visible banner ad.
        /// </summary>
        public void SetNoAds()
        {
            _isNoAds = true;
            HideBanner();
        }
    }
}
