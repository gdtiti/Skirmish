﻿using SharpDX;
using System;
using Buffer = SharpDX.Direct3D11.Buffer;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;
    using Engine.Helpers;

    /// <summary>
    /// Instaced model
    /// </summary>
    public class ModelInstanced : ModelBase
    {
        /// <summary>
        /// Instancing data per instance
        /// </summary>
        private VertexInstancingData[] instancingData = null;
        /// <summary>
        /// Model instance list
        /// </summary>
        private ModelInstance[] instances = null;
        /// <summary>
        /// Temporal instance listo for rendering
        /// </summary>
        private ModelInstance[] instancesTmp = null;

        /// <summary>
        /// Instancing data buffer
        /// </summary>
        protected Buffer InstancingBuffer = null;

        /// <summary>
        /// Enables z-buffer writting
        /// </summary>
        public bool EnableDepthStencil { get; set; }
        /// <summary>
        /// Enables transparent blending
        /// </summary>
        public bool EnableAlphaBlending { get; set; }
        /// <summary>
        /// Gets manipulator per instance list
        /// </summary>
        /// <returns>Gets manipulator per instance list</returns>
        public ModelInstance[] Instances
        {
            get
            {
                return this.instances;
            }
        }
        /// <summary>
        /// Gets instance count
        /// </summary>
        public int Count
        {
            get
            {
                return this.instances.Length;
            }
        }
        /// <summary>
        /// Gets visible instance count
        /// </summary>
        public int VisibleCount
        {
            get
            {
                return Array.FindAll(this.instances, i => i.Visible == true && i.Cull == false).Length;
            }
        }
        /// <summary>
        /// Stride of instancing data
        /// </summary>
        public int InstancingBufferStride { get; protected set; }
        /// <summary>
        /// Instances
        /// </summary>
        public int InstanceCount { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="content">Content</param>
        /// <param name="instances">Number of instances</param>
        /// <param name="dynamic">Sets whether the buffers must be created inmutables or not</param>
        public ModelInstanced(Game game, ModelContent content, int instances, bool dynamic = false)
            : base(game, content, true, instances, true, true, dynamic)
        {
            if (instances <= 0) throw new ArgumentException(string.Format("Instances parameter must be more than 0: {0}", instances));

            this.InstanceCount = instances;

            this.instances = Helper.CreateArray(instances, () => new ModelInstance(this));
            this.instancesTmp = new ModelInstance[instances];
            this.instancingData = new VertexInstancingData[instances];

            this.InstancingBuffer = this.Game.Graphics.Device.CreateVertexBufferWrite(this.instancingData);
            this.InstancingBufferStride = instancingData[0].Stride;

            this.AddVertexBufferBinding(new VertexBufferBinding(this.InstancingBuffer, this.InstancingBufferStride, 0));

            this.EnableDepthStencil = true;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="content">Content</param>
        /// <param name="instances">Number of instances</param>
        /// <param name="dynamic">Sets whether the buffers must be created inmutables or not</param>
        public ModelInstanced(Game game, LODModelContent content, int instances, bool dynamic = false)
            : base(game, content, true, instances, true, true, dynamic)
        {
            if (instances <= 0) throw new ArgumentException(string.Format("Instances parameter must be more than 0: {0}", instances));

            this.InstanceCount = instances;

            this.instances = Helper.CreateArray(instances, () => new ModelInstance(this));
            this.instancesTmp = new ModelInstance[instances];
            this.instancingData = new VertexInstancingData[instances];

            this.InstancingBuffer = this.Game.Graphics.Device.CreateVertexBufferWrite(this.instancingData);
            this.InstancingBufferStride = instancingData[0].Stride;

            this.AddVertexBufferBinding(new VertexBufferBinding(this.InstancingBuffer, this.InstancingBufferStride, 0));

            this.EnableDepthStencil = true;
        }
        /// <summary>
        /// Dispose model buffers
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            if (this.InstancingBuffer != null)
            {
                this.InstancingBuffer.Dispose();
                this.InstancingBuffer = null;
            }
        }
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            if (this.instances != null && this.instances.Length > 0)
            {
                for (int i = 0; i < this.instances.Length; i++)
                {
                    if (this.instances[i].Active)
                    {
                        this.instances[i].Manipulator.Update(context.GameTime);
                    }
                }

                //Update by level of detail
                for (int l = 1; l < (int)LevelOfDetailEnum.Minimum; l *= 2)
                {
                    var drawingData = this.GetDrawingData((LevelOfDetailEnum)l);
                    if (drawingData != null && drawingData.SkinningData != null)
                    {
                        drawingData.SkinningData.Update(context.GameTime);
                    }
                }
            }
        }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            if (this.VisibleCount > 0)
            {
                Drawer effect = null;
                if (context.DrawerMode == DrawerModesEnum.Forward) effect = DrawerPool.EffectInstancing;
                else if (context.DrawerMode == DrawerModesEnum.Deferred) effect = DrawerPool.EffectInstancingGBuffer;
                else if (context.DrawerMode == DrawerModesEnum.ShadowMap) effect = DrawerPool.EffectInstancingShadow;

                if (effect != null)
                {
                    //Process only visible instances
                    Array.Copy(this.instances, this.instancesTmp, this.instances.Length);

                    for (int i = 0; i < this.instancesTmp.Length; i++)
                    {
                        if (!this.instancesTmp[i].Visible || this.instancesTmp[i].Cull)
                        {
                            this.instancesTmp[i] = null;
                        }
                    }

                    //Sort by LOD
                    Array.Sort(this.instancesTmp, (i1, i2) =>
                    {
                        if (i1 == null)
                        {
                            return 1;
                        }
                        else if (i2 == null)
                        {
                            return -1;
                        }
                        else
                        {
                            return i1.LevelOfDetail.CompareTo(i2.LevelOfDetail);
                        }
                    });

                    int instanceIndex = 0;
                    for (int i = 0; i < this.instancesTmp.Length; i++)
                    {
                        if (this.instancesTmp[i] != null)
                        {
                            this.instancingData[instanceIndex].Local = this.instancesTmp[i].Manipulator.LocalTransform;
                            this.instancingData[instanceIndex].TextureIndex = this.instancesTmp[i].TextureIndex;

                            instanceIndex++;
                        }
                    }

                    //Writes instancing data
                    this.WriteInstancingData(this.DeviceContext, this.instancingData);

                    #region Per frame update

                    if (context.DrawerMode == DrawerModesEnum.Forward)
                    {
                        ((EffectInstancing)effect).UpdatePerFrame(
                            context.World,
                            context.ViewProjection,
                            context.EyePosition,
                            context.Frustum,
                            context.Lights,
                            context.ShadowMapStatic,
                            context.ShadowMapDynamic,
                            context.FromLightViewProjection);
                    }
                    else if (context.DrawerMode == DrawerModesEnum.Deferred)
                    {
                        ((EffectInstancingGBuffer)effect).UpdatePerFrame(
                            context.World,
                            context.ViewProjection);
                    }
                    else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                    {
                        ((EffectInstancingShadow)effect).UpdatePerFrame(
                            context.World,
                            context.ViewProjection);
                    }

                    #endregion

                    if (this.EnableDepthStencil)
                    {
                        this.Game.Graphics.SetDepthStencilZEnabled();
                    }
                    else
                    {
                        this.Game.Graphics.SetDepthStencilZDisabled();
                    }

                    if (this.EnableAlphaBlending)
                    {
                        this.Game.Graphics.SetBlendTransparent();
                    }

                    //Render by level of detail
                    for (int l = 1; l < (int)LevelOfDetailEnum.Minimum + 1; l *= 2)
                    {
                        LevelOfDetailEnum lod = (LevelOfDetailEnum)l;

                        //Get instances in this LOD
                        var ins = Array.FindAll(this.instancesTmp, i => i != null && i.LevelOfDetail == lod);
                        if (ins != null && ins.Length > 0)
                        {
                            var drawingData = this.GetDrawingData(lod);
                            if (drawingData != null)
                            {
                                var index = Array.IndexOf(this.instancesTmp, ins[0]);
                                var length = ins.Length;

                                foreach (string meshName in drawingData.Meshes.Keys)
                                {
                                    #region Per skinning update

                                    if (drawingData.SkinningData != null)
                                    {
                                        if (context.DrawerMode == DrawerModesEnum.Forward)
                                        {
                                            ((EffectInstancing)effect).UpdatePerSkinning(drawingData.SkinningData.GetFinalTransforms(meshName));
                                        }
                                        else if (context.DrawerMode == DrawerModesEnum.Deferred)
                                        {
                                            ((EffectInstancingGBuffer)effect).UpdatePerSkinning(drawingData.SkinningData.GetFinalTransforms(meshName));
                                        }
                                        else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                                        {
                                            ((EffectInstancingShadow)effect).UpdatePerSkinning(drawingData.SkinningData.GetFinalTransforms(meshName));
                                        }
                                    }
                                    else
                                    {
                                        if (context.DrawerMode == DrawerModesEnum.Forward)
                                        {
                                            ((EffectInstancing)effect).UpdatePerSkinning(null);
                                        }
                                        else if (context.DrawerMode == DrawerModesEnum.Deferred)
                                        {
                                            ((EffectInstancingGBuffer)effect).UpdatePerSkinning(null);
                                        }
                                        else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                                        {
                                            ((EffectInstancingShadow)effect).UpdatePerSkinning(null);
                                        }
                                    }

                                    #endregion

                                    var dictionary = drawingData.Meshes[meshName];

                                    foreach (string material in dictionary.Keys)
                                    {
                                        var mesh = dictionary[material];
                                        var mat = drawingData.Materials[material];

                                        #region Per object update

                                        var matdata = mat != null ? mat.Material : Material.Default;
                                        var texture = mat != null ? mat.DiffuseTexture : null;
                                        var normalMap = mat != null ? mat.NormalMap : null;

                                        if (context.DrawerMode == DrawerModesEnum.Forward)
                                        {
                                            ((EffectInstancing)effect).UpdatePerObject(matdata, texture, normalMap);
                                        }
                                        else if (context.DrawerMode == DrawerModesEnum.Deferred)
                                        {
                                            ((EffectInstancingGBuffer)effect).UpdatePerObject(matdata, texture, normalMap);
                                        }

                                        #endregion

                                        var technique = effect.GetTechnique(mesh.VertextType, DrawingStages.Drawing, context.DrawerMode);

                                        mesh.SetInputAssembler(this.DeviceContext, effect.GetInputLayout(technique));

                                        for (int p = 0; p < technique.Description.PassCount; p++)
                                        {
                                            technique.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                                            mesh.Draw(this.DeviceContext, index, length);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Frustum culling test
        /// </summary>
        /// <param name="frustum">Frustum</param>
        public override void FrustumCulling(BoundingFrustum frustum)
        {
            //Cull was made per instance
            this.Cull = true;

            for (int i = 0; i < this.Instances.Length; i++)
            {
                if (this.Instances[i].Visible)
                {
                    this.Instances[i].FrustumCulling(frustum);

                    if (!this.Instances[i].Cull)
                    {
                        this.Cull = false;
                    }
                }
            }
        }
        /// <summary>
        /// Sets cull value
        /// </summary>
        /// <param name="value">New value</param>
        public override void SetCulling(bool value)
        {
            base.SetCulling(value);

            if (this.instances != null && this.instances.Length > 0)
            {
                for (int i = 0; i < this.instances.Length; i++)
                {
                    this.instances[i].SetCulling(value);
                }
            }
        }
        /// <summary>
        /// Set instance positions
        /// </summary>
        /// <param name="positions">New positions</param>
        public void SetPositions(Vector3[] positions)
        {
            if (positions != null && positions.Length > 0)
            {
                if (this.Instances != null && this.Instances.Length > 0)
                {
                    for (int i = 0; i < this.Instances.Length; i++)
                    {
                        if (i < positions.Length)
                        {
                            this.Instances[i].Manipulator.SetPosition(positions[i], true);
                            this.Instances[i].Active = true;
                            this.Instances[i].Visible = true;
                        }
                        else
                        {
                            this.Instances[i].Active = false;
                            this.Instances[i].Visible = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Writes instancing data
        /// </summary>
        /// <param name="deviceContext">Immediate context</param>
        /// <param name="data">Instancig data</param>
        protected virtual void WriteInstancingData(DeviceContext deviceContext, VertexInstancingData[] data)
        {
            if (data != null && data.Length > 0)
            {
                this.InstanceCount = data.Length;

                if (this.InstancingBuffer != null)
                {
                    deviceContext.WriteBuffer(this.InstancingBuffer, data);
                }
            }
            else
            {
                this.InstanceCount = 0;
            }
        }
    }
}
