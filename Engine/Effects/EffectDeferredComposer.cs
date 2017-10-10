﻿using SharpDX;
using System;
using Device = SharpDX.Direct3D11.Device;
using EffectMatrixVariable = SharpDX.Direct3D11.EffectMatrixVariable;
using EffectScalarVariable = SharpDX.Direct3D11.EffectScalarVariable;
using EffectShaderResourceVariable = SharpDX.Direct3D11.EffectShaderResourceVariable;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using EffectVariable = SharpDX.Direct3D11.EffectVariable;
using EffectVectorVariable = SharpDX.Direct3D11.EffectVectorVariable;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Font effect
    /// </summary>
    public class EffectDeferredComposer : Drawer
    {
        /// <summary>
        /// Directional light technique
        /// </summary>
        public readonly EffectTechnique DeferredDirectionalLight = null;
        /// <summary>
        /// Point stencil technique
        /// </summary>
        public readonly EffectTechnique DeferredPointStencil = null;
        /// <summary>
        /// Point light technique
        /// </summary>
        public readonly EffectTechnique DeferredPointLight = null;
        /// <summary>
        /// Spot stencil technique
        /// </summary>
        public readonly EffectTechnique DeferredSpotStencil = null;
        /// <summary>
        /// Spot light technique
        /// </summary>
        public readonly EffectTechnique DeferredSpotLight = null;
        /// <summary>
        /// Technique to combine all light sources
        /// </summary>
        public readonly EffectTechnique DeferredCombineLights = null;

        /// <summary>
        /// World matrix effect variable
        /// </summary>
        private EffectMatrixVariable world = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;
        /// <summary>
        /// View * projection from light matrix for low definition shadows
        /// </summary>
        private EffectMatrixVariable lightViewProjectionLD = null;
        /// <summary>
        /// View * projection from light matrix for high definition shadows
        /// </summary>
        private EffectMatrixVariable lightViewProjectionHD = null;
        /// <summary>
        /// Eye position effect variable
        /// </summary>
        private EffectVectorVariable eyePositionWorld = null;
        /// <summary>
        /// Global ambient light effect variable;
        /// </summary>
        private EffectScalarVariable globalAmbient;
        /// <summary>
        /// Directional light effect variable
        /// </summary>
        private EffectVariable directionalLight = null;
        /// <summary>
        /// Point light effect variable
        /// </summary>
        private EffectVariable pointLight = null;
        /// <summary>
        /// Spot light effect variable
        /// </summary>
        private EffectVariable spotLight = null;
        /// <summary>
        /// Fog start effect variable
        /// </summary>
        private EffectScalarVariable fogStart = null;
        /// <summary>
        /// Fog range effect variable
        /// </summary>
        private EffectScalarVariable fogRange = null;
        /// <summary>
        /// Fog color effect variable
        /// </summary>
        private EffectVectorVariable fogColor = null;
        /// <summary>
        /// Shadow maps flag effect variable
        /// </summary>
        private EffectScalarVariable shadowMaps = null;
        /// <summary>
        /// Color Map effect variable
        /// </summary>
        private EffectShaderResourceVariable tg1Map = null;
        /// <summary>
        /// Normal Map effect variable
        /// </summary>
        private EffectShaderResourceVariable tg2Map = null;
        /// <summary>
        /// Depth Map effect variable
        /// </summary>
        private EffectShaderResourceVariable tg3Map = null;
        /// <summary>
        /// Low definition shadow map effect variable
        /// </summary>
        private EffectShaderResourceVariable shadowMapLD = null;
        /// <summary>
        /// High definition shadow map effect variable
        /// </summary>
        private EffectShaderResourceVariable shadowMapHD = null;
        /// <summary>
        /// Light Map effect variable
        /// </summary>
        private EffectShaderResourceVariable lightMap = null;
        /// <summary>
        /// Material palette width effect variable
        /// </summary>
        private EffectScalarVariable materialPaletteWidth = null;
        /// <summary>
        /// Materials palette
        /// </summary>
        private EffectShaderResourceVariable materialPalette = null;
        /// <summary>
        /// Level of detail ranges effect variable
        /// </summary>
        private EffectVectorVariable lod = null;

        /// <summary>
        /// Current target 1
        /// </summary>
        private ShaderResourceView currentTG1Map = null;
        /// <summary>
        /// Current target 2
        /// </summary>
        private ShaderResourceView currentTG2Map = null;
        /// <summary>
        /// Current target 3
        /// </summary>
        private ShaderResourceView currentTG3Map = null;
        /// <summary>
        /// Current low definition shadow map
        /// </summary>
        private ShaderResourceView currentShadowMapLD = null;
        /// <summary>
        /// Current high definition shadow map
        /// </summary>
        private ShaderResourceView currentShadowMapHD = null;
        /// <summary>
        /// Current light map
        /// </summary>
        private ShaderResourceView currentLightMap = null;
        /// <summary>
        /// Current material palette
        /// </summary>
        private ShaderResourceView currentMaterialPalette = null;

        /// <summary>
        /// World matrix
        /// </summary>
        protected Matrix World
        {
            get
            {
                return this.world.GetMatrix();
            }
            set
            {
                this.world.SetMatrix(value);
            }
        }
        /// <summary>
        /// World view projection matrix
        /// </summary>
        protected Matrix WorldViewProjection
        {
            get
            {
                return this.worldViewProjection.GetMatrix();
            }
            set
            {
                this.worldViewProjection.SetMatrix(value);
            }
        }
        /// <summary>
        /// View * projection from light matrix for low definition shadows
        /// </summary>
        protected Matrix LightViewProjectionLD
        {
            get
            {
                return this.lightViewProjectionLD.GetMatrix();
            }
            set
            {
                this.lightViewProjectionLD.SetMatrix(value);
            }
        }
        /// <summary>
        /// View * projection from light matrix for high definition shadows
        /// </summary>
        protected Matrix LightViewProjectionHD
        {
            get
            {
                return this.lightViewProjectionHD.GetMatrix();
            }
            set
            {
                this.lightViewProjectionHD.SetMatrix(value);
            }
        }
        /// <summary>
        /// Camera eye position
        /// </summary>
        protected Vector3 EyePositionWorld
        {
            get
            {
                Vector4 v = this.eyePositionWorld.GetFloatVector();

                return new Vector3(v.X, v.Y, v.Z);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, value.Z, 1f);

                this.eyePositionWorld.Set(v4);
            }
        }
        /// <summary>
        /// Global almbient light intensity
        /// </summary>
        protected float GlobalAmbient
        {
            get
            {
                return this.globalAmbient.GetFloat();
            }
            set
            {
                this.globalAmbient.Set(value);
            }
        }
        /// <summary>
        /// Directional light
        /// </summary>
        protected BufferDirectionalLight DirectionalLight
        {
            get
            {
                using (DataStream ds = this.directionalLight.GetRawValue(default(BufferDirectionalLight).GetStride()))
                {
                    ds.Position = 0;

                    return ds.Read<BufferDirectionalLight>();
                }
            }
            set
            {
                using (DataStream ds = DataStream.Create<BufferDirectionalLight>(new BufferDirectionalLight[] { value }, true, false))
                {
                    ds.Position = 0;

                    this.directionalLight.SetRawValue(ds, default(BufferDirectionalLight).GetStride());
                }
            }
        }
        /// <summary>
        /// Point light
        /// </summary>
        protected BufferPointLight PointLight
        {
            get
            {
                using (DataStream ds = this.pointLight.GetRawValue(default(BufferPointLight).GetStride()))
                {
                    ds.Position = 0;

                    return ds.Read<BufferPointLight>();
                }
            }
            set
            {
                using (DataStream ds = DataStream.Create<BufferPointLight>(new BufferPointLight[] { value }, true, false))
                {
                    ds.Position = 0;

                    this.pointLight.SetRawValue(ds, default(BufferPointLight).GetStride());
                }
            }
        }
        /// <summary>
        /// Spot light
        /// </summary>
        protected BufferSpotLight SpotLight
        {
            get
            {
                using (DataStream ds = this.spotLight.GetRawValue(default(BufferSpotLight).GetStride()))
                {
                    ds.Position = 0;

                    return ds.Read<BufferSpotLight>();
                }
            }
            set
            {
                using (DataStream ds = DataStream.Create<BufferSpotLight>(new BufferSpotLight[] { value }, true, false))
                {
                    ds.Position = 0;

                    this.spotLight.SetRawValue(ds, default(BufferSpotLight).GetStride());
                }
            }
        }
        /// <summary>
        /// Fog start distance
        /// </summary>
        protected float FogStart
        {
            get
            {
                return this.fogStart.GetFloat();
            }
            set
            {
                this.fogStart.Set(value);
            }
        }
        /// <summary>
        /// Fog range distance
        /// </summary>
        protected float FogRange
        {
            get
            {
                return this.fogRange.GetFloat();
            }
            set
            {
                this.fogRange.Set(value);
            }
        }
        /// <summary>
        /// Fog color
        /// </summary>
        protected Color4 FogColor
        {
            get
            {
                return new Color4(this.fogColor.GetFloatVector());
            }
            set
            {
                this.fogColor.Set(value);
            }
        }
        /// <summary>
        /// Shadow maps flag
        /// </summary>
        protected int ShadowMaps
        {
            get
            {
                return this.shadowMaps.GetInt();
            }
            set
            {
                this.shadowMaps.Set(value);
            }
        }
        /// <summary>
        /// Color Map
        /// </summary>
        protected ShaderResourceView TG1Map
        {
            get
            {
                return this.tg1Map.GetResource();
            }
            set
            {
                if (this.currentTG1Map != value)
                {
                    this.tg1Map.SetResource(value);

                    this.currentTG1Map = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Normal Map
        /// </summary>
        protected ShaderResourceView TG2Map
        {
            get
            {
                return this.tg2Map.GetResource();
            }
            set
            {
                if (this.currentTG2Map != value)
                {
                    this.tg2Map.SetResource(value);

                    this.currentTG2Map = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Depth Map
        /// </summary>
        protected ShaderResourceView TG3Map
        {
            get
            {
                return this.tg3Map.GetResource();
            }
            set
            {
                if (this.currentTG3Map != value)
                {
                    this.tg3Map.SetResource(value);

                    this.currentTG3Map = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Low definition shadow map
        /// </summary>
        protected ShaderResourceView ShadowMapLD
        {
            get
            {
                return this.shadowMapLD.GetResource();
            }
            set
            {
                if (this.currentShadowMapLD != value)
                {
                    this.shadowMapLD.SetResource(value);

                    this.currentShadowMapLD = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// High definition shadow map
        /// </summary>
        protected ShaderResourceView ShadowMapHD
        {
            get
            {
                return this.shadowMapHD.GetResource();
            }
            set
            {
                if (this.currentShadowMapHD != value)
                {
                    this.shadowMapHD.SetResource(value);

                    this.currentShadowMapHD = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Light Map
        /// </summary>
        protected ShaderResourceView LightMap
        {
            get
            {
                return this.lightMap.GetResource();
            }
            set
            {
                if (this.currentLightMap != value)
                {
                    this.lightMap.SetResource(value);

                    this.currentLightMap = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Material palette width
        /// </summary>
        protected uint MaterialPaletteWidth
        {
            get
            {
                return (uint)this.materialPaletteWidth.GetFloat();
            }
            set
            {
                this.materialPaletteWidth.Set((float)value);
            }
        }
        /// <summary>
        /// Material palette
        /// </summary>
        protected ShaderResourceView MaterialPalette
        {
            get
            {
                return this.materialPalette.GetResource();
            }
            set
            {
                if (this.currentMaterialPalette != value)
                {
                    this.materialPalette.SetResource(value);

                    this.currentMaterialPalette = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Level of detail ranges
        /// </summary>
        protected Vector3 LOD
        {
            get
            {
                var v = this.lod.GetFloatVector();

                return new Vector3(v.X, v.Y, v.Z);
            }
            set
            {
                var v = new Vector4(value, 0);

                this.lod.Set(v);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDeferredComposer(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.DeferredDirectionalLight = this.Effect.GetTechniqueByName("DeferredDirectionalLight");
            this.DeferredPointStencil = this.Effect.GetTechniqueByName("DeferredPointStencil");
            this.DeferredPointLight = this.Effect.GetTechniqueByName("DeferredPointLight");
            this.DeferredSpotStencil = this.Effect.GetTechniqueByName("DeferredSpotStencil");
            this.DeferredSpotLight = this.Effect.GetTechniqueByName("DeferredSpotLight");
            this.DeferredCombineLights = this.Effect.GetTechniqueByName("DeferredCombineLights");

            this.world = this.Effect.GetVariableByName("gWorld").AsMatrix();
            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.lightViewProjectionLD = this.Effect.GetVariableByName("gLightViewProjectionLD").AsMatrix();
            this.lightViewProjectionHD = this.Effect.GetVariableByName("gLightViewProjectionHD").AsMatrix();
            this.eyePositionWorld = this.Effect.GetVariableByName("gEyePositionWorld").AsVector();
            this.globalAmbient = this.Effect.GetVariableByName("gGlobalAmbient").AsScalar();
            this.directionalLight = this.Effect.GetVariableByName("gDirLight");
            this.pointLight = this.Effect.GetVariableByName("gPointLight");
            this.spotLight = this.Effect.GetVariableByName("gSpotLight");
            this.fogStart = this.Effect.GetVariableByName("gFogStart").AsScalar();
            this.fogRange = this.Effect.GetVariableByName("gFogRange").AsScalar();
            this.fogColor = this.Effect.GetVariableByName("gFogColor").AsVector();
            this.shadowMaps = this.Effect.GetVariableByName("gShadows").AsScalar();
            this.tg1Map = this.Effect.GetVariableByName("gTG1Map").AsShaderResource();
            this.tg2Map = this.Effect.GetVariableByName("gTG2Map").AsShaderResource();
            this.tg3Map = this.Effect.GetVariableByName("gTG3Map").AsShaderResource();
            this.shadowMapLD = this.Effect.GetVariableByName("gShadowMapLD").AsShaderResource();
            this.shadowMapHD = this.Effect.GetVariableByName("gShadowMapHD").AsShaderResource();
            this.lightMap = this.Effect.GetVariableByName("gLightMap").AsShaderResource();
            this.materialPaletteWidth = this.Effect.GetVariableByName("gMaterialPaletteWidth").AsScalar();
            this.materialPalette = this.Effect.GetVariableByName("gMaterialPalette").AsShaderResource();
            this.lod = this.Effect.GetVariableByName("gLOD").AsVector();
        }
        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="instanced">Use instancing data</param>
        /// <param name="stage">Stage</param>
        /// <param name="mode">Mode</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public override EffectTechnique GetTechnique(VertexTypes vertexType, bool instanced, DrawingStages stage, DrawerModesEnum mode)
        {
            throw new Exception("Use technique variables directly");
        }

        /// <summary>
        /// Update effect globals
        /// </summary>
        /// <param name="materialPalette">Material palette texture</param>
        /// <param name="materialPaletteWidth">Material palette texture width</param>
        /// <param name="lod1">High level of detail maximum distance</param>
        /// <param name="lod2">Medium level of detail maximum distance</param>
        /// <param name="lod3">Low level of detail maximum distance</param>
        public void UpdateGlobals(
            ShaderResourceView materialPalette,
            uint materialPaletteWidth,
            float lod1,
            float lod2,
            float lod3)
        {
            this.MaterialPalette = materialPalette;
            this.MaterialPaletteWidth = materialPaletteWidth;

            this.LOD = new Vector3(lod1, lod2, lod3);
        }
        /// <summary>
        /// Updates per frame variables
        /// </summary>
        /// <param name="world">World matrix</param>
        /// <param name="viewProjection">View * projection matrix</param>
        /// <param name="eyePositionWorld">Eye position in world coordinates</param>
        /// <param name="colorMap">Color map texture</param>
        /// <param name="normalMap">Normal map texture</param>
        /// <param name="depthMap">Depth map texture</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            Vector3 eyePositionWorld,
            ShaderResourceView colorMap,
            ShaderResourceView normalMap,
            ShaderResourceView depthMap)
        {
            this.World = world;
            this.WorldViewProjection = world * viewProjection;
            this.EyePositionWorld = eyePositionWorld;

            this.TG1Map = colorMap;
            this.TG2Map = normalMap;
            this.TG3Map = depthMap;
        }
        /// <summary>
        /// Updates per directional light variables
        /// </summary>
        /// <param name="light">Light</param>
        /// <param name="lightViewProjectionLD">View * projection from light matrix for low definition shadows</param>
        /// <param name="lightViewProjectionHD">View * projection from light matrix for high definition shadows</param>
        /// <param name="shadowMaps">Shadow map flags</param>
        /// <param name="shadowMapLD">Low definition shadow map texture</param>
        /// <param name="shadowMapHD">High definition shadow map texture</param>
        public void UpdatePerLight(
            SceneLightDirectional light,
            Matrix lightViewProjectionLD,
            Matrix lightViewProjectionHD,
            int shadowMaps,
            ShaderResourceView shadowMapLD,
            ShaderResourceView shadowMapHD)
        {
            this.DirectionalLight = new BufferDirectionalLight(light);

            this.LightViewProjectionLD = lightViewProjectionLD;
            this.LightViewProjectionHD = lightViewProjectionHD;
            this.ShadowMapLD = shadowMapLD;
            this.ShadowMapHD = shadowMapHD;
            this.ShadowMaps = shadowMaps;
        }
        /// <summary>
        /// Updates per spot light variables
        /// </summary>
        /// <param name="light">Light</param>
        /// <param name="transform">Translation matrix</param>
        /// <param name="viewProjection">View * projection matrix</param>
        public void UpdatePerLight(
            SceneLightPoint light,
            Matrix transform,
            Matrix viewProjection)
        {
            this.PointLight = new BufferPointLight(light);
            this.World = transform;
            this.WorldViewProjection = transform * viewProjection;
        }
        /// <summary>
        /// Updates per spot light variables
        /// </summary>
        /// <param name="light">Light</param>
        /// <param name="transform">Translation and rotation matrix</param>
        /// <param name="viewProjection">View * projection matrix</param>
        public void UpdatePerLight(
            SceneLightSpot light,
            Matrix transform,
            Matrix viewProjection)
        {
            this.SpotLight = new BufferSpotLight(light);
            this.World = transform;
            this.WorldViewProjection = transform * viewProjection;
        }
        /// <summary>
        /// Updates composer variables
        /// </summary>
        /// <param name="world">World matrix</param>
        /// <param name="viewProjection">View * projection matrix</param>
        /// <param name="eyePositionWorld">Eye position in world coordinates</param>
        /// <param name="globalAmbient">Global ambient</param>
        /// <param name="fogStart">Fog start</param>
        /// <param name="fogRange">Fog range</param>
        /// <param name="fogColor">Fog color</param>
        /// <param name="depthMap">Depth map texture</param>
        /// <param name="lightMap">Light map</param>
        public void UpdateComposer(
            Matrix world,
            Matrix viewProjection,
            Vector3 eyePositionWorld,
            float globalAmbient,
            float fogStart,
            float fogRange,
            Color4 fogColor,
            ShaderResourceView depthMap,
            ShaderResourceView lightMap)
        {
            this.WorldViewProjection = world * viewProjection;
            this.EyePositionWorld = eyePositionWorld;

            this.GlobalAmbient = globalAmbient;
            this.FogStart = fogStart;
            this.FogRange = fogRange;
            this.FogColor = fogColor;

            this.TG3Map = depthMap;
            this.LightMap = lightMap;
        }
    }
}
