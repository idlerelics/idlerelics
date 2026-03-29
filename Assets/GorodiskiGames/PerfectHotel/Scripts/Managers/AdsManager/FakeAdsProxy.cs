using Game.Core;
using Utilities;

namespace Game.Managers
{
    public sealed class FakeAdsProxy : BaseAdsProxy
    {
        private const string _fakeInterstitialId = "fake_keyInterstitial";
        private const string _fakeRewardedId = "fake_keyRewarded";

        public FakeAdsProxy(Timer timer) : base(timer)
        {
        }

        public override void OnPostTick()
        {
        }

        public override void LoadRewarded()
        {
        }

        public override void ShowRewarded()
        {
            var key = _fakeRewardedId;
            Log.Info($"Show ad {key}");
            ON_REWARDED_WATCHED.SafeInvoke();
        }

        public override void LoadInterstitial()
        {
        }

        public override void ShowInterstitial()
        {
            var key = _fakeInterstitialId;
            Log.Info($"Show ad {key}");
            ON_INTERSTITIAL_WATCHED.SafeInvoke();
        }

        public override void LoadBanner()
        {
        }

        public override void ShowBanner()
        {
        }

        public override void HideBanner()
        {
        }

        public override void UnloadBanner()
        {
        }
    }
}