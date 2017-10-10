﻿using SharpDX;
using System;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class EffectDefaultTerrain : Drawer
    {
        /// <summary>
        /// Forward with alpha map drawing technique
        /// </summary>
        public readonly EngineEffectTechnique TerrainAlphaMapForward = null;
        /// <summary>
        /// Forward with slopes drawing technique
        /// </summary>
        public readonly EngineEffectTechnique TerrainSlopesForward = null;
        /// <summary>
        /// Forward full drawing technique
        /// </summary>
        public readonly EngineEffectTechnique TerrainFullForward = null;

        /// <summary>
        /// Directional lights effect variable
        /// </summary>
        private EngineEffectVariable dirLights = null;
        /// <summary>
        /// Point lights effect variable
        /// </summary>
        private EngineEffectVariable pointLights = null;
        /// <summary>
        /// Spot light effect variable
        /// </summary>
        private EngineEffectVariable spotLights = null;
        /// <summary>
        /// Global ambient light effect variable;
        /// </summary>
        private EngineEffectVariableScalar globalAmbient;
        /// <summary>
        /// Light count effect variable
        /// </summary>
        private EngineEffectVariableVector lightCount = null;
        /// <summary>
        /// Eye position effect variable
        /// </summary>
        private EngineEffectVariableVector eyePositionWorld = null;
        /// <summary>
        /// Fog start effect variable
        /// </summary>
        private EngineEffectVariableScalar fogStart = null;
        /// <summary>
        /// Fog range effect variable
        /// </summary>
        private EngineEffectVariableScalar fogRange = null;
        /// <summary>
        /// Fog color effect variable
        /// </summary>
        private EngineEffectVariableVector fogColor = null;
        /// <summary>
        /// Shadow maps flag effect variable
        /// </summary>
        private EngineEffectVariableScalar shadowMaps = null;
        /// <summary>
        /// World matrix effect variable
        /// </summary>
        private EngineEffectVariableMatrix world = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EngineEffectVariableMatrix worldViewProjection = null;
        /// <summary>
        /// Texture resolution effect variable
        /// </summary>
        private EngineEffectVariableScalar textureResolution = null;
        /// <summary>
        /// From light View * Projection transform for low definition shadows
        /// </summary>
        private EngineEffectVariableMatrix fromLightViewProjectionLD = null;
        /// <summary>
        /// From light View * Projection transform for high definition shadows
        /// </summary>
        private EngineEffectVariableMatrix fromLightViewProjectionHD = null;
        /// <summary>
        /// Use diffuse map color variable
        /// </summary>
        private EngineEffectVariableScalar useColorDiffuse = null;
        /// <summary>
        /// Use specular map color variable
        /// </summary>
        private EngineEffectVariableScalar useColorSpecular = null;
        /// <summary>
        /// Material index effect variable
        /// </summary>
        private EngineEffectVariableScalar materialIndex = null;
        /// <summary>
        /// Low resolution textures effect variable
        /// </summary>
        private EngineEffectVariableTexture diffuseMapLR = null;
        /// <summary>
        /// High resolution textures effect variable
        /// </summary>
        private EngineEffectVariableTexture diffuseMapHR = null;
        /// <summary>
        /// Normal map effect variable
        /// </summary>
        private EngineEffectVariableTexture normalMap = null;
        /// <summary>
        /// Specular map effect variable
        /// </summary>
        private EngineEffectVariableTexture specularMap = null;
        /// <summary>
        /// Low definition shadow map effect variable
        /// </summary>
        private EngineEffectVariableTexture shadowMapLD = null;
        /// <summary>
        /// High definition shadow map effect variable
        /// </summary>
        private EngineEffectVariableTexture shadowMapHD = null;
        /// <summary>
        /// Color texture array effect variable
        /// </summary>
        private EngineEffectVariableTexture colorTextures = null;
        /// <summary>
        /// Alpha map effect variable
        /// </summary>
        private EngineEffectVariableTexture alphaMap = null;
        /// <summary>
        /// Slope ranges effect variable
        /// </summary>
        private EngineEffectVariableVector parameters = null;
        /// <summary>
        /// Material palette width effect variable
        /// </summary>
        private EngineEffectVariableScalar materialPaletteWidth = null;
        /// <summary>
        /// Material palette
        /// </summary>
        private EngineEffectVariableTexture materialPalette = null;
        /// <summary>
        /// Level of detail ranges effect variable
        /// </summary>
        private EngineEffectVariableVector lod = null;

        /// <summary>
        /// Current diffuse map (Low resolution)
        /// </summary>
        private EngineShaderResourceView currentDiffuseMapLR = null;
        /// <summary>
        /// Current normal map (High resolution)
        /// </summary>
        private EngineShaderResourceView currentDiffuseMapHR = null;
        /// <summary>
        /// Current normal map
        /// </summary>
        private EngineShaderResourceView currentNormalMap = null;
        /// <summary>
        /// Current specular map
        /// </summary>
        private EngineShaderResourceView currentSpecularMap = null;
        /// <summary>
        /// Current low definition shadow map
        /// </summary>
        private EngineShaderResourceView currentShadowMapLD = null;
        /// <summary>
        /// Current high definition shadow map
        /// </summary>
        private EngineShaderResourceView currentShadowMapHD = null;
        /// <summary>
        /// Current color texure array
        /// </summary>
        private EngineShaderResourceView currentColorTextures = null;
        /// <summary>
        /// Current alpha map
        /// </summary>
        private EngineShaderResourceView currentAlphaMap = null;
        /// <summary>
        /// Current material palette
        /// </summary>
        private EngineShaderResourceView currentMaterialPalette = null;

        /// <summary>
        /// Directional lights
        /// </summary>
        protected BufferDirectionalLight[] DirLights
        {
            get
            {
                return this.dirLights.GetValue<BufferDirectionalLight>(BufferDirectionalLight.MAX);
            }
            set
            {
                this.dirLights.SetValue(value, BufferDirectionalLight.MAX);
            }
        }
        /// <summary>
        /// Point light
        /// </summary>
        protected BufferPointLight[] PointLights
        {
            get
            {
                return this.pointLights.GetValue<BufferPointLight>(BufferPointLight.MAX);
            }
            set
            {
                this.pointLights.SetValue(value, BufferPointLight.MAX);
            }
        }
        /// <summary>
        /// Spot light
        /// </summary>
        protected BufferSpotLight[] SpotLights
        {
            get
            {
                return this.spotLights.GetValue<BufferSpotLight>(BufferSpotLight.MAX);
            }
            set
            {
                this.spotLights.SetValue(value, BufferSpotLight.MAX);
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
        /// Light count
        /// </summary>
        protected int[] LightCount
        {
            get
            {
                Int4 v = this.lightCount.GetIntVector();

                return new int[] { v.X, v.Y, v.Z };
            }
            set
            {
                Int4 v4 = new Int4(value[0], value[1], value[2], 0);

                this.lightCount.Set(v4);
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
        /// Texture resolution
        /// </summary>
        protected float TextureResolution
        {
            get
            {
                return this.textureResolution.GetFloat();
            }
            set
            {
                this.textureResolution.Set(value);
            }
        }
        /// <summary>
        /// From light View * Projection transform for low definition shadows
        /// </summary>
        protected Matrix FromLightViewProjectionLD
        {
            get
            {
                return this.fromLightViewProjectionLD.GetMatrix();
            }
            set
            {
                this.fromLightViewProjectionLD.SetMatrix(value);
            }
        }
        /// <summary>
        /// From light View * Projection transform for high definition shadows
        /// </summary>
        protected Matrix FromLightViewProjectionHD
        {
            get
            {
                return this.fromLightViewProjectionHD.GetMatrix();
            }
            set
            {
                this.fromLightViewProjectionHD.SetMatrix(value);
            }
        }
        /// <summary>
        /// Use diffuse map color
        /// </summary>
        protected bool UseColorDiffuse
        {
            get
            {
                return this.useColorDiffuse.GetBool();
            }
            set
            {
                this.useColorDiffuse.Set(value);
            }
        }
        /// <summary>
        /// Use specular map color
        /// </summary>
        protected bool UseColorSpecular
        {
            get
            {
                return this.useColorSpecular.GetBool();
            }
            set
            {
                this.useColorSpecular.Set(value);
            }
        }
        /// <summary>
        /// Material index
        /// </summary>
        protected uint MaterialIndex
        {
            get
            {
                return (uint)this.materialIndex.GetFloat();
            }
            set
            {
                this.materialIndex.Set((float)value);
            }
        }
        /// <summary>
        /// Low resolution textures
        /// </summary>
        protected EngineShaderResourceView DiffuseMapLR
        {
            get
            {
                return this.diffuseMapLR.GetResource();
            }
            set
            {
                if (this.currentDiffuseMapLR != value)
                {
                    this.diffuseMapLR.SetResource(value);

                    this.currentDiffuseMapLR = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// High resolution textures
        /// </summary>
        protected EngineShaderResourceView DiffuseMapHR
        {
            get
            {
                return this.diffuseMapHR.GetResource();
            }
            set
            {
                if (this.currentDiffuseMapHR != value)
                {
                    this.diffuseMapHR.SetResource(value);

                    this.currentDiffuseMapHR = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Normal map
        /// </summary>
        protected EngineShaderResourceView NormalMap
        {
            get
            {
                return this.normalMap.GetResource();
            }
            set
            {
                if (this.currentNormalMap != value)
                {
                    this.normalMap.SetResource(value);

                    this.currentNormalMap = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Scpecular map
        /// </summary>
        protected EngineShaderResourceView SpecularMap
        {
            get
            {
                return this.specularMap.GetResource();
            }
            set
            {
                if (this.currentSpecularMap != value)
                {
                    this.specularMap.SetResource(value);

                    this.currentSpecularMap = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Low definition shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapLD
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
        protected EngineShaderResourceView ShadowMapHD
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
        /// Color textures for alpha map
        /// </summary>
        protected EngineShaderResourceView ColorTextures
        {
            get
            {
                return this.colorTextures.GetResource();
            }
            set
            {
                if (this.currentColorTextures != value)
                {
                    this.colorTextures.SetResource(value);

                    this.currentColorTextures = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Alpha map
        /// </summary>
        protected EngineShaderResourceView AlphaMap
        {
            get
            {
                return this.alphaMap.GetResource();
            }
            set
            {
                if (this.currentAlphaMap != value)
                {
                    this.alphaMap.SetResource(value);

                    this.currentAlphaMap = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Slope ranges
        /// </summary>
        protected Vector4 Parameters
        {
            get
            {
                return this.parameters.GetFloatVector();
            }
            set
            {
                this.parameters.Set(value);
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
        protected EngineShaderResourceView MaterialPalette
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
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDefaultTerrain(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            this.TerrainAlphaMapForward = this.Effect.GetTechniqueByName("TerrainAlphaMapForward");
            this.TerrainSlopesForward = this.Effect.GetTechniqueByName("TerrainSlopesForward");
            this.TerrainFullForward = this.Effect.GetTechniqueByName("TerrainFullForward");

            //Globals
            this.materialPaletteWidth = this.Effect.GetVariableScalar("gMaterialPaletteWidth");
            this.materialPalette = this.Effect.GetVariableTexture("gMaterialPalette");
            this.lod = this.Effect.GetVariableVector("gLOD");

            //Per frame
            this.world = this.Effect.GetVariableMatrix("gVSWorld");
            this.worldViewProjection = this.Effect.GetVariableMatrix("gVSWorldViewProjection");
            this.textureResolution = this.Effect.GetVariableScalar("gVSTextureResolution");

            this.fromLightViewProjectionLD = this.Effect.GetVariableMatrix("gPSLightViewProjectionLD");
            this.fromLightViewProjectionHD = this.Effect.GetVariableMatrix("gPSLightViewProjectionHD");
            this.eyePositionWorld = this.Effect.GetVariableVector("gPSEyePositionWorld");
            this.globalAmbient = this.Effect.GetVariableScalar("gPSGlobalAmbient");
            this.lightCount = this.Effect.GetVariableVector("gPSLightCount");
            this.shadowMaps = this.Effect.GetVariableScalar("gPSShadows");
            this.fogColor = this.Effect.GetVariableVector("gPSFogColor");
            this.fogStart = this.Effect.GetVariableScalar("gPSFogStart");
            this.fogRange = this.Effect.GetVariableScalar("gPSFogRange");
            this.dirLights = this.Effect.GetVariable("gPSDirLights");
            this.pointLights = this.Effect.GetVariable("gPSPointLights");
            this.spotLights = this.Effect.GetVariable("gPSSpotLights");
            this.shadowMapLD = this.Effect.GetVariableTexture("gPSShadowMapLD");
            this.shadowMapHD = this.Effect.GetVariableTexture("gPSShadowMapHD");

            //Per object
            this.parameters = this.Effect.GetVariableVector("gPSParams");
            this.useColorDiffuse = this.Effect.GetVariableScalar("gPSUseColorDiffuse");
            this.useColorSpecular = this.Effect.GetVariableScalar("gPSUseColorSpecular");
            this.materialIndex = this.Effect.GetVariableScalar("gPSMaterialIndex");
            this.normalMap = this.Effect.GetVariableTexture("gPSNormalMapArray");
            this.specularMap = this.Effect.GetVariableTexture("gPSSpecularMapArray");
            this.colorTextures = this.Effect.GetVariableTexture("gPSColorTextureArray");
            this.alphaMap = this.Effect.GetVariableTexture("gPSAlphaTexture");
            this.diffuseMapLR = this.Effect.GetVariableTexture("gPSDiffuseMapLRArray");
            this.diffuseMapHR = this.Effect.GetVariableTexture("gPSDiffuseMapHRArray");
        }
        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="instanced">Use instancing data</param>
        /// <param name="stage">Stage</param>
        /// <param name="mode">Mode</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public override EngineEffectTechnique GetTechnique(VertexTypes vertexType, bool instanced, DrawingStages stage, DrawerModesEnum mode)
        {
            EngineEffectTechnique technique = null;

            if (stage == DrawingStages.Drawing)
            {
                if (vertexType == VertexTypes.Terrain)
                {
                    if (mode == DrawerModesEnum.Forward) technique = this.TerrainFullForward;
                }
            }

            if (technique == null)
            {
                throw new Exception(string.Format("Bad vertex type for effect, stage and mode: {0} - {1} - {2}", vertexType, stage, mode));
            }

            return technique;
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
            EngineShaderResourceView materialPalette,
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
        /// Update per frame data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="eyePositionWorld">Eye position in world coordinates</param>
        /// <param name="textureResolution">Texture resolution</param>
        /// <param name="lights">Scene ligths</param>
        /// <param name="shadowMaps">Shadow map flags</param>
        /// <param name="shadowMapLD">Low definition shadow map texture</param>
        /// <param name="shadowMapHD">High definition shadow map texture</param>
        /// <param name="fromLightViewProjectionLD">From light View * Projection transform for low definition shadows</param>
        /// <param name="fromLightViewProjectionHD">From light View * Projection transform for high definition shadows</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            float textureResolution,
            Vector3 eyePositionWorld,
            SceneLights lights,
            int shadowMaps,
            EngineShaderResourceView shadowMapLD,
            EngineShaderResourceView shadowMapHD,
            Matrix fromLightViewProjectionLD,
            Matrix fromLightViewProjectionHD)
        {
            this.World = world;
            this.WorldViewProjection = world * viewProjection;
            this.TextureResolution = textureResolution;

            var globalAmbient = 0f;
            var bDirLights = new BufferDirectionalLight[BufferDirectionalLight.MAX];
            var bPointLights = new BufferPointLight[BufferPointLight.MAX];
            var bSpotLights = new BufferSpotLight[BufferSpotLight.MAX];
            var lCount = new[] { 0, 0, 0 };

            if (lights != null)
            {
                this.EyePositionWorld = eyePositionWorld;

                globalAmbient = lights.GlobalAmbientLight;

                var dirLights = lights.GetVisibleDirectionalLights();
                for (int i = 0; i < Math.Min(dirLights.Length, BufferDirectionalLight.MAX); i++)
                {
                    bDirLights[i] = new BufferDirectionalLight(dirLights[i]);
                }

                var pointLights = lights.GetVisiblePointLights();
                for (int i = 0; i < Math.Min(pointLights.Length, BufferPointLight.MAX); i++)
                {
                    bPointLights[i] = new BufferPointLight(pointLights[i]);
                }

                var spotLights = lights.GetVisibleSpotLights();
                for (int i = 0; i < Math.Min(spotLights.Length, BufferSpotLight.MAX); i++)
                {
                    bSpotLights[i] = new BufferSpotLight(spotLights[i]);
                }

                lCount[0] = Math.Min(dirLights.Length, BufferDirectionalLight.MAX);
                lCount[1] = Math.Min(pointLights.Length, BufferPointLight.MAX);
                lCount[2] = Math.Min(spotLights.Length, BufferSpotLight.MAX);

                this.FogStart = lights.FogStart;
                this.FogRange = lights.FogRange;
                this.FogColor = lights.FogColor;

                this.FromLightViewProjectionLD = fromLightViewProjectionLD;
                this.FromLightViewProjectionHD = fromLightViewProjectionHD;
                this.ShadowMapLD = shadowMapLD;
                this.ShadowMapHD = shadowMapHD;
                this.ShadowMaps = shadowMaps;
            }
            else
            {
                this.EyePositionWorld = Vector3.Zero;

                this.FogStart = 0;
                this.FogRange = 0;
                this.FogColor = Color.Transparent;

                this.FromLightViewProjectionLD = Matrix.Identity;
                this.FromLightViewProjectionHD = Matrix.Identity;
                this.ShadowMapLD = null;
                this.ShadowMapHD = null;
                this.ShadowMaps = 0;
            }

            this.GlobalAmbient = globalAmbient;
            this.DirLights = bDirLights;
            this.PointLights = bPointLights;
            this.SpotLights = bSpotLights;
            this.LightCount = lCount;
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="normalMap">Normal map</param>
        /// <param name="specularMap">Specular map</param>
        /// <param name="useAlphaMap">Use alpha mapping</param>
        /// <param name="alphaMap">Alpha map</param>
        /// <param name="colorTextures">Color textures</param>
        /// <param name="useSlopes">Use slope texturing</param>
        /// <param name="diffuseMapLR">Low resolution textures</param>
        /// <param name="diffuseMapHR">High resolution textures</param>
        /// <param name="slopeRanges">Slope ranges</param>
        /// <param name="proportion">Lerping proportion</param>
        /// <param name="materialIndex">Marerial index</param>
        public void UpdatePerObject(
            EngineShaderResourceView normalMap,
            EngineShaderResourceView specularMap,
            bool useAlphaMap,
            EngineShaderResourceView alphaMap,
            EngineShaderResourceView colorTextures,
            bool useSlopes,
            Vector2 slopeRanges,
            EngineShaderResourceView diffuseMapLR,
            EngineShaderResourceView diffuseMapHR,
            float proportion,
            uint materialIndex)
        {
            this.NormalMap = normalMap;
            this.SpecularMap = specularMap;
            this.UseColorSpecular = specularMap != null;

            this.AlphaMap = alphaMap;
            this.ColorTextures = colorTextures;
            this.UseColorDiffuse = colorTextures != null;

            this.DiffuseMapLR = diffuseMapLR;
            this.DiffuseMapHR = diffuseMapHR;

            this.Parameters = new Vector4(0, proportion, slopeRanges.X, slopeRanges.Y);

            this.MaterialIndex = materialIndex;
        }
    }
}
