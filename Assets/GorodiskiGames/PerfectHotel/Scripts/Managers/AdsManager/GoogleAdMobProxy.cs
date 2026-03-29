using System;
using System.Collections.Generic;
using Game.Core;
using UnityEngine;
using GoogleMobileAds.Api;
using Utilities;

namespace Game.Managers
{
    public sealed class GoogleAdMobProxy : BaseAdsProxy
    {
        private const int kReloadDuration = 2;

        private readonly Dictionary<string, float> _rewardedReloadTimeMap;
        private readonly Dictionary<string, float> _interstitialReloadTimeMap;

        BannerView _bannerView;
        InterstitialAd _interstitialAd;
        RewardedAd _rewardedAd;

        private bool _isDebugBuild;

        private string InterstitialID;
        private string BannerID;
        private string RewardedID;

        public GoogleAdMobProxy(Timer timer) : base(timer)
        {
            _interstitialReloadTimeMap = new Dictionary<string, float>();
            _rewardedReloadTimeMap = new Dictionary<string, float>();
        }

        public override void Initialize()
        {
            base.Initialize();

            _isDebugBuild = GameConstants.IsDebugBuild();

            GetIDs(out InterstitialID, out BannerID, out RewardedID);

            MobileAds.Initialize(initStatus => { OnSdkInitializedEvent(); });
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        private void GetIDs(out string interstitialID, out string bannerID, out string rewardedID)
        {
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
            INITIALIZED.SafeInvoke();
        }

        public override void OnPostTick()
        {
            if (_interstitialReloadTimeMap.Count + _rewardedReloadTimeMap.Count == 0)
                return;

            try
            {
                var keys = new List<string>(_rewardedReloadTimeMap.Keys);
                foreach (var key in keys)
                {
                    if (Time.time >= _rewardedReloadTimeMap[key])
                    {
                        _rewardedReloadTimeMap.Remove(key);
                        if (_rewardedAd != null)
                        {
                            _rewardedAd.Destroy();
                            _rewardedAd = null;
                        }
                        Debug.Log("Loading rewarded ad. " + Time.time);
                        var adRequest = new AdRequest();
                        RewardedAd.Load(key, adRequest, (RewardedAd ad, LoadAdError error) =>
                        {
                            if (error != null || ad == null)
                            {
                                Debug.LogError("Rewarded ad failed to load an ad with error : " + error);
                                OnRewardedAdLoadFailedEvent(key);
                                return;
                            }
                            Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());
                            _rewardedAd = ad;
                            OnRewardedAdLoadedEvent();
                            RegisterEventHandlers(ad);
                        });
                    }
                }

                keys = new List<string>(_interstitialReloadTimeMap.Keys);
                foreach (var key in keys)
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



        //banner
        public override void LoadBanner()
        {
            var key = BannerID;
            if (string.IsNullOrEmpty(key)) return;

            if (_bannerView == null)
                _bannerView = new BannerView(key, AdSize.Banner, AdPosition.Bottom);

            var adRequest = new AdRequest();
            Debug.Log("Loading banner.");
            _bannerView.LoadAd(adRequest);
            HideBanner();
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

        public override void UnloadBanner()
        {
            if (_bannerView != null)
            {
                Debug.Log("Destroying banner.");
                _bannerView.Destroy();
                _bannerView = null;
            }
        }



        //interstitial
        public override void LoadInterstitial()
        {
            var key = InterstitialID;
            if (string.IsNullOrEmpty(key)) return;
            _interstitialReloadTimeMap[key] = Time.time;
        }

        public override void ShowInterstitial()
        {
            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                Debug.Log("Showing interstitial ad.");
                ON_INTERSTITIAL_SHOW.SafeInvoke();
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

        private void OnInterstitialAdLoadFailedEvent(string key)
        {
            _interstitialReloadTimeMap[key] = Time.time + kReloadDuration;
        }

        private void RegisterEventHandlers(InterstitialAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Interstitial ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("Interstitial ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Interstitial ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Interstitial ad full screen content closed.");
                OnInterstitialHiddenEvent();
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Interstitial ad failed to open full screen content with error : "
                    + error);
            };
        }

        private void OnInterstitialHiddenEvent()
        {
            try
            {
                ON_INTERSTITIAL_WATCHED.SafeInvoke();
                LoadInterstitial();
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }



        //rewarded
        public override void LoadRewarded()
        {
            var key = RewardedID;
            if (string.IsNullOrEmpty(key)) return;
            _rewardedReloadTimeMap[key] = Time.time;
        }

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

        private void RegisterEventHandlers(RewardedAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Rewarded ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("Rewarded ad was clicked.");
            };
            // Raised when the ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Rewarded ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Rewarded ad full screen content closed.");
                OnRewardedAdHiddenEvent();
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Rewarded ad failed to open full screen content with error : "
                    + error);
            };
        }

        private void OnRewardedAdLoadedEvent()
        {
        }

        private void OnRewardedAdHiddenEvent()
        {
            try
            {
                ON_REWARDED_WATCHED.SafeInvoke();
                LoadRewarded();
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }
        private void OnRewardedAdLoadFailedEvent(string key)
        {
            _rewardedReloadTimeMap[key] = Time.time + kReloadDuration;
        }
    }
}

