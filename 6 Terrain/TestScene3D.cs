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
        private TextDrawer help = null;

        private Engine.Terrain terrain = null;
        private List<Line> oks = new List<Line>();
        private List<Line> errs = new List<Line>();
        private LineListDrawer terrainLineDrawer = null;

        private Model helicopter = null;
        private Vector3 h = Vector3.UnitY * 5f;
        private float v = 10f;
        private Curve curve = null;
        private float curveTime = 0;
        private LineListDrawer helicopterLineDrawer = null;

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

            #region Text

            this.title = this.AddText("Tahoma", 18, Color.White);
            this.help = this.AddText("Lucida Casual", 12, Color.Yellow);

            this.title.Text = "Terrain collision and trajectories test";
            this.help.Text = "";

            this.title.Position = Vector2.Zero;
            this.help.Position = new Vector2(0, 24);

            #endregion

            #region Models

            this.terrain = this.AddTerrain("terrain.dae", new TerrainDescription() { });
            this.helicopter = this.AddModel("helicopter.dae");
            this.helicopter.TextureIndex = 1;

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

            #endregion

            #region Trajectory

            this.curveLineDrawer = this.AddLineListDrawer(20000);
            this.curveLineDrawer.UseZBuffer = false;
            this.curveLineDrawer.Visible = false;
            this.curveLineDrawer.SetLines(this.wAxisColor, GeometryUtil.CreateAxis(Matrix.Identity, 20f));

            #endregion

            this.GeneratePath(Vector3.Zero, CurveInterpolations.CatmullRom);
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

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey) || this.Game.Input.KeyPressed(Keys.RShiftKey);

            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.terrainLineDrawer.Visible = !this.terrainLineDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F2))
            {
                this.curveLineDrawer.Visible = !this.curveLineDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F3))
            {
                this.helicopterLineDrawer.Visible = !this.helicopterLineDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F5))
            {
                this.curveTime = 0f;
            }

            if (this.Game.Input.KeyJustReleased(Keys.G))
            {
                this.GeneratePath(this.helicopter.Manipulator.Position, shift ? CurveInterpolations.Linear : CurveInterpolations.CatmullRom);
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

            float time = gameTime.ElapsedSeconds * this.v;

            if (this.Game.Input.KeyPressed(Keys.LShiftKey))
            {
                time *= 0.01f;
            }

            if (this.curveTime + time <= this.curve.Length)
            {
                Vector3 p0 = this.curve.GetPosition(this.curveTime, CurveInterpolations.CatmullRom);
                Vector3 p1 = this.curve.GetPosition(this.curveTime + time, CurveInterpolations.CatmullRom);

                Vector3 p0t;
                Vector3 p1t;
                if (this.terrain.FindGroundPosition(p0.X, p0.Z, out p0t)) { p0 = p0t; }
                if (this.terrain.FindGroundPosition(p1.X, p1.Z, out p1t)) { p1 = p1t; }
                p0 += h;
                p1 += h;

                Vector3 cfw = this.helicopter.Manipulator.Forward;
                Vector3 nfw = Vector3.Normalize(p1 - p0);
                cfw.Y = 0f;
                nfw.Y = 0f;

                //Calc helicopter pitch and roll amount
                float a = Helper.Angle2(Vector3.Normalize(nfw), Vector3.Normalize(cfw), Vector3.Up) * 5f;
                float d = MathUtil.Clamp(Vector3.DistanceSquared(p0, p1) * 10f, 0f, MathUtil.PiOverTwo);

                this.help.Text = string.Format("Pitch {0}; Roll {1};", MathUtil.RadiansToDegrees(d).ToString(), MathUtil.RadiansToDegrees(a).ToString());

                this.helicopter.Manipulator.SetPosition(p0);
                this.helicopter.Manipulator.LookAt(p1, 0.05f);
                if (!float.IsNaN(d)) this.helicopter.Manipulator.PitchDown(gameTime, d);
                if (!float.IsNaN(a)) this.helicopter.Manipulator.RollRight(gameTime, a);

                this.curveTime += time;

                Matrix m = Matrix.RotationQuaternion(this.helicopter.Manipulator.Rotation) * Matrix.Translation(this.helicopter.Manipulator.Position);

                this.curveLineDrawer.SetLines(this.hAxisColor, GeometryUtil.CreateAxis(m, 5f));
                this.curveLineDrawer.SetLines(this.velocityColor, new[] { new Line(p0, p1) });
            }
            else
            {
                this.GeneratePath(this.helicopter.Manipulator.Position, CurveInterpolations.CatmullRom);
            }

            BoundingSphere sph = this.helicopter.GetBoundingSphere();

            this.helicopterLineDrawer.SetLines(new Color4(Color.White.ToColor3(), 0.20f), GeometryUtil.CreateWiredSphere(sph, 50, 20));

            if (this.follow)
            {
                this.Camera.LookTo(sph.Center);
                this.Camera.Goto(sph.Center + (this.helicopter.Manipulator.Backward * 15f) + (Vector3.UnitY * 5f), CameraTranslations.UseDelta);
            }
        }

        private void GeneratePath(Vector3 position, CurveInterpolations interpolation)
        {
            BoundingBox bbox = this.terrain.GetBoundingBox();

            List<Vector3> points = new List<Vector3>();

            points.Add(position);

            for (int i = 0; i < 10; i++)
            {
                Vector3 v = rnd.NextVector3(bbox.Minimum * 0.9f, bbox.Maximum * 0.9f);

                Vector3 pos;
                if (this.terrain.FindGroundPosition(v.X, v.Z, out pos))
                {
                    points.Add(pos + h);
                }
            }

            this.curve = new Curve(points.ToArray());
            this.curveTime = 0;

            List<Vector3> path = new List<Vector3>();

            float pass = this.curve.Length / 500f;

            for (float i = 0; i <= this.curve.Length; i += pass)
            {
                Vector3 v = this.curve.GetPosition(i, interpolation);

                Vector3 pos;
                if (this.terrain.FindGroundPosition(v.X, v.Z, out pos))
                {
                    path.Add(pos + h);
                }
            }

            this.curveLineDrawer.SetLines(this.curvesColor, GeometryUtil.CreatePath(path.ToArray()));
            this.curveLineDrawer.SetLines(this.pointsColor, GeometryUtil.CreateCrossList(this.curve.Points, 0.5f));
            this.curveLineDrawer.SetLines(this.segmentsColor, GeometryUtil.CreatePath(this.curve.Points));
        }
    }
}
