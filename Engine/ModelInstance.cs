﻿using SharpDX;
using System;
using PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology;

namespace Engine
{
    using Engine.Animation;
    using Engine.Common;

    /// <summary>
    /// Model instance
    /// </summary>
    public class ModelInstance : ITransformable3D, IRayPickable<Triangle>, ICullable
    {
        /// <summary>
        /// Global id counter
        /// </summary>
        private static int InstanceId = 0;

        /// <summary>
        /// Model
        /// </summary>
        private BaseModel model = null;
        /// <summary>
        /// Update point cache flag
        /// </summary>
        private bool updatePoints = true;
        /// <summary>
        /// Update triangle cache flag
        /// </summary>
        private bool updateTriangles = true;
        /// <summary>
        /// Points caché
        /// </summary>
        private Vector3[] positionCache = null;
        /// <summary>
        /// Triangle list cache
        /// </summary>
        private Triangle[] triangleCache = null;
        /// <summary>
        /// Coarse bounding sphere
        /// </summary>
        private BoundingSphere coarseBoundingSphere;
        /// <summary>
        /// Bounding sphere
        /// </summary>
        private BoundingSphere boundingSphere;
        /// <summary>
        /// Bounding box
        /// </summary>
        private BoundingBox boundingBox;
        /// <summary>
        /// Gets if model has volumes
        /// </summary>
        private bool hasVolumes
        {
            get
            {
                var points = this.GetPoints();

                return points != null && points.Length > 0;
            }
        }
        /// <summary>
        /// Level of detail
        /// </summary>
        private LevelOfDetailEnum levelOfDetail = LevelOfDetailEnum.High;

        /// <summary>
        /// Instance id
        /// </summary>
        public readonly int Id;
        /// <summary>
        /// Manipulator
        /// </summary>
        public Manipulator3D Manipulator { get; private set; }
        /// <summary>
        /// Texture index
        /// </summary>
        public uint TextureIndex = 0;
        /// <summary>
        /// Active
        /// </summary>
        public bool Active = true;
        /// <summary>
        /// Visible
        /// </summary>
        public bool Visible = true;
        /// <summary>
        /// Instance level of detail
        /// </summary>
        public LevelOfDetailEnum LevelOfDetail
        {
            get
            {
                return this.levelOfDetail;
            }
            set
            {
                this.levelOfDetail = this.model.GetLODNearest(value);
            }
        }
        /// <summary>
        /// Animation controller
        /// </summary>
        public AnimationController AnimationController = new AnimationController();
        /// <summary>
        /// Gets the current instance lights collection
        /// </summary>
        public SceneLight[] Lights { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model">Model</param>
        public ModelInstance(BaseModel model)
        {
            this.Id = ++InstanceId;
            this.model = model;

            this.Manipulator = new Manipulator3D();
            this.Manipulator.Updated += new EventHandler(ManipulatorUpdated);

            var drawData = model.GetDrawingData(LevelOfDetailEnum.High);
            if (drawData != null)
            {
                this.coarseBoundingSphere = BoundingSphere.FromPoints(drawData.GetPoints(true));

                if (drawData.Lights != null && drawData.Lights.Length > 0)
                {
                    this.Lights = new SceneLight[drawData.Lights.Length];

                    for (int l = 0; l < drawData.Lights.Length; l++)
                    {
                        this.Lights[l] = drawData.Lights[l].Clone();
                    }
                }
            }
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="context">Context</param>
        public virtual void Update(UpdateContext context)
        {
            this.Manipulator.Update(context.GameTime);

            if (this.Lights != null && this.Lights.Length > 0)
            {
                for (int i = 0; i < this.Lights.Length; i++)
                {
                    this.Lights[i].ParentTransform = this.Manipulator.LocalTransform;
                }
            }
        }

        /// <summary>
        /// Sets a new manipulator to this instance
        /// </summary>
        /// <param name="manipulator">Manipulator</param>
        public void SetManipulator(Manipulator3D manipulator)
        {
            this.Manipulator.Updated -= ManipulatorUpdated;
            this.Manipulator = null;

            this.Manipulator = manipulator;
            this.Manipulator.Updated += ManipulatorUpdated;
        }
        /// <summary>
        /// Occurs when manipulator transform updated
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void ManipulatorUpdated(object sender, EventArgs e)
        {
            this.InvalidateCache();
        }

        /// <summary>
        /// Invalidates the internal caché
        /// </summary>
        public void InvalidateCache()
        {
            this.updatePoints = true;
            this.updateTriangles = true;

            this.boundingSphere = new BoundingSphere();
            this.boundingBox = new BoundingBox();
        }
        /// <summary>
        /// Gets point list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or position list</returns>
        public Vector3[] GetPoints(bool refresh = false)
        {
            if (refresh || this.updatePoints)
            {
                var drawingData = this.model.GetDrawingData(this.model.GetLODMinimum());
                if (drawingData.SkinningData != null)
                {
                    this.positionCache = drawingData.GetPoints(
                        this.Manipulator.LocalTransform,
                        this.AnimationController.GetCurrentPose(drawingData.SkinningData),
                        refresh);
                }
                else
                {
                    this.positionCache = drawingData.GetPoints(this.Manipulator.LocalTransform);
                }

                this.updatePoints = false;
            }

            return this.positionCache;
        }
        /// <summary>
        /// Gets triangle list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or triangle list</returns>
        public Triangle[] GetTriangles(bool refresh = false)
        {
            if (refresh || this.updateTriangles)
            {
                var drawingData = this.model.GetDrawingData(this.model.GetLODMinimum());
                if (drawingData.SkinningData != null)
                {
                    this.triangleCache = drawingData.GetTriangles(
                        this.Manipulator.LocalTransform,
                        this.AnimationController.GetCurrentPose(drawingData.SkinningData),
                        refresh);
                }
                else
                {
                    this.triangleCache = drawingData.GetTriangles(this.Manipulator.LocalTransform);
                }

                this.updateTriangles = false;
            }

            return this.triangleCache;
        }
        /// <summary>
        /// Gets bounding sphere
        /// </summary>
        /// <returns>Returns bounding sphere. Empty if the vertex type hasn't position channel</returns>
        public BoundingSphere GetBoundingSphere()
        {
            return this.GetBoundingSphere(false);
        }
        /// <summary>
        /// Gets bounding sphere
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns bounding sphere. Empty if the vertex type hasn't position channel</returns>
        public BoundingSphere GetBoundingSphere(bool refresh)
        {
            if (refresh || this.boundingSphere == new BoundingSphere())
            {
                var points = this.GetPoints(refresh);
                if (points != null && points.Length > 0)
                {
                    this.boundingSphere = BoundingSphere.FromPoints(points);
                }
            }

            return this.boundingSphere;
        }
        /// <summary>
        /// Gets bounding box
        /// </summary>
        /// <returns>Returns bounding box. Empty if the vertex type hasn't position channel</returns>
        public BoundingBox GetBoundingBox()
        {
            return this.GetBoundingBox(false);
        }
        /// <summary>
        /// Gets bounding box
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns bounding box. Empty if the vertex type hasn't position channel</returns>
        public BoundingBox GetBoundingBox(bool refresh)
        {
            if (refresh || this.boundingBox == new BoundingBox())
            {
                var points = this.GetPoints(refresh);
                if (points != null && points.Length > 0)
                {
                    this.boundingBox = BoundingBox.FromPoints(points);
                }
            }

            return this.boundingBox;
        }

        /// <summary>
        /// Gets nearest picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="triangle">Triangle found</param>
        /// <param name="distance">Distance to position</param>
        /// <returns>Returns true if ground position found</returns>
        public virtual bool PickNearest(ref Ray ray, bool facingOnly, out Vector3 position, out Triangle triangle, out float distance)
        {
            position = new Vector3();
            triangle = new Triangle();
            distance = float.MaxValue;

            BoundingSphere bsph = this.GetBoundingSphere();
            if (bsph.Intersects(ref ray))
            {
                Triangle[] triangles = this.GetTriangles();
                if (triangles != null && triangles.Length > 0)
                {
                    Vector3 p;
                    Triangle t;
                    float d;
                    if (Intersection.IntersectNearest(ref ray, triangles, facingOnly, out p, out t, out d))
                    {
                        position = p;
                        triangle = t;
                        distance = d;

                        return true;
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// Gets first picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="triangle">Triangle found</param>
        /// <param name="distance">Distance to position</param>
        /// <returns>Returns true if ground position found</returns>
        public virtual bool PickFirst(ref Ray ray, bool facingOnly, out Vector3 position, out Triangle triangle, out float distance)
        {
            position = new Vector3();
            triangle = new Triangle();
            distance = float.MaxValue;

            BoundingSphere bsph = this.GetBoundingSphere();
            if (bsph.Intersects(ref ray))
            {
                Triangle[] triangles = this.GetTriangles();
                if (triangles != null && triangles.Length > 0)
                {
                    Vector3 p;
                    Triangle t;
                    float d;
                    if (Intersection.IntersectFirst(ref ray, triangles, facingOnly, out p, out t, out d))
                    {
                        position = p;
                        triangle = t;
                        distance = d;

                        return true;
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// Get all picking positions of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="positions">Ground positions if exists</param>
        /// <param name="triangles">Triangles found</param>
        /// <param name="distances">Distances to positions</param>
        /// <returns>Returns true if ground position found</returns>
        public virtual bool PickAll(ref Ray ray, bool facingOnly, out Vector3[] positions, out Triangle[] triangles, out float[] distances)
        {
            positions = null;
            triangles = null;
            distances = null;

            BoundingSphere bsph = this.GetBoundingSphere();
            if (bsph.Intersects(ref ray))
            {
                Triangle[] ts = this.GetTriangles();
                if (ts != null && ts.Length > 0)
                {
                    Vector3[] p;
                    Triangle[] t;
                    float[] d;
                    if (Intersection.IntersectAll(ref ray, ts, facingOnly, out p, out t, out d))
                    {
                        positions = p;
                        triangles = t;
                        distances = d;

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="volume">Culling volume</param>
        /// <param name="distance">If the object is inside the volume, returns the distance</param>
        /// <returns>Returns true if the object is outside of the frustum</returns>
        public virtual bool Cull(ICullingVolume volume, out float distance)
        {
            var cull = true;
            distance = float.MaxValue;

            if (this.hasVolumes)
            {
                if (this.model.SphericVolume)
                {
                    cull = volume.Contains(this.GetBoundingSphere()) == ContainmentType.Disjoint;
                }
                else
                {
                    cull = volume.Contains(this.GetBoundingBox()) == ContainmentType.Disjoint;
                }
            }
            else
            {
                cull = false;
            }

            if (!cull)
            {
                var eyePosition = volume.Position;

                distance = Vector3.DistanceSquared(this.Manipulator.Position, eyePosition);

                this.SetLOD(eyePosition);
            }

            return cull;
        }
        /// <summary>
        /// Set level of detail values
        /// </summary>
        /// <param name="origin">Origin point</param>
        private void SetLOD(Vector3 origin)
        {
            var position = Vector3.TransformCoordinate(this.coarseBoundingSphere.Center, this.Manipulator.LocalTransform);
            var radius = this.coarseBoundingSphere.Radius * this.Manipulator.AveragingScale;
            var bsph = new BoundingSphere(position, radius);

            var dist = Vector3.Distance(position, origin) - radius;
            if (dist < GameEnvironment.LODDistanceHigh)
            {
                this.LevelOfDetail = LevelOfDetailEnum.High;
            }
            else if (dist < GameEnvironment.LODDistanceMedium)
            {
                this.LevelOfDetail = LevelOfDetailEnum.Medium;
            }
            else if (dist < GameEnvironment.LODDistanceLow)
            {
                this.LevelOfDetail = LevelOfDetailEnum.Low;
            }
            else if (dist < GameEnvironment.LODDistanceMinimum)
            {
                this.LevelOfDetail = LevelOfDetailEnum.Minimum;
            }
            else
            {
                this.levelOfDetail = LevelOfDetailEnum.None;
            }
        }

        /// <summary>
        /// Gets internal volume
        /// </summary>
        /// <param name="full"></param>
        /// <returns>Returns internal volume</returns>
        public Triangle[] GetVolume(bool full)
        {
            var drawingData = this.model.GetDrawingData(this.model.GetLODMinimum());
            if (full)
            {
                return this.GetTriangles(true);
            }
            else
            {
                if (drawingData.VolumeMesh != null)
                {
                    return Triangle.Transform(drawingData.VolumeMesh, this.Manipulator.LocalTransform);
                }
                else
                {
                    //Generate cylinder
                    var cylinder = BoundingCylinder.FromPoints(this.GetPoints());
                    return Triangle.ComputeTriangleList(PrimitiveTopology.TriangleList, cylinder, 8);
                }

            }
        }

        /// <summary>
        /// Gets the text representation of the current instance
        /// </summary>
        /// <returns>Returns the text representation of the current instance</returns>
        public override string ToString()
        {
            return string.Format(
                "Id: {0}; LOD: {1}; Active: {2}; Visible: {3}",
                this.Id,
                this.LevelOfDetail,
                this.Active,
                this.Visible);
        }
    }
}
