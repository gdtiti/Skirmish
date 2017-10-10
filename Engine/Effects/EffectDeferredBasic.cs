﻿using SharpDX;
using System;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class EffectDeferredBasic : Drawer
    {
        /// <summary>
        /// Position color drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionColor = null;
        /// <summary>
        /// Position color skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionColorSkinned = null;
        /// <summary>
        /// Position normal color drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionNormalColor = null;
        /// <summary>
        /// Position normal color skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionNormalColorSkinned = null;
        /// <summary>
        /// Position texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionTexture = null;
        /// <summary>
        /// Position texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionTextureSkinned = null;
        /// <summary>
        /// Position normal texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionNormalTexture = null;
        /// <summary>
        /// Position normal texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionNormalTextureSkinned = null;
        /// <summary>
        /// Position normal texture tangent drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionNormalTextureTangent = null;
        /// <summary>
        /// Position normal texture tangent skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionNormalTextureTangentSkinned = null;
        /// <summary>
        /// Position color drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionColor = null;
        /// <summary>
        /// Position color skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionColorSkinned = null;
        /// <summary>
        /// Position normal color drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionNormalColor = null;
        /// <summary>
        /// Position normal color skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionNormalColorSkinned = null;
        /// <summary>
        /// Position texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionTexture = null;
        /// <summary>
        /// Position texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionTextureSkinned = null;
        /// <summary>
        /// Position normal texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionNormalTexture = null;
        /// <summary>
        /// Position normal texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionNormalTextureSkinned = null;
        /// <summary>
        /// Position normal texture tangent drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionNormalTextureTangent = null;
        /// <summary>
        /// Position normal texture tangent skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionNormalTextureTangentSkinned = null;

        /// <summary>
        /// World matrix effect variable
        /// </summary>
        private EngineEffectVariableMatrix world = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EngineEffectVariableMatrix worldViewProjection = null;
        /// <summary>
        /// Animation data effect variable
        /// </summary>
        private EngineEffectVariableScalar animationOffset = null;
        /// <summary>
        /// Material index effect variable
        /// </summary>
        private EngineEffectVariableScalar materialIndex = null;
        /// <summary>
        /// Texture index effect variable
        /// </summary>
        private EngineEffectVariableScalar textureIndex = null;
        /// <summary>
        /// Diffuse map effect variable
        /// </summary>
        private EngineEffectVariableTexture diffuseMap = null;
        /// <summary>
        /// Normal map effect variable
        /// </summary>
        private EngineEffectVariableTexture normalMap = null;
        /// <summary>
        /// Specular map effect variable
        /// </summary>
        private EngineEffectVariableTexture specularMap = null;
        /// <summary>
        /// Animation palette width effect variable
        /// </summary>
        private EngineEffectVariableScalar animationPaletteWidth = null;
        /// <summary>
        /// Animation palette
        /// </summary>
        private EngineEffectVariableTexture animationPalette = null;

        /// <summary>
        /// Current diffuse map
        /// </summary>
        private EngineShaderResourceView currentDiffuseMap = null;
        /// <summary>
        /// Current normal map
        /// </summary>
        private EngineShaderResourceView currentNormalMap = null;
        /// <summary>
        /// Current specular map
        /// </summary>
        private EngineShaderResourceView currentSpecularMap = null;
        /// <summary>
        /// Current animation palette
        /// </summary>
        private EngineShaderResourceView currentAnimationPalette = null;

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
        /// Animation data
        /// </summary>
        protected uint AnimationOffset
        {
            get
            {
                return (uint)this.animationOffset.GetInt();
            }
            set
            {
                this.animationOffset.Set(value);
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
        /// Texture index
        /// </summary>
        protected uint TextureIndex
        {
            get
            {
                return (uint)this.textureIndex.GetFloat();
            }
            set
            {
                this.textureIndex.Set((float)value);
            }
        }
        /// <summary>
        /// Diffuse map
        /// </summary>
        protected EngineShaderResourceView DiffuseMap
        {
            get
            {
                return this.diffuseMap.GetResource();
            }
            set
            {
                if (this.currentDiffuseMap != value)
                {
                    this.diffuseMap.SetResource(value);

                    this.currentDiffuseMap = value;

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
        /// Specular map
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
        /// Animation palette width
        /// </summary>
        protected uint AnimationPaletteWidth
        {
            get
            {
                return (uint)this.animationPaletteWidth.GetFloat();
            }
            set
            {
                this.animationPaletteWidth.Set((float)value);
            }
        }
        /// <summary>
        /// Animation palette
        /// </summary>
        protected EngineShaderResourceView AnimationPalette
        {
            get
            {
                return this.animationPalette.GetResource();
            }
            set
            {
                if (this.currentAnimationPalette != value)
                {
                    this.animationPalette.SetResource(value);

                    this.currentAnimationPalette = value;

                    Counters.TextureUpdates++;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDeferredBasic(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            this.PositionColor = this.Effect.GetTechniqueByName("PositionColor");
            this.PositionColorSkinned = this.Effect.GetTechniqueByName("PositionColorSkinned");
            this.PositionNormalColor = this.Effect.GetTechniqueByName("PositionNormalColor");
            this.PositionNormalColorSkinned = this.Effect.GetTechniqueByName("PositionNormalColorSkinned");
            this.PositionTexture = this.Effect.GetTechniqueByName("PositionTexture");
            this.PositionTextureSkinned = this.Effect.GetTechniqueByName("PositionTextureSkinned");
            this.PositionNormalTexture = this.Effect.GetTechniqueByName("PositionNormalTexture");
            this.PositionNormalTextureSkinned = this.Effect.GetTechniqueByName("PositionNormalTextureSkinned");
            this.PositionNormalTextureTangent = this.Effect.GetTechniqueByName("PositionNormalTextureTangent");
            this.PositionNormalTextureTangentSkinned = this.Effect.GetTechniqueByName("PositionNormalTextureTangentSkinned");
            this.InstancingPositionColor = this.Effect.GetTechniqueByName("PositionColorI");
            this.InstancingPositionColorSkinned = this.Effect.GetTechniqueByName("PositionColorSkinnedI");
            this.InstancingPositionNormalColor = this.Effect.GetTechniqueByName("PositionNormalColorI");
            this.InstancingPositionNormalColorSkinned = this.Effect.GetTechniqueByName("PositionNormalColorSkinnedI");
            this.InstancingPositionTexture = this.Effect.GetTechniqueByName("PositionTextureI");
            this.InstancingPositionTextureSkinned = this.Effect.GetTechniqueByName("PositionTextureSkinnedI");
            this.InstancingPositionNormalTexture = this.Effect.GetTechniqueByName("PositionNormalTextureI");
            this.InstancingPositionNormalTextureSkinned = this.Effect.GetTechniqueByName("PositionNormalTextureSkinnedI");
            this.InstancingPositionNormalTextureTangent = this.Effect.GetTechniqueByName("PositionNormalTextureTangentI");
            this.InstancingPositionNormalTextureTangentSkinned = this.Effect.GetTechniqueByName("PositionNormalTextureTangentSkinnedI");

            this.world = this.Effect.GetVariableMatrix("gWorld");
            this.worldViewProjection = this.Effect.GetVariableMatrix("gWorldViewProjection");
            this.animationOffset = this.Effect.GetVariableScalar("gAnimationOffset");
            this.materialIndex = this.Effect.GetVariableScalar("gMaterialIndex");
            this.textureIndex = this.Effect.GetVariableScalar("gTextureIndex");
            this.diffuseMap = this.Effect.GetVariableTexture("gDiffuseMapArray");
            this.normalMap = this.Effect.GetVariableTexture("gNormalMapArray");
            this.specularMap = this.Effect.GetVariableTexture("gSpecularMapArray");
            this.animationPaletteWidth = this.Effect.GetVariableScalar("gAnimationPaletteWidth");
            this.animationPalette = this.Effect.GetVariableTexture("gAnimationPalette");
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
            if (stage == DrawingStages.Drawing)
            {
                if (mode == DrawerModesEnum.Deferred)
                {
                    switch (vertexType)
                    {
                        case VertexTypes.PositionColor:
                            return instanced ? this.InstancingPositionColor : this.PositionColor;
                        case VertexTypes.PositionTexture:
                            return instanced ? this.InstancingPositionTexture : this.PositionTexture;
                        case VertexTypes.PositionNormalColor:
                            return instanced ? this.InstancingPositionNormalColor : this.PositionNormalColor;
                        case VertexTypes.PositionNormalTexture:
                            return instanced ? this.InstancingPositionNormalTexture : this.PositionNormalTexture;
                        case VertexTypes.PositionNormalTextureTangent:
                            return instanced ? this.InstancingPositionNormalTextureTangent : this.PositionNormalTextureTangent;
                        case VertexTypes.PositionColorSkinned:
                            return instanced ? this.InstancingPositionColorSkinned : this.PositionColorSkinned;
                        case VertexTypes.PositionTextureSkinned:
                            return instanced ? this.InstancingPositionTextureSkinned : this.PositionTextureSkinned;
                        case VertexTypes.PositionNormalColorSkinned:
                            return instanced ? this.InstancingPositionNormalColorSkinned : this.PositionNormalColorSkinned;
                        case VertexTypes.PositionNormalTextureSkinned:
                            return instanced ? this.InstancingPositionNormalTextureSkinned : this.PositionNormalTextureSkinned;
                        case VertexTypes.PositionNormalTextureTangentSkinned:
                            return instanced ? this.InstancingPositionNormalTextureTangentSkinned : this.PositionNormalTextureTangentSkinned;
                        default:
                            throw new Exception(string.Format("Bad vertex type for effect and stage: {0} - {1}", vertexType, stage));
                    }
                }
                else
                {
                    throw new Exception(string.Format("Bad mode for effect: {0}", mode));
                }
            }
            else
            {
                throw new Exception(string.Format("Bad stage for effect: {0}", stage));
            }
        }

        /// <summary>
        /// Update effect globals
        /// </summary>
        /// <param name="animationPalette">Animation palette texture</param>
        /// <param name="animationPaletteWith">Animation palette texture width</param>
        public void UpdateGlobals(
            EngineShaderResourceView animationPalette,
            uint animationPaletteWidth)
        {
            this.AnimationPalette = animationPalette;
            this.AnimationPaletteWidth = animationPaletteWidth;
        }
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World Matrix</param>
        /// <param name="viewProjection">View * projection</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection)
        {
            this.World = world;
            this.WorldViewProjection = world * viewProjection;
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="diffuseMap">Diffuse map</param>
        /// <param name="normalMap">Normal map</param>
        /// <param name="specularMap">Specular map</param>
        /// <param name="materialIndex">Material index</param>
        /// <param name="textureIndex">Texture index</param>
        /// <param name="animationOffset">Animation index</param>
        public void UpdatePerObject(
            EngineShaderResourceView diffuseMap,
            EngineShaderResourceView normalMap,
            EngineShaderResourceView specularMap,
            uint materialIndex,
            uint textureIndex,
            uint animationOffset)
        {
            this.DiffuseMap = diffuseMap;
            this.NormalMap = normalMap;
            this.SpecularMap = specularMap;
            this.MaterialIndex = materialIndex;
            this.TextureIndex = textureIndex;

            this.AnimationOffset = animationOffset;
        }
    }
}
