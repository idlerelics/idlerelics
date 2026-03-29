using System;
using System.Collections.Generic;
using Game.Core;
using UnityEngine;
using GoogleMobileAds.Api;
using Utilities;

namespace Game.Managers
{
    /// <summary>
    /// Google AdMob implementation of the ad provider.
    /// Handles loading, showing, and reloading banner, interstitial, and rewarded ads.
    ///
    /// The reload system uses dictionaries mapping ad IDs to scheduled reload times.
    /// When an ad fails to load, it's scheduled to retry after kReloadDuration seconds.
    /// The OnPostTick method checks these schedules each frame.
    ///
    /// Ad types:
    /// - Banner: small ad at the bottom of the screen (always visible)
    /// - Interstitial: full-screen ad between game actions
    /// - Rewarded: full-screen ad the player chooses to watch for a reward
    /// </summary>
    public sealed class GoogleAdMobProxy : BaseAdsProxy
    {
        private const int kReloadDuration = 2; // Seconds to wait before retrying a failed ad load

        // Maps ad unit IDs to the Time.time when they should be reloaded
        private readonly Dictionary<string, float> _rewardedReloadTimeMap;
        private readonly Dictionary<string, float> _interstitialReloadTimeMap;
        // FIX #2: Reusable key list to avoid per-frame allocations in OnPostTick().
        // Previously, two new List<string> were created every frame just to iterate dictionary
        // keys safely during modification. This cached list is cleared and refilled instead.
        private readonly List<string> _tempKeys = new List<string>();

        // Google AdMob ad objects
        BannerView _bannerView;            // Persistent banner at screen bottom
        InterstitialAd _interstitialAd;    // Full-screen ad between actions
        RewardedAd _rewardedAd;            // Ad watched for rewards

        private bool _isDebugBuild;        // If true, use test ad IDs

        // Current ad unit IDs (set based on platform and debug mode)
        private string InterstitialID;
        private string BannerID;
        private string RewardedID;

        public GoogleAdMobProxy(Timer timer) : base(timer)
        {
            _interstitialReloadTimeMap = new Dictionary<string, float>();
            _rewardedReloadTimeMap = new Dictionary<string, float>();
        }

        /// <summary>
        /// Initializes the AdMob SDK. MobileAds.Initialize is asynchronous --
        /// it calls the callback when the SDK is ready to serve ads.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            _isDebugBuild = GameConstants.IsDebugBuild();

            // Select the correct ad IDs based on platform (iOS/Android) and build type (debug/release)
            GetIDs(out InterstitialID, out BannerID, out RewardedID);

            // Initialize the AdMob SDK (async -- callback fires when ready)
            MobileAds.Initialize(initStatus => { OnSdkInitializedEvent(); });
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        /// <summary>
        /// Selects ad unit IDs based on the current platform and build type.
        /// Debug builds use Google's test IDs (which serve test ads and don't violate policies).
        /// Production builds use real IDs from the AdMob dashboard.
        ///
        /// 'out' parameters allow this method to return multiple values.
        /// </summary>
        private void GetIDs(out string interstitialID, out string bannerID, out string rewardedID)
        {
            // Default to iOS test IDs
            interstitialID = AdsConstants.AdMobInterstitialIDiOSTest;
            bannerID = AdsConstants.AdMobBannerIDiOSTest;
            rewardedID = AdsConstants.AdMobRewardedIDiOSTest;

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                interstitialID = AdsConstants.AdMobInterstitialIDiOS;
                bannerID = AdsConstants.AdMobBannerIDiOS;
                rewardedID = AdsConstants.AdMobRewardedIDiOS;

                if (_isDebugBuild)
                {
                    interstitialID = AdsConstants.AdMobInterstitialIDiOSTest;
                    bannerID = AdsConstants.AdMobBannerIDiOSTest;
                    rewardedID = AdsConstants.AdMobRewardedIDiOSTest;
                }
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                interstitialID = AdsConstants.AdMobInterstitialIDAndroid;
                bannerID = AdsConstants.AdMobBannerIDAndroid;
                rewardedID = AdsConstants.AdMobRewardedIDAndroid;

                if (_isDebugBuild)
                {
                    interstitialID = AdsConstants.AdMobInterstitialIDAndroidTest;
                    bannerID = AdsConstants.AdMobBannerIDAndroidTest;
                    rewardedID = AdsConstants.AdMobRewardedIDAndroidTest;
                }
            }
        }

        private void OnSdkInitializedEvent()
        {
            INITIALIZED.SafeInvoke(); // Notify listeners that ads are ready
        }

        /// <summary>
        /// Called every frame after Update. Checks if any ads are scheduled for reload
        /// and loads them when their scheduled time arrives.
        ///
        /// This retry mechanism ensures ads keep loading even after failures,
        /// with a short delay (kReloadDuration) between attempts.
        /// </summary>
        public override void OnPostTick()
        {
            // Skip if nothing is scheduled for reload
            if (_interstitialReloadTimeMap.Count + _rewardedReloadTimeMap.Count == 0)
                return;

            try
            {
                // Check and reload rewarded ads
                _tempKeys.Clear();
                _tempKeys.AddRange(_rewardedReloadTimeMap.Keys);
                foreach (var key in _tempKeys)
                {
                    if (Time.time >= _rewardedReloadTimeMap[key])
                    {
                        _rewardedReloadTimeMap.Remove(key);
                        if (_rewardedAd != null)
                        {
                            _rewardedAd.Destroy(); // Clean up the old ad object
                            _rewardedAd = null;
                        }
                        Debug.Log("Loading rewarded ad. " + Time.time);
                        var adRequest = new AdRequest();
                        // RewardedAd.Load is async -- callback fires when the ad is ready or fails
                        RewardedAd.Load(key, adRequest, (RewardedAd ad, LoadAdError error) =>
                        {
                            if (error != null || ad == null)
                            {
                                Debug.LogError("Rewarded ad failed to load an ad with error : " + error);
                                OnRewardedAdLoadFailedEvent(key); // Schedule retry
                                return;
                            }
                            Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());
                            _rewardedAd = ad;
                            OnRewardedAdLoadedEvent();
                            RegisterEventHandlers(ad); // Subscribe to ad lifecycle events
                        });
                    }
                }

                // Check and reload interstitial ads (same pattern as rewarded)
                _tempKeys.Clear();
                _tempKeys.AddRange(_interstitialReloadTimeMap.Keys);
                foreach (var key in _tempKeys)
                {
                    if (Time.time >= _interstitialReloadTimeMap[key])
                    {
                        _interstitialReloadTimeMap.Remove(key);
                        if (_interstitialAd != null)
                        {
                            _interstitialAd.Destroy();
                            _interstitialAd = null;
                        }
                        Debug.Log("Loading interstitial ad. " + Time.time);
                        var adRequest = new AdRequest();
                        InterstitialAd.Load(key, adRequest, (InterstitialAd ad, LoadAdError error) =>
                        {
                            if (error != null || ad == null)
                            {
                                Debug.LogError("Interstitial ad failed to load an ad with error : " + error);
                                OnInterstitialAdLoadFailedEvent(key);
                                return;
                            }
                            Debug.Log("Interstitial ad loaded with response : " + ad.GetResponseInfo());
                            _interstitialAd = ad;
                            OnInterstitialAdLoadedEvent();
                            RegisterEventHandlers(ad);
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }



        // ---- Banner Ad Methods ----

        /// <summary>
        /// Loads a banner ad at the bottom of the screen.
        /// BannerView is a persistent ad view -- once created, it stays until destroyed.
        /// </summary>
        public override void LoadBanner()
        {
            var key = BannerID;
            if (string.IsNullOrEmpty(key)) return;

            if (_bannerView == null)
                _bannerView = new BannerView(key, AdSize.Banner, AdPosition.Bottom);

            var adRequest = new AdRequest();
            Debug.Log("Loading banner.");
            _bannerView.LoadAd(adRequest);
            HideBanner(); // Start hidden, show when ready
        }

        public override void ShowBanner()
        {
            if (_bannerView != null)
            {
                Debug.Log("Show banner.");
                _bannerView.Show();
            }
        }

        public override void HideBanner()
        {
            if (_bannerView != null)
            {
                Debug.Log("Hide banner.");
                _bannerView.Hide();
            }
        }

        /// <summary>Completely destroys the banner view and frees its resources.</summary>
        public override void UnloadBanner()
        {
            if (_bannerView != null)
            {
                Debug.Log("Destroying banner.");
                _bannerView.Destroy();
                _bannerView = null;
            }
        }



        // ---- Interstitial Ad Methods ----

        /// <summary>
        /// Schedules an interstitial ad to load. The actual loading happens in OnPostTick
        /// when the scheduled time arrives (immediately, since we set time to Time.time).
        /// </summary>
        public override void LoadInterstitial()
        {
            var key = InterstitialID;
            if (string.IsNullOrEmpty(key)) return;
            _interstitialReloadTimeMap[key] = Time.time; // Schedule for immediate load
        }

        /// <summary>Shows the interstitial if it's loaded and ready.</summary>
        public override void ShowInterstitial()
        {
            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                Debug.Log("Showing interstitial ad.");
                ON_INTERSTITIAL_SHOW.SafeInvoke(); // Notify listeners before showing
                _interstitialAd.Show();
            }
            else
            {
                Debug.LogError("Interstitial ad is not ready yet.");
            }
        }

        private void OnInterstitialAdLoadedEvent()
        {
        }

        /// <summary>On load failure, schedule a retry after kReloadDuration seconds.</summary>
        private void OnInterstitialAdLoadFailedEvent(string key)
        {
            _interstitialReloadTimeMap[key] = Time.time + kReloadDuration;
        }

        /// <summary>
        /// Registers event handlers for an interstitial ad's lifecycle.
        /// These are callback-based events provided by the AdMob SDK.
        /// Lambda syntax (=>) creates inline anonymous functions.
        /// </summary>
        private void RegisterEventHandlers(InterstitialAd ad)
        {
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Interstitial ad recorded an impression.");
            };
            ad.OnAdClicked += () =>
            {
                Debug.Log("Interstitial ad was clicked.");
            };
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Interstitial ad full screen content opened.");
            };
            // When the ad is closed, reload a new one for next time
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Interstitial ad full screen content closed.");
                OnInterstitialHiddenEvent();
            };
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Interstitial ad failed to open full screen content with error : "
                    + error);
            };
        }

        /// <summary>Called when user closes the interstitial. Notifies listeners and reloads.</summary>
        private void OnInterstitialHiddenEvent()
        {
            try
            {
                ON_INTERSTITIAL_WATCHED.SafeInvoke(); // Notify game logic
                LoadInterstitial();                    // Pre-load the next one
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }



        // ---- Rewarded Ad Methods ----

        /// <summary>Schedules a rewarded ad to load (same pattern as interstitial).</summary>
        public override void LoadRewarded()
        {
            var key = RewardedID;
            if (string.IsNullOrEmpty(key)) return;
            _rewardedReloadTimeMap[key] = Time.time;
        }

        /// <summary>
        /// Shows the rewarded ad if loaded. The reward callback fires when the user
        /// finishes watching (the ad.Show callback receives the Reward object).
        /// </summary>
        public override void ShowRewarded()
        {
            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                Debug.Log("Showing rewarded ad.");
                _rewardedAd.Show((Reward reward) =>
                {
                    Debug.Log(String.Format("Rewarded ad granted a reward: {0} {1}",
                                            reward.Amount,
                                            reward.Type));
                });
            }
            else
            {
                Debug.LogError("Rewarded ad is not ready yet.");
            }
        }

        /// <summary>Registers lifecycle event handlers for a rewarded ad.</summary>
        private void RegisterEventHandlers(RewardedAd ad)
        {
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Rewarded ad recorded an impression.");
            };
            ad.OnAdClicked += () =>
            {
                Debug.Log("Rewarded ad was clicked.");
            };
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Rewarded ad full screen content opened.");
            };
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Rewarded ad full screen content closed.");
                OnRewardedAdHiddenEvent();
            };
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Rewarded ad failed to open full screen content with error : "
                    + error);
            };
        }

        private void OnRewardedAdLoadedEvent()
        {
        }

        /// <summary>Called when user closes the rewarded ad. Notifies listeners and reloads.</summary>
        private void OnRewardedAdHiddenEvent()
        {
            try
            {
                ON_REWARDED_WATCHED.SafeInvoke(); // Notify game logic to grant the reward
                LoadRewarded();                    // Pre-load the next one
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        /// <summary>On load failure, schedule a retry after kReloadDuration seconds.</summary>
        private void OnRewardedAdLoadFailedEvent(string key)
        {
            _rewardedReloadTimeMap[key] = Time.time + kReloadDuration;
        }
    }
}
