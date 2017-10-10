﻿using SharpDX;
using SharpDX.Direct3D;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Minimap
    /// </summary>
    public class SpriteTexture : Drawable, IScreenFitted, ITransformable2D
    {
        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        private BufferDescriptor vertexBuffer = null;
        /// <summary>
        /// Index buffer descriptor
        /// </summary>
        private BufferDescriptor indexBuffer = null;
        /// <summary>
        /// View * projection for 2D projection
        /// </summary>
        private Matrix viewProjection = Matrix.Identity;
        /// <summary>
        /// Drawing channels
        /// </summary>
        private SpriteTextureChannelsEnum channels = SpriteTextureChannelsEnum.None;

        /// <summary>
        /// Sprite initial width
        /// </summary>
        public float Width { get; private set; }
        /// <summary>
        /// Sprite initial height
        /// </summary>
        public float Height { get; private set; }
        /// <summary>
        /// Manipulator
        /// </summary>
        public Manipulator2D Manipulator { get; private set; }
        /// <summary>
        /// Texture
        /// </summary>
        public EngineShaderResourceView Texture { get; set; }
        /// <summary>
        /// Drawing channels
        /// </summary>
        public SpriteTextureChannelsEnum Channels
        {
            get
            {
                return this.channels;
            }
            set
            {
                if (this.channels != value)
                {
                    this.channels = value;
                }
            }
        }

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Sprite texture description</param>
        public SpriteTexture(Scene scene, SpriteTextureDescription description)
            : base(scene, description)
        {
            Vector3[] cv;
            Vector2[] cuv;
            uint[] ci;
            GeometryUtil.CreateSprite(
                Vector2.Zero,
                1, 1,
                0, 0,
                out cv,
                out cuv,
                out ci);

            var vertices = VertexPositionTexture.Generate(cv, cuv);

            this.vertexBuffer = this.BufferManager.Add(description.Name, vertices, false, 0);
            this.indexBuffer = this.BufferManager.Add(description.Name, ci, false);

            this.Channels = description.Channel;

            this.Manipulator = new Manipulator2D();
            this.Manipulator.SetPosition(description.Left, description.Top);
            this.Manipulator.Update(new GameTime(), this.Game.Form.RelativeCenter, description.Width, description.Height);

            Matrix view;
            Matrix proj;
            Sprite.CreateViewOrthoProjection(
                this.Game.Form.RenderWidth,
                this.Game.Form.RenderHeight,
                out view,
                out proj);

            this.viewProjection = view * proj;

            this.Width = description.Width;
            this.Height = description.Height;
        }
        /// <summary>
        /// Dispose objects
        /// </summary>
        public override void Dispose()
        {

        }
        /// <summary>
        /// Update state
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {

        }
        /// <summary>
        /// Draw objects
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            this.BufferManager.SetIndexBuffer(this.indexBuffer.Slot);

            if (context.DrawerMode != DrawerModesEnum.ShadowMap)
            {
                Counters.InstancesPerFrame++;
                Counters.PrimitivesPerFrame += this.indexBuffer.Count / 3;
            }

            var technique = DrawerPool.EffectDefaultSprite.GetTechnique(VertexTypes.PositionTexture, false, DrawingStages.Drawing, context.DrawerMode, this.Channels);

            this.BufferManager.SetInputAssembler(technique, this.vertexBuffer.Slot, PrimitiveTopology.TriangleList);

            DrawerPool.EffectDefaultSprite.UpdatePerFrame(this.Manipulator.LocalTransform, this.viewProjection);
            DrawerPool.EffectDefaultSprite.UpdatePerObject(Color.White, this.Texture, 0);

            for (int p = 0; p < technique.PassCount; p++)
            {
                technique.Apply(this.Game.Graphics, p, 0);

                this.Graphics.DeviceContext.DrawIndexed(this.indexBuffer.Count, this.indexBuffer.Offset, this.vertexBuffer.Offset);

                Counters.DrawCallsPerFrame++;
            }
        }
        /// <summary>
        /// Screen resize
        /// </summary>
        public virtual void Resize()
        {
            this.viewProjection = Sprite.CreateViewOrthoProjection(this.Game.Form.RenderWidth, this.Game.Form.RenderHeight);
        }
        /// <summary>
        /// Object resize
        /// </summary>
        /// <param name="width">New width</param>
        /// <param name="height">New height</param>
        public virtual void ResizeSprite(float width, float height)
        {
            this.Manipulator.Update(new GameTime(), this.Game.Form.RelativeCenter, width, height);
        }
    }
}
