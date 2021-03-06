﻿using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Ground gardener description
    /// </summary>
    public class GroundGardenerDescription : SceneObjectDescription
    {
        /// <summary>
        /// Vegetation channel
        /// </summary>
        public class Channel
        {
            /// <summary>
            /// Texture names array for vegetation
            /// </summary>
            public string[] VegetationTextures = null;
            /// <summary>
            /// Normal maps names array for vegetation
            /// </summary>
            public string[] VegetationNormalMaps = null;
            /// <summary>
            /// Vegetation sprite minimum size
            /// </summary>
            public Vector2 MinSize = Vector2.One;
            /// <summary>
            /// Vegetation sprite maximum size
            /// </summary>
            public Vector2 MaxSize = Vector2.One * 2f;
            /// <summary>
            /// Delta
            /// </summary>
            public Vector3 Delta = new Vector3(0.5f, 0.0f, 0.5f);
            /// <summary>
            /// Drawing radius for vegetation
            /// </summary>
            public float StartRadius = 0f;
            /// <summary>
            /// Drawing radius for vegetation
            /// </summary>
            public float EndRadius = 0f;
            /// <summary>
            /// Seed for random position generation
            /// </summary>
            public int Seed = 0;
            /// <summary>
            /// Vegetation saturation per triangle
            /// </summary>
            public float Saturation = 0.1f;
            /// <summary>
            /// Wind effect
            /// </summary>
            public float WindEffect = 1f;
            /// <summary>
            /// Channel enabled
            /// </summary>
            public bool Enabled = true;
            /// <summary>
            /// Geometry output count
            /// </summary>
            public int Count = 1;
        }

        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath = "Resources";

        /// <summary>
        /// Vegetation map
        /// </summary>
        public string VegetationMap = null;
        /// <summary>
        /// Visible radius
        /// </summary>
        public float VisibleRadius
        {
            get
            {
                float vRadius = 0;

                for (int i = 0; i < this.Channels.Length; i++)
                {
                    vRadius = Math.Max(vRadius, this.Channels[i].EndRadius);
                }

                return vRadius;
            }
        }
        /// <summary>
        /// Node size
        /// </summary>
        public float NodeSize = 640f;
        /// <summary>
        /// Material
        /// </summary>
        public MaterialDescription Material = new MaterialDescription();
        /// <summary>
        /// Red vegetation channel from map
        /// </summary>
        public Channel ChannelRed = new Channel() { Enabled = false };
        /// <summary>
        /// Green vegetation channel from map
        /// </summary>
        public Channel ChannelGreen = new Channel() { Enabled = false };
        /// <summary>
        /// Blue vegetation channel from map
        /// </summary>
        public Channel ChannelBlue = new Channel() { Enabled = false };
        /// <summary>
        /// Gets the active channel list
        /// </summary>
        public Channel[] Channels
        {
            get
            {
                List<Channel> channels = new List<Channel>();

                channels.Add(this.ChannelRed);
                channels.Add(this.ChannelGreen);
                channels.Add(this.ChannelBlue);

                return channels.ToArray();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public GroundGardenerDescription()
            : base()
        {
            this.Static = false;
            this.CastShadow = true;
            this.DeferredEnabled = false;
            this.DepthEnabled = true;
            this.AlphaEnabled = true;
        }
    }
}
