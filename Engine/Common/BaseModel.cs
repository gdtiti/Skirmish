﻿using System.Collections.Generic;
using System.IO;

namespace Engine.Common
{
    using Engine.Animation;
    using Engine.Content;

    /// <summary>
    /// Model basic implementation
    /// </summary>
    public abstract class BaseModel : Drawable, UseMaterials, UseSkinningData
    {
        /// <summary>
        /// Meshes by level of detail dictionary
        /// </summary>
        private LODDictionary meshesByLOD = new LODDictionary();
        /// <summary>
        /// Default level of detail
        /// </summary>
        private readonly LevelOfDetailEnum defaultLevelOfDetail = LevelOfDetailEnum.Minimum;

        /// <summary>
        /// Level of detail
        /// </summary>
        public virtual LevelOfDetailEnum LevelOfDetail { get; set; }
        /// <summary>
        /// Gets the texture count for texture index
        /// </summary>
        public int TextureCount { get; private set; }
        /// <summary>
        /// Gets the material list used by the current drawing data
        /// </summary>
        public virtual MeshMaterial[] Materials
        {
            get
            {
                List<MeshMaterial> matList = new List<MeshMaterial>();

                var drawingData = this.GetDrawingData(LevelOfDetailEnum.High);
                if (drawingData != null)
                {
                    foreach (var meshMaterial in drawingData.Materials.Keys)
                    {
                        matList.Add(drawingData.Materials[meshMaterial]);
                    }
                }

                return matList.ToArray();
            }
        }
        /// <summary>
        /// Use anisotropic filtering
        /// </summary>
        public virtual bool UseAnisotropicFiltering { get; set; }
        /// <summary>
        /// Gets the skinning list used by the current drawing data
        /// </summary>
        public virtual SkinningData[] SkinningData
        {
            get
            {
                List<SkinningData> skList = new List<SkinningData>();

                var drawingData = this.GetDrawingData(LevelOfDetailEnum.High);
                if (drawingData != null)
                {
                    foreach (var meshMaterial in drawingData.Materials.Keys)
                    {
                        if (drawingData.SkinningData != null)
                        {
                            skList.Add(drawingData.SkinningData);
                        }
                    }
                }

                return skList.ToArray();
            }
        }
        /// <summary>
        /// Use spheric volume for culling test
        /// </summary>
        public bool SphericVolume { get; set; }

        /// <summary>
        /// Base model
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Object description</param>
        public BaseModel(Scene scene, BaseModelDescription description)
            : base(scene, description)
        {
            var desc = new DrawingDataDescription()
            {
                Instanced = description.Instanced,
                Instances = description.Instances,
                LoadAnimation = description.LoadAnimation,
                LoadNormalMaps = description.LoadNormalMaps,
                TextureCount = this.TextureCount,
                DynamicBuffers = description.Dynamic,
            };

            ModelContent[] geo = null;

            if (!string.IsNullOrEmpty(description.Content.ModelContentFilename))
            {
                var contentDesc = Helper.DeserializeFromFile<ModelContentDescription>(Path.Combine(description.Content.ContentFolder, description.Content.ModelContentFilename));
                using (var loader = contentDesc.GetLoader())
                {
                    geo = loader.Load(description.Content.ContentFolder, contentDesc);
                }
            }
            else if (description.Content.ModelContentDescription != null)
            {
                using (var loader = description.Content.ModelContentDescription.GetLoader())
                {
                    geo = loader.Load(description.Content.ContentFolder, description.Content.ModelContentDescription);
                }
            }
            else if (description.Content.ModelContent != null)
            {
                geo = new[] { description.Content.ModelContent };
            }

            if (geo.Length == 1)
            {
                if (description.Optimize) geo[0].Optimize();

                var drawable = DrawingData.Build(this.Game, this.BufferManager, geo[0], desc);

                this.meshesByLOD.Add(LevelOfDetailEnum.High, drawable);

                this.LevelOfDetail = LevelOfDetailEnum.None;
            }
            else
            {
                var content = new LODModelContent(geo, description.Optimize);

                foreach (var lod in content.Keys)
                {
                    if (this.defaultLevelOfDetail == LevelOfDetailEnum.None)
                    {
                        this.defaultLevelOfDetail = lod;
                    }

                    var drawable = DrawingData.Build(this.Game, this.BufferManager, content[lod], desc);

                    this.meshesByLOD.Add(lod, drawable);
                }

                this.LevelOfDetail = this.defaultLevelOfDetail;
            }

            this.UseAnisotropicFiltering = description.UseAnisotropicFiltering;
        }
        /// <summary>
        /// Dispose model buffers
        /// </summary>
        public override void Dispose()
        {
            Helper.Dispose(this.meshesByLOD);
            this.meshesByLOD = null;
        }

        /// <summary>
        /// Gets the nearest level of detail for the specified level of detail
        /// </summary>
        /// <param name="lod">Level of detail</param>
        /// <returns>Returns the nearest level of detail for the specified level of detail</returns>
        internal LevelOfDetailEnum GetLODNearest(LevelOfDetailEnum lod)
        {
            if (this.meshesByLOD.Keys.Count == 0)
            {
                return this.defaultLevelOfDetail;
            }
            else
            {
                if (this.meshesByLOD.Keys.Count == 1)
                {
                    return this.meshesByLOD.Keys.ToArray()[0];
                }
                else
                {
                    int i = (int)lod;

                    for (int l = i; l > 0; l /= 2)
                    {
                        if (this.meshesByLOD.ContainsKey((LevelOfDetailEnum)l))
                        {
                            return (LevelOfDetailEnum)l;
                        }
                    }

                    return this.defaultLevelOfDetail;
                }
            }
        }
        /// <summary>
        /// Gets the minimum level of detail
        /// </summary>
        /// <returns>Returns the minimum level of detail</returns>
        internal LevelOfDetailEnum GetLODMinimum()
        {
            int l = int.MaxValue;

            foreach (var lod in this.meshesByLOD.Keys)
            {
                if ((int)lod < l)
                {
                    l = (int)lod;
                }
            }

            return (LevelOfDetailEnum)l;
        }
        /// <summary>
        /// Gets the maximum level of detail
        /// </summary>
        /// <returns>Returns the maximum level of detail</returns>
        internal LevelOfDetailEnum GetLODMaximum()
        {
            int l = int.MinValue;

            foreach (var lod in this.meshesByLOD.Keys)
            {
                if ((int)lod > l)
                {
                    l = (int)lod;
                }
            }

            return (LevelOfDetailEnum)l;
        }
        /// <summary>
        /// Gets the drawing data by level of detail
        /// </summary>
        /// <param name="lod">Level of detail</param>
        /// <returns>Returns the drawing data object</returns>
        internal DrawingData GetDrawingData(LevelOfDetailEnum lod)
        {
            if (this.meshesByLOD.ContainsKey(lod))
            {
                return this.meshesByLOD[lod];
            }

            return null;
        }
        /// <summary>
        /// Gets the first drawing data avaliable for the specified level of detail, from the specified one
        /// </summary>
        /// <param name="lod">First level of detail</param>
        /// <returns>Returns the first available level of detail drawing data</returns>
        internal DrawingData GetFirstDrawingData(LevelOfDetailEnum lod)
        {
            while (lod > LevelOfDetailEnum.None)
            {
                if (this.meshesByLOD.ContainsKey(lod))
                {
                    return this.meshesByLOD[lod];
                }

                lod = (LevelOfDetailEnum)((int)lod / 2);
            }

            return null;
        }
    }
}
