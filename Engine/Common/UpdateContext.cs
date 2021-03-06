﻿using SharpDX;

namespace Engine.Common
{
    /// <summary>
    /// Updating context
    /// </summary>
    public class UpdateContext
    {
        /// <summary>
        /// Context name
        /// </summary>
        public string Name = "";
        /// <summary>
        /// Game time
        /// </summary>
        public GameTime GameTime;
        /// <summary>
        /// View matrix
        /// </summary>
        public Matrix View;
        /// <summary>
        /// Projection matrix
        /// </summary>
        public Matrix Projection;
        /// <summary>
        /// Projection near plane distance
        /// </summary>
        public float NearPlaneDistance;
        /// <summary>
        /// Projection far plane distance
        /// </summary>
        public float FarPlaneDistance;
        /// <summary>
        /// View * projection matrix
        /// </summary>
        public Matrix ViewProjection;
        /// <summary>
        /// Camera culling volume
        /// </summary>
        public CullingVolumeCamera CameraVolume;
        /// <summary>
        /// Eye position
        /// </summary>
        public Vector3 EyePosition;
        /// <summary>
        /// Eye view direction
        /// </summary>
        public Vector3 EyeDirection;
        /// <summary>
        /// Lights
        /// </summary>
        public SceneLights Lights;
    }
}
