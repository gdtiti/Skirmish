﻿using SharpDX;
using System;
using System.Collections.Generic;
using Buffer = SharpDX.Direct3D11.Buffer;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;
    using Engine.Helpers;

    public class Terrain2 : Drawable
    {
        public const int MaxPatchesHighLevel = 4;
        public const int MaxPatchesMediumLevel = 6;
        public const int MaxPatchesLowLevel = 8;
        public const int MaxPatchesDataLoadLevel = 10;

        private HeightMap heightMap = null;
        private IndexBufferDictionary indices = new IndexBufferDictionary();
        private TerrainPatchDictionary patches = new TerrainPatchDictionary();
        private QuadTree quadTree = null;
        private QuadTreeNode[] nodesToDraw = null;

        public Terrain2(Game game, TerrainDescription description)
            : base(game)
        {
            ImageContent heightMapImage = new ImageContent()
            {
                Streams = ContentManager.FindContent(description.ContentPath, description.Heightmap.HeightmapFileName),
            };

            //Read heightmap
            this.heightMap = HeightMap.FromStream(heightMapImage.Stream);

            //Get vertices and indices
            VertexData[] vertices;
            uint[] indices;
            this.heightMap.BuildGeometry(
                description.Heightmap.CellSize,
                description.Heightmap.MaximumHeight,
                out vertices, out indices);

            //Initialize Quadtree
            this.quadTree = QuadTree.Build(
                game,
                vertices,
                description);

            //Intialize Indices
            this.InitializeIndices(description.Quadtree.MaxTrianglesPerNode);

            //Initialize patches
            this.InitializePatches(description.Quadtree.MaxTrianglesPerNode);
        }

        public override void Dispose()
        {
            Helper.Dispose(this.indices);
            Helper.Dispose(this.patches);
        }

        private void InitializeIndices(int trianglesPerNode)
        {
            this.indices.Add(LevelOfDetailEnum.High, new Dictionary<IndexBufferShapeEnum, IndexBuffer>());
            this.indices.Add(LevelOfDetailEnum.Medium, new Dictionary<IndexBufferShapeEnum, IndexBuffer>());
            this.indices.Add(LevelOfDetailEnum.Low, new Dictionary<IndexBufferShapeEnum, IndexBuffer>());

            //High level
            for (int i = 0; i < 9; i++)
            {
                IndexBufferShapeEnum shape = (IndexBufferShapeEnum)i;

                uint[] indexList = IndexBufferDictionary.GenerateIndices(shape, trianglesPerNode);
                IndexBuffer buffer = new IndexBuffer()
                {
                    Buffer = this.Game.Graphics.Device.CreateIndexBufferImmutable(indexList),
                    Count = indexList.Length,
                };
                this.indices[LevelOfDetailEnum.High].Add(shape, buffer);
            }

            //Medium level
            for (int i = 0; i < 9; i++)
            {
                IndexBufferShapeEnum shape = (IndexBufferShapeEnum)i;

                uint[] indexList = IndexBufferDictionary.GenerateIndices(shape, trianglesPerNode / 4);
                IndexBuffer buffer = new IndexBuffer()
                {
                    Buffer = this.Game.Graphics.Device.CreateIndexBufferImmutable(indexList),
                    Count = indexList.Length,
                };
                this.indices[LevelOfDetailEnum.Medium].Add(shape, buffer);
            }

            //Low level
            {
                IndexBufferShapeEnum shape = IndexBufferShapeEnum.Full;

                uint[] indexList = IndexBufferDictionary.GenerateIndices(shape, trianglesPerNode / 4 / 4);
                IndexBuffer buffer = new IndexBuffer()
                {
                    Buffer = this.Game.Graphics.Device.CreateIndexBufferImmutable(indexList),
                    Count = indexList.Length,
                };
                this.indices[LevelOfDetailEnum.Low].Add(shape, buffer);
            }
        }

        private void InitializePatches(int trianglesPerNode)
        {
            this.patches.Add(LevelOfDetailEnum.High, new TerrainPatch[MaxPatchesHighLevel]);
            this.patches.Add(LevelOfDetailEnum.Medium, new TerrainPatch[MaxPatchesMediumLevel]);
            this.patches.Add(LevelOfDetailEnum.Low, new TerrainPatch[MaxPatchesLowLevel]);
            this.patches.Add(LevelOfDetailEnum.DataLoad, new TerrainPatch[MaxPatchesDataLoadLevel]);

            for (int i = 0; i < MaxPatchesHighLevel; i++)
            {
                var patch = TerrainPatch.CreatePatch(this.Game, LevelOfDetailEnum.High, trianglesPerNode);
                this.patches[LevelOfDetailEnum.High][i] = patch;
            }

            for (int i = 0; i < MaxPatchesMediumLevel; i++)
            {
                var patch = TerrainPatch.CreatePatch(this.Game, LevelOfDetailEnum.Medium, trianglesPerNode);
                this.patches[LevelOfDetailEnum.Medium][i] = patch;
            }

            for (int i = 0; i < MaxPatchesLowLevel; i++)
            {
                var patch = TerrainPatch.CreatePatch(this.Game, LevelOfDetailEnum.Low, trianglesPerNode);
                this.patches[LevelOfDetailEnum.Low][i] = patch;
            }

            for (int i = 0; i < MaxPatchesDataLoadLevel; i++)
            {
                var patch = TerrainPatch.CreatePatch(this.Game, LevelOfDetailEnum.DataLoad, trianglesPerNode);
                this.patches[LevelOfDetailEnum.DataLoad][i] = patch;
            }
        }

        public override void Update(GameTime gameTime)
        {

        }

        public override void Draw(GameTime gameTime, Context context)
        {
            this.nodesToDraw = this.quadTree.GetNodesToDraw(ref context.Frustum);

            if (this.nodesToDraw != null && this.nodesToDraw.Length > 0)
            {
                //Sort by distance to eye position
                Array.Sort(this.nodesToDraw, (n1, n2) =>
                {
                    float d1 = Vector3.DistanceSquared(n1.Center, context.EyePosition);
                    float d2 = Vector3.DistanceSquared(n2.Center, context.EyePosition);

                    return d1.CompareTo(d2);
                });

                //Assign level of detail by order
                int patchesHighLevel = 0;
                int patchesMediumLevel = 0;
                int patchesLowLevel = 0;
                int patchesDataLoadLevel = 0;

                this.patches.Reset();

                for (int i = 0; i < this.nodesToDraw.Length; i++)
                {
                    if (patchesHighLevel < MaxPatchesHighLevel)
                    {
                        this.patches[LevelOfDetailEnum.High][patchesHighLevel].Action = TerrainPatchActionEnum.Draw;
                        this.patches[LevelOfDetailEnum.High][patchesHighLevel].Current = this.nodesToDraw[i];
                        this.patches[LevelOfDetailEnum.High][patchesHighLevel].IndexBuffer = this.indices[LevelOfDetailEnum.High][IndexBufferShapeEnum.Full];

                        patchesHighLevel++;
                    }
                    else if (patchesMediumLevel < MaxPatchesMediumLevel)
                    {
                        this.patches[LevelOfDetailEnum.Medium][patchesMediumLevel].Action = TerrainPatchActionEnum.Draw;
                        this.patches[LevelOfDetailEnum.Medium][patchesMediumLevel].Current = this.nodesToDraw[i];
                        this.patches[LevelOfDetailEnum.Medium][patchesMediumLevel].IndexBuffer = this.indices[LevelOfDetailEnum.Medium][IndexBufferShapeEnum.Full];

                        patchesMediumLevel++;
                    }
                    else if (patchesLowLevel < MaxPatchesLowLevel)
                    {
                        this.patches[LevelOfDetailEnum.Low][patchesLowLevel].Action = TerrainPatchActionEnum.Draw;
                        this.patches[LevelOfDetailEnum.Low][patchesLowLevel].Current = this.nodesToDraw[i];
                        this.patches[LevelOfDetailEnum.Low][patchesLowLevel].IndexBuffer = this.indices[LevelOfDetailEnum.Low][IndexBufferShapeEnum.Full];

                        patchesLowLevel++;
                    }
                    else if (patchesDataLoadLevel < MaxPatchesDataLoadLevel)
                    {
                        this.patches[LevelOfDetailEnum.DataLoad][patchesDataLoadLevel].Action = TerrainPatchActionEnum.Load;
                        this.patches[LevelOfDetailEnum.DataLoad][patchesDataLoadLevel].Current = this.nodesToDraw[i];
                        this.patches[LevelOfDetailEnum.DataLoad][patchesDataLoadLevel].IndexBuffer = null;

                        patchesDataLoadLevel++;
                    }
                    else
                    {
                        this.nodesToDraw[i].Cull = true;
                    }
                }

                this.patches.Draw(this.Game, gameTime, context);
            }
        }
    }

    public enum LevelOfDetailEnum
    {
        None = 0,
        High = 1,
        Medium = 2,
        Low = 3,
        DataLoad = 4,
    }

    public enum IndexBufferShapeEnum : int
    {
        Full = 0,
        SideTop = 1,
        SideBottom = 2,
        SideLeft = 3,
        SideRight = 4,
        CornerTopLeft = 5,
        CornerBottomLeft = 6,
        CornerTopRight = 7,
        CornerBottomRight = 8,
    }

    public class IndexBuffer
    {
        public Buffer Buffer;
        public int Count;
    }

    public class IndexBufferDictionary : Dictionary<LevelOfDetailEnum, Dictionary<IndexBufferShapeEnum, IndexBuffer>>
    {
        public static uint[] GenerateIndices(IndexBufferShapeEnum bufferShape, int trianglesPerNode)
        {
            int nodes = trianglesPerNode / 2;
            uint side = (uint)Math.Sqrt(nodes);
            uint sideLoss = side / 2;

            bool topSide =
                bufferShape == IndexBufferShapeEnum.CornerTopLeft ||
                bufferShape == IndexBufferShapeEnum.CornerTopRight ||
                bufferShape == IndexBufferShapeEnum.SideTop;

            bool bottomSide =
                bufferShape == IndexBufferShapeEnum.CornerBottomLeft ||
                bufferShape == IndexBufferShapeEnum.CornerBottomRight ||
                bufferShape == IndexBufferShapeEnum.SideBottom;

            bool leftSide =
                bufferShape == IndexBufferShapeEnum.CornerBottomLeft ||
                bufferShape == IndexBufferShapeEnum.CornerTopLeft ||
                bufferShape == IndexBufferShapeEnum.SideLeft;

            bool rightSide =
                bufferShape == IndexBufferShapeEnum.CornerBottomRight ||
                bufferShape == IndexBufferShapeEnum.CornerTopRight ||
                bufferShape == IndexBufferShapeEnum.SideRight;

            uint totalTriangles = (uint)trianglesPerNode;
            if (topSide) totalTriangles -= sideLoss;
            if (bottomSide) totalTriangles -= sideLoss;
            if (leftSide) totalTriangles -= sideLoss;
            if (rightSide) totalTriangles -= sideLoss;

            uint[] indices = new uint[totalTriangles * 3];

            int index = 0;

            for (uint y = 1; y < side; y += 2)
            {
                for (uint x = 1; x < side; x += 2)
                {
                    uint indexPRow = ((y - 1) * side) + x;
                    uint indexCRow = ((y + 0) * side) + x;
                    uint indexNRow = ((y + 1) * side) + x;

                    //Top side
                    if (y == 1 && topSide)
                    {
                        //Top
                        indices[index++] = indexCRow;
                        indices[index++] = indexPRow - 1;
                        indices[index++] = indexPRow + 1;
                    }
                    else
                    {
                        //Top left
                        indices[index++] = indexCRow;
                        indices[index++] = indexPRow - 1;
                        indices[index++] = indexPRow;
                        //Top right
                        indices[index++] = indexCRow;
                        indices[index++] = indexPRow;
                        indices[index++] = indexPRow + 1;
                    }

                    //Bottom side
                    if (y == side - 1 && bottomSide)
                    {
                        //Bottom only
                        indices[index++] = indexCRow;
                        indices[index++] = indexNRow + 1;
                        indices[index++] = indexNRow - 1;
                    }
                    else
                    {
                        //Bottom left
                        indices[index++] = indexCRow;
                        indices[index++] = indexNRow;
                        indices[index++] = indexNRow - 1;
                        //Bottom right
                        indices[index++] = indexCRow;
                        indices[index++] = indexNRow + 1;
                        indices[index++] = indexNRow;
                    }

                    //Left side
                    if (x == 1 && leftSide)
                    {
                        //Left only
                        indices[index++] = indexCRow;
                        indices[index++] = indexPRow - 1;
                        indices[index++] = indexNRow - 1;
                    }
                    else
                    {
                        //Left top
                        indices[index++] = indexCRow;
                        indices[index++] = indexCRow - 1;
                        indices[index++] = indexPRow - 1;
                        //Left bottom
                        indices[index++] = indexCRow;
                        indices[index++] = indexNRow - 1;
                        indices[index++] = indexCRow - 1;
                    }

                    //Right side
                    if (x == side - 1 && rightSide)
                    {
                        //Right only
                        indices[index++] = indexCRow;
                        indices[index++] = indexPRow + 1;
                        indices[index++] = indexNRow + 1;
                    }
                    else
                    {
                        //Right top
                        indices[index++] = indexCRow;
                        indices[index++] = indexPRow + 1;
                        indices[index++] = indexCRow + 1;
                        //Right bottom
                        indices[index++] = indexCRow;
                        indices[index++] = indexCRow + 1;
                        indices[index++] = indexNRow + 1;
                    }
                }
            }

            return indices;
        }
    }

    public enum TerrainPatchActionEnum
    {
        None,
        Draw,
        Load,
    }

    public class TerrainPatch : IDisposable
    {
        public static TerrainPatch CreatePatch(Game game, LevelOfDetailEnum detail, int trianglesPerNode)
        {
            int triangleCount = 0;

            if (detail == LevelOfDetailEnum.High) triangleCount = trianglesPerNode;
            else if (detail == LevelOfDetailEnum.Medium) triangleCount = trianglesPerNode / 4;
            else if (detail == LevelOfDetailEnum.Low) triangleCount = trianglesPerNode / 16;

            if (triangleCount > 0)
            {
                //Vertices
                int vertices = (int)Math.Pow((Math.Sqrt(triangleCount / 4) + 1), 2);

                VertexPositionNormalTextureTangent[] vertexData = new VertexPositionNormalTextureTangent[vertices];

                var vertexBuffer = game.Graphics.Device.CreateVertexBufferWrite(vertexData);
                var vertexBufferBinding = new[]
                {
                    new VertexBufferBinding(vertexBuffer, default(VertexPositionNormalTextureTangent).Stride, 0),
                };

                return new TerrainPatch(game)
                {
                    vertexBuffer = vertexBuffer,
                    vertexBufferBinding = vertexBufferBinding,
                };
            }
            else
            {
                return new TerrainPatch(game);
            }
        }

        private QuadTreeNode current = null;
        private Buffer vertexBuffer = null;
        private VertexBufferBinding[] vertexBufferBinding = null;

        public readonly Game Game;
        public TerrainPatchActionEnum Action = TerrainPatchActionEnum.None;
        public QuadTreeNode Current
        {
            get
            {
                return this.current;
            }
            set
            {
                if (this.current != value)
                {
                    this.current = value;

                    if (this.current != null)
                    {
                        VertexData.WriteVertexBuffer(
                            this.Game.Graphics.DeviceContext,
                            this.vertexBuffer,
                            this.current.GetVertexData(VertexTypes.PositionNormalTextureTangent));
                    }
                }
            }
        }
        public IndexBuffer IndexBuffer = null;
        public Dictionary<int, Drawable[]> Drawables = new Dictionary<int, Drawable[]>();

        public TerrainPatch(Game game)
        {
            this.Game = game;
        }

        public void Dispose()
        {
            Helper.Dispose(this.vertexBuffer);
            Helper.Dispose(this.Drawables);
        }

        public void Reset()
        {
            this.Action = TerrainPatchActionEnum.None;
        }

        public void Draw(GameTime gameTime, Context context, EffectTechnique technique)
        {
            this.Game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);
            this.Game.Graphics.DeviceContext.InputAssembler.SetIndexBuffer(this.IndexBuffer.Buffer, SharpDX.DXGI.Format.R32_UInt, 0);

            for (int p = 0; p < technique.Description.PassCount; p++)
            {
                technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                this.Game.Graphics.DeviceContext.DrawIndexed(this.IndexBuffer.Count, 0, 0);

                Counters.DrawCallsPerFrame++;
                Counters.InstancesPerFrame++;
            }
        }
    }

    public class TerrainPatchDictionary : Dictionary<LevelOfDetailEnum, TerrainPatch[]>
    {
        public void Reset()
        {
            foreach (var item in this.Values)
            {
                if (item != null && item.Length > 0)
                {
                    for (int i = 0; i < item.Length; i++)
                    {
                        item[i].Reset();
                    }
                }
            }
        }

        public void Draw(Game game, GameTime gameTime, Context context)
        {
            Drawer effect = null;
            if (context.DrawerMode == DrawerModesEnum.Forward) effect = DrawerPool.EffectBasic;
            else if (context.DrawerMode == DrawerModesEnum.Deferred) effect = DrawerPool.EffectGBuffer;
            else if (context.DrawerMode == DrawerModesEnum.ShadowMap) effect = DrawerPool.EffectShadow;

            if (effect != null)
            {
                game.Graphics.SetDepthStencilZEnabled();

                #region Per frame update

                if (context.DrawerMode == DrawerModesEnum.Forward)
                {
                    ((EffectBasic)effect).UpdatePerFrame(
                        context.World,
                        context.ViewProjection,
                        context.EyePosition,
                        context.Lights,
                        context.ShadowMap,
                        context.FromLightViewProjection);
                }
                else if (context.DrawerMode == DrawerModesEnum.Deferred)
                {
                    ((EffectBasicGBuffer)effect).UpdatePerFrame(
                        context.World,
                        context.ViewProjection);
                }
                else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                {
                    ((EffectBasicShadow)effect).UpdatePerFrame(
                        context.World,
                        context.ViewProjection);
                }

                #endregion

                #region Per skinning update

                if (context.DrawerMode == DrawerModesEnum.Forward)
                {
                    ((EffectBasic)effect).UpdatePerSkinning(null);
                }
                else if (context.DrawerMode == DrawerModesEnum.Deferred)
                {
                    ((EffectBasicGBuffer)effect).UpdatePerSkinning(null);
                }
                else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                {
                    ((EffectBasicShadow)effect).UpdatePerSkinning(null);
                }

                #endregion

                #region Per object update

                if (context.DrawerMode == DrawerModesEnum.Forward)
                {
                    ((EffectBasic)effect).UpdatePerObject(Material.Default, null, null, 0);
                }
                else if (context.DrawerMode == DrawerModesEnum.Deferred)
                {
                    ((EffectBasicGBuffer)effect).UpdatePerObject(Material.Default, null, null, 0);
                }

                #endregion

                var technique = effect.GetTechnique(VertexTypes.PositionNormalTextureTangent, DrawingStages.Drawing);

                game.Graphics.DeviceContext.InputAssembler.InputLayout = effect.GetInputLayout(technique);
                game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

                foreach (var lod in this.Keys)
                {
                    foreach (var item in this[lod])
                    {
                        if (item.Action == TerrainPatchActionEnum.Draw)
                        {
                            item.Draw(gameTime, context, technique);
                        }
                    }
                }
            }
        }
    }
}
