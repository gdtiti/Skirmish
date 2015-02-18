﻿using System;
using System.Collections.Generic;
using Engine;
using Engine.Common;
using SharpDX;

namespace Terrain
{
    public class TestScene3D : Scene3D
    {
        private Random rnd = new Random();

        private bool follow = false;

        private TextDrawer title = null;
        private TextDrawer load = null;
        private TextDrawer help = null;

        private Engine.Terrain terrain = null;
        private List<Line> oks = new List<Line>();
        private List<Line> errs = new List<Line>();
        private LineListDrawer terrainLineDrawer = null;
        private LineListDrawer terrainGridDrawer = null;

        private Model helicopter = null;
        private float v = 10f;
        private Curve curve = null;
        private float curveTime = 0;
        private LineListDrawer helicopterLineDrawer = null;
        private Vector3 heightOffset = (Vector3.Up * 5f);

        private Color4 gridColor = Color.LightSeaGreen;
        private Color4 curvesColor = Color.Red;
        private Color4 pointsColor = Color.Blue;
        private Color4 segmentsColor = Color.Cyan;
        private Color4 hAxisColor = Color.YellowGreen;
        private Color4 wAxisColor = Color.White;
        private Color4 velocityColor = Color.Green;
        private LineListDrawer curveLineDrawer = null;

        public TestScene3D(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.Camera.NearPlaneDistance = 0.5f;
            this.Camera.FarPlaneDistance = 5000f;

            this.segmentsColor.Alpha = 0.8f;
            this.gridColor.Alpha = 0.5f;

            #region Text

            this.title = this.AddText("Tahoma", 18, Color.White);
            this.load = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.help = this.AddText("Lucida Casual", 12, Color.Yellow);

            this.title.Text = "Terrain collision and trajectories test";
            this.load.Text = "";
            this.help.Text = "";

            this.title.Position = Vector2.Zero;
            this.load.Position = new Vector2(0, 24);
            this.help.Position = new Vector2(0, 48);

            #endregion

            #region Models

            string loadingText = null;

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            sw.Start();
            this.terrain = this.AddTerrain("terrain.dae", new TerrainDescription() { UsePathFinding = true, PathNodeSize = 2f, });
            sw.Stop();
            loadingText += string.Format("terrain: {0} ", sw.Elapsed.TotalSeconds);

            sw.Restart();
            this.helicopter = this.AddModel("helicopter.dae");
            this.helicopter.TextureIndex = 1;
            sw.Stop();
            loadingText += string.Format("helicopter: {0} ", sw.Elapsed.TotalSeconds);

            this.load.Text = loadingText;

            #endregion

            #region Path finding Grid

            List<Line> squares = new List<Line>();

            for (int i = 0; i < this.terrain.grid.Nodes.Length; i++)
            {
                squares.AddRange(GeometryUtil.CreateWiredSquare(this.terrain.grid.Nodes[i].GetCorners()));
            }

            this.terrainGridDrawer = this.AddLineListDrawer(Line.Transform(squares.ToArray(), Matrix.Translation(0, 0.1f, 0)), this.gridColor);
            this.terrainGridDrawer.UseZBuffer = true;
            this.terrainGridDrawer.EnableAlphaBlending = true;
            this.terrainGridDrawer.Visible = false;

            #endregion

            #region Ground position test

            BoundingBox bbox = this.terrain.GetBoundingBox();

            float sep = 2f;
            for (float x = bbox.Minimum.X + 1; x < bbox.Maximum.X - 1; x += sep)
            {
                for (float z = bbox.Minimum.Z + 1; z < bbox.Maximum.Z - 1; z += sep)
                {
                    Vector3 pos;
                    if (this.terrain.FindGroundPosition(x, z, out pos))
                    {
                        this.oks.Add(new Line(pos, pos + Vector3.Up));
                    }
                    else
                    {
                        this.errs.Add(new Line(x, 10, z, x, -10, z));
                    }
                }
            }

            this.terrainLineDrawer = this.AddLineListDrawer(oks.Count + errs.Count);
            this.terrainLineDrawer.UseZBuffer = true;
            this.terrainLineDrawer.Visible = false;

            if (this.oks.Count > 0)
            {
                this.terrainLineDrawer.AddLines(Color.Green, this.oks.ToArray());
            }
            if (this.errs.Count > 0)
            {
                this.terrainLineDrawer.AddLines(Color.Red, this.errs.ToArray());
            }

            #endregion

            #region Helicopter

            this.helicopterLineDrawer = this.AddLineListDrawer(1000);
            this.helicopterLineDrawer.Visible = false;

            Vector3 gPos;
            if (this.terrain.FindGroundPosition(new Vector2(10, 10), out gPos))
            {
                this.helicopter.Manipulator.SetPosition(gPos);
            }

            #endregion

            #region Trajectory

            this.curveLineDrawer = this.AddLineListDrawer(20000);
            this.curveLineDrawer.UseZBuffer = false;
            this.curveLineDrawer.Visible = false;
            this.curveLineDrawer.SetLines(this.wAxisColor, GeometryUtil.CreateAxis(Matrix.Identity, 20f));

            #endregion

            this.Camera.Goto(this.helicopter.Manipulator.Position + Vector3.One * 25f);
            this.Camera.LookTo(this.helicopter.Manipulator.Position);

            Vector3 v1 = this.GetRandomPoint(Vector3.Zero);
            Vector3 v2 = this.GetRandomPoint(Vector3.Zero);

            Engine.PathFinding.Path path = this.terrain.FindPath(v1, v2);
            if (path != null)
            {
                this.curve = path.GenerateCurve();

                this.ComputePath(CurveInterpolations.CatmullRom);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Keys.Space))
            {
                this.follow = !this.follow;
            }

            if (this.Game.Input.KeyJustReleased(Keys.Home))
            {
                this.curveTime = 0f;

                BoundingBox bbox = this.terrain.GetBoundingBox();

                this.curve = new Curve();
                this.curve.AddPosition(0, this.helicopter.Manipulator.Position);
                this.curve.AddPosition(50, this.helicopter.Manipulator.Position + (Vector3.Up * 5f) + (this.helicopter.Manipulator.Forward * 10f));
                this.curve.AddPosition(this.GetRandomPoint(this.heightOffset));
                this.curve.AddPosition(this.GetRandomPoint(this.heightOffset));
                this.curve.AddPosition(this.GetRandomPoint(this.heightOffset));
                this.curve.AddPosition(this.GetRandomPoint(this.heightOffset));
                this.curve.AddPosition(this.GetRandomPoint(this.heightOffset));
                this.curve.AddPosition(this.GetRandomPoint(this.heightOffset));

                this.ComputePath(CurveInterpolations.CatmullRom);
            }

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey) || this.Game.Input.KeyPressed(Keys.RShiftKey);

            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.terrainLineDrawer.Visible = !this.terrainLineDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F2))
            {
                this.terrainGridDrawer.Visible = !this.terrainGridDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F3))
            {
                this.curveLineDrawer.Visible = !this.curveLineDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F4))
            {
                this.helicopterLineDrawer.Visible = !this.helicopterLineDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F5))
            {
                this.curveTime = 0f;
            }

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.Camera.MoveLeft(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.Camera.MoveRight(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.Camera.MoveForward(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.Camera.MoveBackward(gameTime, shift);
            }

#if DEBUG
            if (this.Game.Input.RightMouseButtonPressed)
#endif
            {
                this.Camera.RotateMouse(
                    this.Game.GameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }

            if (this.curve != null)
            {
                float time = gameTime.ElapsedSeconds * this.v;

                if (this.Game.Input.KeyPressed(Keys.LShiftKey))
                {
                    time *= 0.01f;
                }

                if (this.curveTime + time <= this.curve.Length - 300f)
                {
                    int segment;
                    float segmentDistance;
                    this.curve.FindSegment(this.curveTime, out segment, out segmentDistance);

                    int segment2;
                    float segmentDistance2;
                    this.curve.FindSegment(this.curveTime + time, out segment2, out segmentDistance2);

                    float segmentDelta = segmentDistance2 - segmentDistance;

                    Vector3 p0 = this.curve.GetPosition(this.curveTime, CurveInterpolations.CatmullRom);
                    Vector3 p1 = this.curve.GetPosition(this.curveTime + time, CurveInterpolations.CatmullRom);

                    Vector3 cfw = this.helicopter.Manipulator.Forward;
                    Vector3 nfw = Vector3.Normalize(p1 - p0);
                    cfw.Y = 0f;
                    nfw.Y = 0f;

                    float pitch = Vector3.DistanceSquared(p0, p1) * 10f;
                    float roll = Helper.Angle(Vector3.Normalize(nfw), Vector3.Normalize(cfw), Vector3.Up) * 20f;

                    pitch = MathUtil.Clamp(pitch, -MathUtil.PiOverFour, MathUtil.PiOverFour);
                    roll = MathUtil.Clamp(roll, -MathUtil.PiOverFour, MathUtil.PiOverFour);

                    roll *= pitch / MathUtil.PiOverFour;

                    Quaternion r =
                        Helper.LookAt(p0, p1) *
                        Quaternion.RotationYawPitchRoll(0, -pitch, roll);

                    r = Quaternion.Slerp(this.helicopter.Manipulator.Rotation, r, 0.33f);

                    this.helicopter.Manipulator.SetPosition(p0);
                    this.helicopter.Manipulator.SetRotation(r);

                    this.curveTime += time;

                    this.curveLineDrawer.SetLines(this.velocityColor, new[] { new Line(p0, p1) });

                    this.help.Text = string.Format(
                        "Pitch {0:+00.00;-00.00}; Roll {1:+00.00;-00.00}; Delta {2:00.0000}; Segment {3}/{4:00.0000}/{5:00.0000}",
                        MathUtil.RadiansToDegrees(pitch),
                        MathUtil.RadiansToDegrees(roll),
                        pitch / MathUtil.PiOverFour,
                        segment,
                        segmentDistance,
                        segmentDelta);
                }
                else
                {
                    this.curve.AddPosition(this.GetRandomPoint(this.heightOffset));

                    this.ComputePath(CurveInterpolations.CatmullRom);
                }
            }

            Matrix m = Matrix.RotationQuaternion(this.helicopter.Manipulator.Rotation) * Matrix.Translation(this.helicopter.Manipulator.Position);
            BoundingSphere sph = this.helicopter.GetBoundingSphere();

            this.curveLineDrawer.SetLines(this.hAxisColor, GeometryUtil.CreateAxis(m, 5f));

            this.helicopterLineDrawer.SetLines(new Color4(Color.White.ToColor3(), 0.20f), GeometryUtil.CreateWiredSphere(sph, 50, 20));

            if (this.follow)
            {
                this.Camera.LookTo(sph.Center);
                this.Camera.Goto(sph.Center + (this.helicopter.Manipulator.Backward * 15f) + (Vector3.UnitY * 5f), CameraTranslations.UseDelta);
            }
        }

        private void ComputePath(CurveInterpolations interpolation)
        {
            List<Vector3> path = new List<Vector3>();

            float pass = this.curve.Length / 500f;

            for (float i = 0; i <= this.curve.Length; i += pass)
            {
                Vector3 pos = this.curve.GetPosition(i, interpolation);

                path.Add(pos);
            }

            this.curveLineDrawer.SetLines(this.curvesColor, GeometryUtil.CreatePath(path.ToArray()));
            this.curveLineDrawer.SetLines(this.pointsColor, GeometryUtil.CreateCrossList(this.curve.Points, 0.5f));
            this.curveLineDrawer.SetLines(this.segmentsColor, GeometryUtil.CreatePath(this.curve.Points));
        }

        private Vector3 GetRandomPoint(Vector3 offset)
        {
            BoundingBox bbox = this.terrain.GetBoundingBox();

            while (true)
            {
                Vector3 v = rnd.NextVector3(bbox.Minimum * 0.9f, bbox.Maximum * 0.9f);

                Vector3 p;
                if (terrain.FindGroundPosition(v.X, v.Z, out p))
                {
                    return p + offset;
                }
            }
        }
    }
}
