﻿using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;

namespace Engine
{
    using Engine.Collections.Generic;
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Terrain model
    /// </summary>
    public class Scenery : Ground, UseMaterials
    {
        #region Helper Classes

        /// <summary>
        /// Scenery patches dictionary
        /// </summary>
        class SceneryPatchDictionary : Dictionary<int, SceneryPatch>
        {

        }
        /// <summary>
        /// Terrain patch
        /// </summary>
        /// <remarks>
        /// Holds the necessary information to render a portion of terrain using an arbitrary level of detail
        /// </remarks>
        class SceneryPatch : IDisposable
        {
            const int MAX = 1024 * 2;

            /// <summary>
            /// Creates a new patch
            /// </summary>
            /// <param name="game">Game</param>
            /// <param name="content">Content</param>
            /// <param name="node">Quadtree node</param>
            /// <returns>Returns the new generated patch</returns>
            public static SceneryPatch CreatePatch(Game game, BufferManager bufferManager, ModelContent content, PickingQuadTreeNode<Triangle> node)
            {
                var desc = new DrawingDataDescription()
                {
                    Instanced = false,
                    Instances = 0,
                    LoadAnimation = true,
                    LoadNormalMaps = true,
                    TextureCount = 0,
                    DynamicBuffers = false,
                    Constraint = node.BoundingBox,
                };

                var drawingData = DrawingData.Build(game, bufferManager, content, desc);

                return new SceneryPatch(game, drawingData);
            }

            /// <summary>
            /// Game
            /// </summary>
            protected Game Game = null;
            /// <summary>
            /// Drawing data
            /// </summary>
            public DrawingData DrawingData = null;

            /// <summary>
            /// Current quadtree node
            /// </summary>
            public PickingQuadTreeNode<Triangle> Current { get; private set; }

            /// <summary>
            /// Cosntructor
            /// </summary>
            /// <param name="game">Game</param>
            /// <param name="drawingData">Drawing data</param>
            public SceneryPatch(Game game, DrawingData drawingData)
            {
                this.Game = game;
                this.DrawingData = drawingData;
            }
            /// <summary>
            /// Releases created resources
            /// </summary>
            public void Dispose()
            {
                Helper.Dispose(this.DrawingData);
            }

            /// <summary>
            /// Updates the scenery patch
            /// </summary>
            /// <param name="context">Context</param>
            /// <param name="node">Node</param>
            public void Update(UpdateContext context, PickingQuadTreeNode<Triangle> node)
            {
                this.Current = node;
            }
            /// <summary>
            /// Draws the scenery patch
            /// </summary>
            /// <param name="context">Drawing context</param>
            /// <param name="sceneryEffect">Scenery effect</param>
            public void DrawScenery(DrawContext context, Drawer sceneryEffect, BufferManager bufferManager)
            {
                int count = 0;

                foreach (string meshName in this.DrawingData.Meshes.Keys)
                {
                    var dictionary = this.DrawingData.Meshes[meshName];

                    foreach (string material in dictionary.Keys)
                    {
                        #region Per object update

                        var mat = this.DrawingData.Materials[material];

                        if (context.DrawerMode == DrawerModesEnum.Forward)
                        {
                            ((EffectDefaultBasic)sceneryEffect).UpdatePerObject(
                                mat.DiffuseTexture,
                                mat.NormalMap,
                                mat.SpecularTexture,
                                mat.ResourceIndex,
                                0,
                                0);
                        }
                        else if (context.DrawerMode == DrawerModesEnum.Deferred)
                        {
                            ((EffectDeferredBasic)sceneryEffect).UpdatePerObject(
                                mat.DiffuseTexture,
                                mat.NormalMap,
                                mat.SpecularTexture,
                                mat.ResourceIndex,
                                0,
                                0);
                        }
                        else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                        {
                            ((EffectShadowBasic)sceneryEffect).UpdatePerObject(
                                mat.DiffuseTexture,
                                0,
                                0);
                        }

                        #endregion

                        var mesh = dictionary[material];
                        bufferManager.SetIndexBuffer(mesh.IndexBuffer.Slot);

                        var technique = sceneryEffect.GetTechnique(mesh.VertextType, mesh.Instanced, DrawingStages.Drawing, context.DrawerMode);
                        bufferManager.SetInputAssembler(technique, mesh.VertexBuffer.Slot, mesh.Topology);

                        count += mesh.IndexBuffer.Count > 0 ? mesh.IndexBuffer.Count : mesh.VertexBuffer.Count;

                        for (int p = 0; p < technique.Description.PassCount; p++)
                        {
                            technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                            mesh.Draw(this.Game.Graphics);

                            Counters.DrawCallsPerFrame++;
                        }
                    }
                }

                if (context.DrawerMode != DrawerModesEnum.ShadowMap)
                {
                    Counters.InstancesPerFrame++;
                    Counters.PrimitivesPerFrame += count / 3;
                }
            }

            /// <summary>
            /// Gets all the used materials
            /// </summary>
            /// <returns>Returns the used materials array</returns>
            public IEnumerable<MeshMaterial> GetMaterials()
            {
                List<MeshMaterial> matList = new List<MeshMaterial>();

                foreach (string meshName in this.DrawingData.Meshes.Keys)
                {
                    var dictionary = this.DrawingData.Meshes[meshName];

                    foreach (string material in dictionary.Keys)
                    {
                        matList.Add(this.DrawingData.Materials[material]);
                    }
                }

                return matList;
            }
        }

        #endregion

        /// <summary>
        /// Scenery patch list
        /// </summary>
        private SceneryPatchDictionary patchDictionary = new SceneryPatchDictionary();
        /// <summary>
        /// Visible Nodes
        /// </summary>
        private PickingQuadTreeNode<Triangle>[] visibleNodes = null;

        /// <summary>
        /// Gets the used material list
        /// </summary>
        public virtual MeshMaterial[] Materials
        {
            get
            {
                List<MeshMaterial> matList = new List<MeshMaterial>();

                var nodes = this.groundPickingQuadtree.GetLeafNodes();

                foreach (var node in nodes)
                {
                    var mats = this.patchDictionary[node.Id].GetMaterials();
                    if (mats != null)
                    {
                        matList.AddRange(mats);
                    }
                }

                return matList.ToArray();
            }
        }
        /// <summary>
        /// Gets the visible node count
        /// </summary>
        public int VisiblePatchesCount
        {
            get
            {
                return this.visibleNodes != null ? this.visibleNodes.Length : 0;
            }
        }
        /// <summary>
        /// Gets the current model lights collection
        /// </summary>
        public SceneLight[] Lights { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Terrain description</param>
        public Scenery(Scene scene, GroundDescription description)
            : base(scene, description)
        {
            ModelContent content = null;

            if (!string.IsNullOrEmpty(description.Content.ModelContentFilename))
            {
                var contentDesc = Helper.DeserializeFromFile<ModelContentDescription>(Path.Combine(description.Content.ContentFolder, description.Content.ModelContentFilename));

                var t = LoaderCOLLADA.Load(description.Content.ContentFolder, contentDesc);
                content = t[0];
            }
            else if(description.Content.ModelContentDescription != null)
            {
                var t = LoaderCOLLADA.Load(description.Content.ContentFolder, description.Content.ModelContentDescription);
                content = t[0];
            }
            else if (description.Content.HeightmapDescription != null)
            {
                content = ModelContent.FromHeightmap(
                    description.Content.HeightmapDescription.ContentPath,
                    description.Content.HeightmapDescription.HeightmapFileName,
                    description.Content.HeightmapDescription.Textures.TexturesLR,
                    description.Content.HeightmapDescription.CellSize,
                    description.Content.HeightmapDescription.MaximumHeight);
            }
            else if (description.Content.ModelContent != null)
            {
                content = description.Content.ModelContent;
            }

            #region Patches

            this.groundPickingQuadtree = new PickingQuadTree<Triangle>(content.GetTriangles(), description.Quadtree.MaximumDepth);

            var nodes = this.groundPickingQuadtree.GetLeafNodes();

            foreach (var node in nodes)
            {
                var patch = SceneryPatch.CreatePatch(this.Game, this.BufferManager, content, node);

                this.patchDictionary.Add(node.Id, patch);
            }

            #endregion

            #region Lights

            List<SceneLight> lights = new List<SceneLight>();

            foreach (var key in content.Lights.Keys)
            {
                var l = content.Lights[key];

                if (l.LightType == LightContentTypeEnum.Point)
                {
                    lights.Add(l.CreatePointLight());
                }
                else if (l.LightType == LightContentTypeEnum.Spot)
                {
                    lights.Add(l.CreateSpotLight());
                }
            }

            this.Lights = lights.ToArray();

            #endregion
        }
        /// <summary>
        /// Dispose of created resources
        /// </summary>
        public override void Dispose()
        {
            Helper.Dispose(this.patchDictionary);
        }
        /// <summary>
        /// Objects updating
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            this.visibleNodes = this.groundPickingQuadtree.GetNodesInVolume(ref context.Frustum);
            if (this.visibleNodes != null && this.visibleNodes.Length > 0)
            {
                //Sort nodes - draw far nodes first
                Array.Sort(this.visibleNodes, (n1, n2) =>
                {
                    float d1 = (n1.Center - context.EyePosition).LengthSquared();
                    float d2 = (n2.Center - context.EyePosition).LengthSquared();

                    return -d1.CompareTo(d2);
                });

                foreach (var node in this.visibleNodes)
                {
                    this.patchDictionary[node.Id].Update(context, node);
                }
            }
        }
        /// <summary>
        /// Objects drawing
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            var nodes = this.visibleNodes.Length > 0 ? this.visibleNodes : this.groundPickingQuadtree.GetLeafNodes();

            if (nodes != null && nodes.Length > 0)
            {
                Drawer sceneryEffect = null;

                if (context.DrawerMode == DrawerModesEnum.Forward)
                {
                    sceneryEffect = DrawerPool.EffectDefaultBasic;

                    #region Per frame update

                    ((EffectDefaultBasic)sceneryEffect).UpdatePerFrame(
                        context.World,
                        context.ViewProjection,
                        context.EyePosition,
                        context.Lights,
                        context.ShadowMaps,
                        context.ShadowMapLow,
                        context.ShadowMapHigh,
                        context.FromLightViewProjectionLow,
                        context.FromLightViewProjectionHigh);

                    #endregion
                }
                else if (context.DrawerMode == DrawerModesEnum.Deferred)
                {
                    sceneryEffect = DrawerPool.EffectDeferredBasic;

                    #region Per frame update

                    ((EffectDeferredBasic)sceneryEffect).UpdatePerFrame(
                        context.World,
                        context.ViewProjection);

                    #endregion
                }
                else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                {
                    sceneryEffect = DrawerPool.EffectShadowBasic;

                    #region Per frame update

                    ((EffectShadowBasic)sceneryEffect).UpdatePerFrame(
                        context.World,
                        context.ViewProjection);

                    #endregion
                }

                this.Game.Graphics.SetBlendDefault();

                foreach (var node in nodes)
                {
                    this.patchDictionary[node.Id].DrawScenery(context, sceneryEffect, this.BufferManager);
                }
            }
        }

        /// <summary>
        /// Gets terrain bounding boxes at specified level
        /// </summary>
        /// <param name="level">Level</param>
        /// <returns>Returns terrain bounding boxes</returns>
        public BoundingBox[] GetBoundingBoxes(int level = 0)
        {
            return this.groundPickingQuadtree.GetBoundingBoxes(level);
        }
    }
}
