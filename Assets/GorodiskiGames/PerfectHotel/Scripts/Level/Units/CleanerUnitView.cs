using Game.Level.Unit;
using UnityEngine;

namespace Game.Level.Cleaner
{
    /// <summary>
    /// Specialized UnitView for cleaner characters. Adds a particle effect
    /// that plays when the cleaner is upgraded (e.g., sparkles or a "level up" burst).
    ///
    /// "sealed" means no other class can inherit from this one.
    /// Extends UnitView, which handles all standard unit animations and navigation.
    /// </summary>
    public sealed class CleanerUnitView : UnitView
    {
        // SerializeField lets you assign the ParticleSystem in the Unity Inspector.
        // ParticleSystem is Unity's component for visual effects like sparks, smoke, etc.
        [SerializeField] private ParticleSystem _particles;

        /// <summary>
        /// Triggers the particle effect on this cleaner unit.
        /// Called when the cleaner is upgraded to provide visual feedback to the player.
        /// </summary>
        public void PlayUnitParticles()
        {
            _particles.Play();
        }
    }
}
