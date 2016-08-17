﻿using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using System;
using Buffer = SharpDX.Direct3D11.Buffer;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using InputLayout = SharpDX.Direct3D11.InputLayout;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;
    using Engine.Helpers;

    /// <summary>
    /// Billboard drawer
    /// </summary>
    public class Billboard : Drawable
    {
        /// <summary>
        /// Local transform
        /// </summary>
        private Matrix local = Matrix.Identity;

        /// <summary>
        /// Vertex buffer
        /// </summary>
        protected Buffer VertexBuffer;
        /// <summary>
        /// Vertex buffer binding
        /// </summary>
        protected VertexBufferBinding[] VertexBufferBinding = new VertexBufferBinding[0];
        /// <summary>
        /// Vertex count
        /// </summary>
        protected int VertexCount { get; set; }
        /// <summary>
        /// Textures
        /// </summary>
        protected ShaderResourceView Textures { get; set; }
        /// <summary>
        /// Texture count
        /// </summary>
        protected uint TextureCount { get; set; }

        /// <summary>
        /// Manipulator
        /// </summary>
        public Manipulator3D Manipulator { get; set; }
        /// <summary>
        /// Drawing start radius from eye point
        /// </summary>
        public float StartRadius { get; set; }
        /// <summary>
        /// Drawing end radius from eye point
        /// </summary>
        public float EndRadius { get; set; }
        /// <summary>
        /// Material
        /// </summary>
        public Material Material { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="content">Content</param>
        public Billboard(Game game, ModelContent content, int maxCount)
            : base(game)
        {
            this.Manipulator = new Manipulator3D();
            this.StartRadius = 0;
            this.EndRadius = 0;
            this.Material = Material.Default;

            #region Vertex buffer

            VertexBillboard[] tmp = new VertexBillboard[maxCount];

            this.VertexBuffer = this.Game.Graphics.Device.CreateVertexBufferWrite(tmp);
            this.VertexBufferBinding = new VertexBufferBinding[]
            {
                new VertexBufferBinding(this.VertexBuffer, (new VertexBillboard()).Stride, 0),
            };
            this.VertexCount = maxCount;

            #endregion

            #region Textures

            if (content.Images != null && content.Images.Count > 0)
            {
                foreach (var image in content.Images)
                {
                    this.TextureCount = (uint)image.Value.Count;

                    this.Textures = this.Game.Graphics.Device.LoadTextureArray(image.Value.Streams);

                    break;
                }
            }

            #endregion
        }
        /// <summary>
        /// Resources release
        /// </summary>
        public override void Dispose()
        {
            Helper.Dispose(this.VertexBuffer);
            Helper.Dispose(this.Textures);
        }
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            this.Manipulator.Update(context.GameTime);

            this.local = context.World * this.Manipulator.LocalTransform;
        }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            if (this.VertexCount > 0)
            {
                EffectBillboard effect = DrawerPool.EffectBillboard;
                EffectTechnique technique = null;
                if (context.DrawerMode == DrawerModesEnum.Forward) { technique = effect.ForwardBillboard; }
                else if (context.DrawerMode == DrawerModesEnum.Deferred) { technique = effect.DeferredBillboard; }
                else if (context.DrawerMode == DrawerModesEnum.ShadowMap) { technique = effect.ShadowMapBillboard; }

                if (technique != null)
                {
                    #region Per frame update

                    if (context.DrawerMode == DrawerModesEnum.Forward)
                    {
                        effect.UpdatePerFrame(
                            this.local,
                            context.ViewProjection,
                            context.EyePosition,
                            context.Lights,
                            context.ShadowMap,
                            context.FromLightViewProjection);
                    }
                    else if (context.DrawerMode == DrawerModesEnum.Deferred)
                    {
                        effect.UpdatePerFrame(
                            this.local,
                            context.ViewProjection,
                            context.EyePosition,
                            context.Lights,
                            context.ShadowMap,
                            context.FromLightViewProjection);
                    }
                    else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                    {
                        effect.UpdatePerFrame(
                            this.local,
                            context.ViewProjection,
                            context.EyePosition);
                    }

                    #endregion

                    this.Game.Graphics.SetDepthStencilZEnabled();

                    if (context.DrawerMode == DrawerModesEnum.Forward)
                    {
                        this.Game.Graphics.SetBlendTransparent();
                    }
                    else if (context.DrawerMode == DrawerModesEnum.Deferred)
                    {
                        this.Game.Graphics.SetBlendDeferredComposerTransparent();
                    }
                    else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                    {
                        this.Game.Graphics.SetBlendTransparent();
                    }

                    #region Per object update

                    if (context.DrawerMode == DrawerModesEnum.Forward)
                    {
                        effect.UpdatePerObject(this.Material, this.StartRadius, this.EndRadius, this.TextureCount, this.Textures);
                    }
                    else if (context.DrawerMode == DrawerModesEnum.Deferred)
                    {
                        effect.UpdatePerObject(this.Material, this.StartRadius, this.EndRadius, this.TextureCount, this.Textures);
                    }
                    else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                    {
                        effect.UpdatePerObject(this.Material, this.StartRadius, this.EndRadius, this.TextureCount, this.Textures);
                    }

                    #endregion

                    this.DeviceContext.InputAssembler.InputLayout = effect.GetInputLayout(technique);
                    Counters.IAInputLayoutSets++;
                    this.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
                    Counters.IAPrimitiveTopologySets++;
                    this.DeviceContext.InputAssembler.SetVertexBuffers(0, this.VertexBufferBinding);
                    Counters.IAVertexBuffersSets++;
                    this.DeviceContext.InputAssembler.SetIndexBuffer(null, Format.R32_UInt, 0);
                    Counters.IAIndexBufferSets++;

                    for (int p = 0; p < technique.Description.PassCount; p++)
                    {
                        technique.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                        this.DeviceContext.Draw(this.VertexCount, 0);

                        Counters.DrawCallsPerFrame++;
                        Counters.InstancesPerFrame++;
                    }
                }
            }
        }
        /// <summary>
        /// Writes vertex data
        /// </summary>
        /// <param name="data">Vertex data</param>
        public void WriteData(VertexData[] data)
        {
            var vData = VertexData.Convert(VertexTypes.Billboard, data, null, null, Matrix.Identity);

            this.Game.Graphics.DeviceContext.WriteBuffer(this.VertexBuffer, Array.ConvertAll(vData, v => (VertexBillboard)v));
        }
    }
}
