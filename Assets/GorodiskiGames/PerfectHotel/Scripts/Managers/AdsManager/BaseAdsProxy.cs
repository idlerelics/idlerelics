using System;
using Game.Core;

namespace Game.Managers
{
    /// <summary>
    /// Abstract base class for ad provider implementations (Google AdMob, Unity Ads, Fake Ads, etc.).
    ///
    /// The "Proxy" pattern: this class defines the interface for showing ads, and each
    /// concrete subclass handles the actual ad SDK calls. This lets the game code work
    /// with any ad provider without knowing the details.
    ///
    /// 'abstract' methods have no body here -- subclasses MUST implement them.
    /// 'virtual' methods have a default implementation that subclasses CAN override.
    ///
    /// The lifecycle for each ad type is: Load -> Show -> (user watches) -> callback fires.
    /// </summary>
    public abstract class BaseAdsProxy : IDisposable
    {
        // Events for ad lifecycle. Other systems subscribe to know when ads are ready/watched.
        public Action INITIALIZED;                     // SDK is ready to serve ads
        public Action ON_INTERSTITIAL_SHOW;            // Interstitial is about to show
        public Action ON_INTERSTITIAL_WATCHED;         // User finished watching interstitial
        public Action ON_INTERSTITIAL_LOADED;          // Interstitial ad is loaded and ready
        public Action ON_INTERSTITIAL_FAILED_TO_LOAD;  // Interstitial failed to load
        public Action ON_REWARDED_WATCHED;             // User finished watching rewarded ad

        private readonly Timer _timer; // Used for per-frame POST_TICK updates (ad reload checks)

        protected BaseAdsProxy(Timer timer)
        {
            _timer = timer;
        }

        /// <summary>Initializes the ad SDK and starts listening for post-tick updates.</summary>
        public virtual void Initialize()
        {
            _timer.POST_TICK += OnPostTick; // Subscribe to post-frame updates
        }

        /// <summary>Cleans up by unsubscribing from the timer.</summary>
        public virtual void Dispose()
        {
            _timer.POST_TICK -= OnPostTick;
        }

        // ---- Abstract methods that each ad provider must implement ----
        public abstract void LoadRewarded();       // Start loading a rewarded ad
        public abstract void ShowRewarded();        // Display the rewarded ad
        public abstract void OnPostTick();          // Called each frame after Update (for reload timers)
        public abstract void LoadInterstitial();    // Start loading an interstitial ad
        public abstract void ShowInterstitial();    // Display the interstitial ad
        public abstract void LoadBanner();          // Start loading a banner ad
        public abstract void ShowBanner();          // Show the banner
        public abstract void HideBanner();          // Hide the banner (still loaded)
        public abstract void UnloadBanner();        // Completely remove the banner
    }
}
