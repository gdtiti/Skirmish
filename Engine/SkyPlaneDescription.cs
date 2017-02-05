﻿using SharpDX;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Sky plane description
    /// </summary>
    public class SkyPlaneDescription : DrawableDescription
    {
        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath = "Resources";
        /// <summary>
        /// Texture 1 name
        /// </summary>
        public string Texture1Name { get; set; }
        /// <summary>
        /// Texture 2 name
        /// </summary>
        public string Texture2Name { get; set; }

        /// <summary>
        /// Maximum brightness value when animated with key light
        /// </summary>
        public float MaxBrightness { get; set; }
        /// <summary>
        /// Minimum brightness value when animated with key light
        /// </summary>
        public float MinBrightness { get; set; }
        /// <summary>
        /// Clouds quad size
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// Texture repeat
        /// </summary>
        public int Repeat { get; set; }
        /// <summary>
        /// Plane width
        /// </summary>
        public float PlaneWidth { get; set; }
        /// <summary>
        /// Plane top
        /// </summary>
        public float PlaneTop { get; set; }
        /// <summary>
        /// Plane bottom
        /// </summary>
        public float PlaneBottom { get; set; }
        /// <summary>
        /// Fading distance
        /// </summary>
        public float FadingDistance { get; set; }
        /// <summary>
        /// Wind velocity
        /// </summary>
        public float Velocity { get; set; }
        /// <summary>
        /// Wind direction
        /// </summary>
        public Vector2 Direction { get; set; }
        /// <summary>
        /// Perturbation scale
        /// </summary>
        public float PerturbationScale { get; set; }
        /// <summary>
        /// Sky plane mode
        /// </summary>
        public SkyPlaneMode Mode { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SkyPlaneDescription()
            : base()
        {
            this.Static = true;
            this.AlwaysVisible = false;
            this.CastShadow = false;
            this.DeferredEnabled = true;
            this.EnableDepthStencil = false;
            this.EnableAlphaBlending = false;

            this.MaxBrightness = 0.75f;
            this.MinBrightness = 0.15f;
            this.Size = 100;
            this.Repeat = 2;
            this.PlaneWidth = 50;
            this.PlaneTop = 1f;
            this.PlaneBottom = -0.5f;
            this.FadingDistance = 20f;
            this.Velocity = 1f;
            this.Direction = Vector2.One;
            this.PerturbationScale = 0.3f;
            this.Mode = SkyPlaneMode.Static;
        }
    }
}