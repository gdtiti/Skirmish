﻿using SharpDX;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Particle emitter
    /// </summary>
    public class ParticleEmitter
    {
        /// <summary>
        /// Emitter position
        /// </summary>
        public Vector3 Position { get; set; }
        /// <summary>
        /// Emitter velocity
        /// </summary>
        public Vector3 Velocity { get; set; }
        /// <summary>
        /// Emission rate
        /// </summary>
        public float EmissionRate { get; set; }
        /// <summary>
        /// Emitter duration
        /// </summary>
        public float Duration { get; set; }
        /// <summary>
        /// Gets or sets wheter the emitter duration is infinite
        /// </summary>
        public bool InfiniteDuration { get; set; }
        /// <summary>
        /// Gets wheter the emitter is active
        /// </summary>
        public bool Active
        {
            get
            {
                return (this.InfiniteDuration || this.Duration > 0);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ParticleEmitter()
        {

        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="context">Updating context</param>
        public virtual void Update(UpdateContext context)
        {
            if (!this.InfiniteDuration)
            {
                this.Duration -= context.GameTime.ElapsedSeconds;
            }
        }
  
        /// <summary>
        /// Gets the maximum number of particles running at the same time
        /// </summary>
        /// <param name="maxParticleDuration">Maximum particle duration</param>
        /// <returns>Returns the maximum number of particles running at the same time</returns>
        public int GetMaximumConcurrentParticles(float maxParticleDuration)
        {
            float maxActiveParticles = maxParticleDuration * (1f / this.EmissionRate);

            return (int)(maxActiveParticles != (int)maxActiveParticles ? maxActiveParticles + 1 : maxActiveParticles);
        }
    }
}
