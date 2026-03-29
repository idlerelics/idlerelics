using System;
using Game.Core;

namespace Game.Managers
{
    public abstract class BaseAdsProxy : IDisposable
    {
        public Action INITIALIZED;

        public Action ON_INTERSTITIAL_SHOW;
        public Action ON_INTERSTITIAL_WATCHED;
        public Action ON_INTERSTITIAL_LOADED;
        public Action ON_INTERSTITIAL_FAILED_TO_LOAD;

        public Action ON_REWARDED_WATCHED;

        private readonly Timer _timer;

        protected BaseAdsProxy(Timer timer)
        {
            _timer = timer;
        }

        public virtual void Initialize()
        {
            _timer.POST_TICK += OnPostTick;
        }

        public virtual void Dispose()
        {
            _timer.POST_TICK -= OnPostTick;
        }

        public abstract void LoadRewarded();
        public abstract void ShowRewarded();
        public abstract void OnPostTick();
        public abstract void LoadInterstitial();
        public abstract void ShowInterstitial();
        public abstract void LoadBanner();
        public abstract void ShowBanner();
        public abstract void HideBanner();
        public abstract void UnloadBanner();
    }
}