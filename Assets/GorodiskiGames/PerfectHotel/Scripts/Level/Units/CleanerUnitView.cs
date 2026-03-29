using Game.Level.Unit;
using UnityEngine;

namespace Game.Level.Cleaner
{
    public sealed class CleanerUnitView : UnitView
    {
        [SerializeField] private ParticleSystem _particles;

        public void PlayUnitParticles()
        {
            _particles.Play();
        }
    }
}

