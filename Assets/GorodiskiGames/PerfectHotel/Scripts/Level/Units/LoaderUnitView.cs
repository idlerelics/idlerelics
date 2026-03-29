using Game.Level.Unit;
using UnityEngine;

namespace Game.Level.Loader
{
    public sealed class LoaderUnitView : UnitStaffView
    {
        [SerializeField] private ParticleSystem _particles;

        internal void PlayUnitParticles()
        {
            _particles.Play();
        }
    }
}

