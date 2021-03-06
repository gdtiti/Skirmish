﻿
namespace Engine
{
    using Engine.Content;

    /// <summary>
    /// Ground description
    /// </summary>
    public class GroundDescription : SceneObjectDescription
    {
        /// <summary>
        /// Quadtree description
        /// </summary>
        public class QuadtreeDescription
        {
            /// <summary>
            /// Maximum depth
            /// </summary>
            public int MaximumDepth { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            public QuadtreeDescription()
            {
                this.MaximumDepth = 3;
            }
        }

        /// <summary>
        /// Content
        /// </summary>
        public ContentDescription Content = new ContentDescription();
        /// <summary>
        /// Quadtree
        /// </summary>
        public QuadtreeDescription Quadtree = new QuadtreeDescription();
        /// <summary>
        /// Use anisotropic filtering
        /// </summary>
        public bool UseAnisotropic = true;

        /// <summary>
        /// Constructor
        /// </summary>
        public GroundDescription()
            : base()
        {
            this.Static = true;
            this.CastShadow = true;
            this.DeferredEnabled = true;
            this.DepthEnabled = true;
            this.AlphaEnabled = false;
        }
    }
}
