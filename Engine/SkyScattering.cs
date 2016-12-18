﻿using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using Buffer = SharpDX.Direct3D11.Buffer;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;
    using Engine.Helpers;

    /// <summary>
    /// Scattered sky
    /// </summary>
    public class SkyScattering : Drawable
    {
        /// <summary>
        /// Index buffer
        /// </summary>
        private Buffer indexBuffer = null;
        /// <summary>
        /// Index count
        /// </summary>
        private int indexCount = 0;
        /// <summary>
        /// Vertex buffer
        /// </summary>
        private Buffer vertexBuffer = null;
        /// <summary>
        /// Vertex buffer binding
        /// </summary>
        private VertexBufferBinding[] vertexBufferBinding = null;

        /// <summary>
        /// Rayleigh scattering constant value
        /// </summary>
        private float rayleighScattering;
        /// <summary>
        /// Mie scattering constant value
        /// </summary>
        private float mieScattering;
        /// <summary>
        /// Wave length
        /// </summary>
        private Color4 wavelength;
        /// <summary>
        /// Sphere inner radius
        /// </summary>
        private float sphereInnerRadius;
        /// <summary>
        /// Sphere outer radius
        /// </summary>
        private float sphereOuterRadius;

        /// <summary>
        /// Time of day controller
        /// </summary>
        protected TimeOfDay TimeOfDayController = null;
        /// <summary>
        /// Scene directional light (the sun light)
        /// </summary>
        protected SceneLightDirectional Light;
        /// <summary>
        /// Rayleigh scattering * 4 * PI
        /// </summary>
        protected float RayleighScattering4PI { get; private set; }
        /// <summary>
        /// Mie scattering * 4 * PI
        /// </summary>
        protected float MieScattering4PI { get; private set; }
        /// <summary>
        /// Inverse wave length * 4
        /// </summary>
        protected Color4 InvWaveLength4 { get; private set; }
        /// <summary>
        /// Scattering Scale
        /// </summary>
        protected float ScatteringScale { get; private set; }

        /// <summary>
        /// Planet radius
        /// </summary>
        public float PlanetRadius { get; set; }
        /// <summary>
        /// Planet atmosphere radius from surface
        /// </summary>
        public float PlanetAtmosphereRadius { get; set; }
        /// <summary>
        /// Rayleigh scattering constant value
        /// </summary>
        public float RayleighScattering
        {
            get
            {
                return this.rayleighScattering;
            }
            set
            {
                this.rayleighScattering = value;
                this.RayleighScattering4PI = value * 4.0f * MathUtil.Pi;
            }
        }
        /// <summary>
        /// Rayleigh scale depth value
        /// </summary>
        public float RayleighScaleDepth { get; set; }
        /// <summary>
        /// Mie scattering constant value
        /// </summary>
        public float MieScattering
        {
            get
            {
                return this.mieScattering;
            }
            set
            {
                this.mieScattering = value;
                this.MieScattering4PI = value * 4.0f * MathUtil.Pi;
            }
        }
        /// <summary>
        /// Mie phase assymetry value
        /// </summary>
        public float MiePhaseAssymetry { get; set; }
        /// <summary>
        /// Mie scale depth value
        /// </summary>
        public float MieScaleDepth { get; set; }
        /// <summary>
        /// Light wave length
        /// </summary>
        public Color4 WaveLength
        {
            get
            {
                return this.wavelength;
            }
            set
            {
                this.wavelength = value;
                this.InvWaveLength4 = new Color4(
                    1f / (float)Math.Pow(value.Red, 4.0f),
                    1f / (float)Math.Pow(value.Green, 4.0f),
                    1f / (float)Math.Pow(value.Blue, 4.0f),
                    1.0f);
            }
        }
        /// <summary>
        /// Sky brightness
        /// </summary>
        public float Brightness { get; set; }
        /// <summary>
        /// Sphere inner radius
        /// </summary>
        public float SphereInnerRadius
        {
            get
            {
                return this.sphereInnerRadius;
            }
            set
            {
                this.sphereInnerRadius = value;

                this.CalcScale();
            }
        }
        /// <summary>
        /// Sphere outter radius
        /// </summary>
        public float SphereOuterRadius
        {
            get
            {
                return this.sphereOuterRadius;
            }
            set
            {
                this.sphereOuterRadius = value;

                this.CalcScale();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="description">Sky scattering description class</param>
        public SkyScattering(Game game, SkyScatteringDescription description)
            : base(game, description)
        {
            this.Cull = false;

            this.TimeOfDayController = new TimeOfDay();

            this.PlanetRadius = description.PlanetRadius;
            this.PlanetAtmosphereRadius = description.PlanetAtmosphereRadius;

            this.RayleighScattering = description.RayleighScattering;
            this.RayleighScaleDepth = description.RayleighScaleDepth;
            this.MieScattering = description.MieScattering;
            this.MiePhaseAssymetry = description.MiePhaseAssymetry;
            this.MieScaleDepth = description.MieScaleDepth;

            this.WaveLength = description.WaveLength;
            this.Brightness = description.Brightness;

            this.sphereInnerRadius = 1.0f;
            this.sphereOuterRadius = this.sphereInnerRadius * 1.025f;
            this.CalcScale();

            this.InitializeBuffers();
        }
        /// <summary>
        /// Resource releasing
        /// </summary>
        public override void Dispose()
        {
            Helper.Dispose(this.vertexBuffer);
            Helper.Dispose(this.indexBuffer);
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="context">Updating context</param>
        public override void Update(UpdateContext context)
        {
            this.TimeOfDayController.Update(context.GameTime.ElapsedSeconds);

            this.Light.Direction = this.TimeOfDayController.LightDirection;
        }
        /// <summary>
        /// Draws content
        /// </summary>
        /// <param name="context">Drawing context</param>
        public override void Draw(DrawContext context)
        {
            EffectSkyScattering effect = DrawerPool.EffectSkyScattering;
            EffectTechnique technique = effect.SkyScattering;

            effect.UpdatePerFrame(
                Matrix.Translation(context.EyePosition),
                context.ViewProjection,
                this.PlanetRadius,
                this.PlanetAtmosphereRadius,
                this.SphereOuterRadius,
                this.SphereInnerRadius,
                this.Brightness,
                this.RayleighScattering,
                this.RayleighScattering4PI,
                this.MieScattering,
                this.MieScattering4PI,
                this.InvWaveLength4,
                this.ScatteringScale,
                this.RayleighScaleDepth,
                this.Light.Direction);

            //Sets vertex and index buffer
            this.Game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);
            Counters.IAVertexBuffersSets++;
            this.Game.Graphics.DeviceContext.InputAssembler.SetIndexBuffer(this.indexBuffer, Format.R32_UInt, 0);
            Counters.IAIndexBufferSets++;
            this.Game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            Counters.IAPrimitiveTopologySets++;

            for (int p = 0; p < technique.Description.PassCount; p++)
            {
                technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                this.Game.Graphics.DeviceContext.DrawIndexed(this.indexCount, 0, 0);

                Counters.DrawCallsPerFrame++;
                Counters.InstancesPerFrame++;
                Counters.TrianglesPerFrame += this.indexCount / 3;
            }
        }

        /// <summary>
        /// Begins sky animation
        /// </summary>
        /// <param name="light">Light used as sun light</param>
        /// <param name="startTime">Start time</param>
        /// <param name="speed">Animation speed</param>
        public void BeginAnimation(SceneLightDirectional light, float startTime, float speed)
        {
            this.Light = light;

            this.TimeOfDayController.BeginAnimation(startTime, speed);
        }
        /// <summary>
        /// Begins sky animation
        /// </summary>
        /// <param name="light">Light used as sun light</param>
        /// <param name="startTime">Start time</param>
        /// <param name="speed">Animation speed</param>
        public void BeginAnimation(SceneLightDirectional light, TimeSpan startTime, float speed)
        {
            this.Light = light;

            this.TimeOfDayController.BeginAnimation(startTime, speed);
        }

        /// <summary>
        /// Initialize buffers
        /// </summary>
        private void InitializeBuffers()
        {
            VertexData[] vData;
            uint[] iData;
            VertexData.CreateSphere(1, 10, 10, out vData, out iData);

            List<VertexPosition> vertices = new List<VertexPosition>();
            Array.ForEach(vData, v => { vertices.Add(VertexData.CreateVertexPosition(v)); });

            var indices = Helper.ChangeCoordinate(iData);

            this.vertexBuffer = this.Game.Graphics.Device.CreateVertexBufferImmutable(vertices.ToArray());
            this.vertexBufferBinding = new[]
            {
                new VertexBufferBinding(this.vertexBuffer, vertices[0].Stride, 0),
            };

            this.indexBuffer = this.Game.Graphics.Device.CreateIndexBufferImmutable(indices);
            this.indexCount = indices.Length;
        }
        /// <summary>
        /// Calc current scattering scale from sphere radius values
        /// </summary>
        private void CalcScale()
        {
            this.ScatteringScale = 1.0f / (this.SphereOuterRadius - this.SphereInnerRadius);
        }
    }
}