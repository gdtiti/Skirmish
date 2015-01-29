﻿using System;
using Engine;
using Engine.Common;
using SharpDX;

namespace Collada
{
    public class TestScene3D : Scene3D
    {
        private const float fogStartRel = 0.25f;
        private const float fogRangeRel = 0.75f;

        private readonly Vector3 minScaleSize = new Vector3(0.5f);
        private readonly Vector3 maxScaleSize = new Vector3(2f);

        private Cursor cursor = null;
        private TextDrawer title = null;
        private TextDrawer fps = null;
        private Terrain ground = null;
        private ModelInstanced lamps = null;
        private ModelInstanced helicopters = null;
        private ParticleSystem rain = null;

        private int selectedHelicopter = 0;
        private bool chaseCamera = false;

        public TestScene3D(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.cursor = this.AddCursor("target.png", 16, 16);

            this.title = this.AddText("Tahoma", 18, Color.White);
            this.title.Text = "Collada Scene with billboards and animation";
            this.title.Position = Vector2.Zero;

            this.fps = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.fps.Text = null;
            this.fps.Position = new Vector2(0, 24);

            TerrainDescription terrainDescription = new TerrainDescription()
            {
                AddVegetation = true,
                VegetarionTextures = new[] { "tree0.dds", "tree1.dds", "tree2.dds", "tree3.dds", "tree4.png", "tree5.png" },
                Saturation = 0.05f,
                MinSize = Vector2.One * 2f,
                MaxSize = Vector2.One * 4f,
                Seed = 1024,
            };

            this.ground = this.AddTerrain("Ground.dae", Matrix.Scaling(20, 20, 20), terrainDescription);
            this.helicopters = this.AddInstancingModel("Helicopter.dae", 15);
            this.lamps = this.AddInstancingModel("Poly.dae", 2);
            this.rain = this.AddParticleSystem(ParticleSystemDescription.Rain(0.5f, "raindrop.dds"));

            this.InitializeCamera();
            this.InitializeEnvironment();
            this.InitializeHelicopters();
        }
        private void InitializeCamera()
        {
            this.Camera.NearPlaneDistance = 0.5f;
            this.Camera.FarPlaneDistance = 500;
            this.Camera.Mode = CameraModes.Free;
        }
        private void InitializeEnvironment()
        {
            GameEnvironment.Background = Color.CornflowerBlue;

            this.Lights.FogStart = this.Camera.FarPlaneDistance * fogStartRel;
            this.Lights.FogRange = this.Camera.FarPlaneDistance * fogRangeRel;
            this.Lights.FogColor = Color.CornflowerBlue;

            this.Lights.PointLightEnabled = true;
            this.Lights.PointLight.Ambient = new Color4(0.3f, 0.3f, 0.3f, 1.0f);
            this.Lights.PointLight.Diffuse = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
            this.Lights.PointLight.Specular = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
            this.Lights.PointLight.Attributes = new Vector3(0.1f, 0.0f, 0.0f);
            this.Lights.PointLight.Range = 80.0f;

            this.Lights.SpotLightEnabled = true;
            this.Lights.SpotLight.Direction = Vector3.Down;
            this.Lights.SpotLight.Ambient = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
            this.Lights.SpotLight.Diffuse = new Color4(1.0f, 1.0f, 0.0f, 1.0f);
            this.Lights.SpotLight.Specular = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            this.Lights.SpotLight.Attributes = new Vector3(0.15f, 0.0f, 0.0f);
            this.Lights.SpotLight.Spot = 16f;
            this.Lights.SpotLight.Range = 100.0f;
        }
        private void InitializeHelicopters()
        {
            int rows = 3;
            float left = 15f;
            float back = 25f;
            int x = 0;
            int z = 0;

            this.lamps.Instances[0].Manipulator.SetScale(0.1f);
            this.lamps.Instances[1].Manipulator.SetScale(0.1f);

            Random rnd = new Random();

            for (int i = 0; i < this.helicopters.Count; i++)
            {
                this.helicopters.Instances[i].TextureIndex = rnd.Next(0, 2);

                this.helicopters.Instances[i].Manipulator.LinearVelocity = 10f;
                this.helicopters.Instances[i].Manipulator.AngularVelocity = 45f;

                if (x >= rows) x = 0;
                z = i / rows;

                float posX = (x++ * left);
                float posZ = (z * -back);

                Vector3 p;
                if (this.ground.FindGroundPosition(posX, posZ, out p))
                {
                    this.helicopters.Instances[i].Manipulator.SetScale(1);
                    this.helicopters.Instances[i].Manipulator.SetRotation(Quaternion.Identity);
                    this.helicopters.Instances[i].Manipulator.SetPosition(p + (Vector3.UnitY * 15f));
                }
            }

            this.Camera.Goto(this.helicopters.Instances[this.selectedHelicopter].Manipulator.Position + (Vector3.One * 10f));
            this.Camera.LookTo(this.helicopters.Instances[this.selectedHelicopter].Manipulator.Position + (Vector3.UnitY * 2.5f));
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Keys.Home))
            {
                this.InitializeHelicopters();
            }

            this.UpdateInput(gameTime);
            this.UpdateEnvironment(gameTime);
            this.UpdateHelicopters(gameTime);

            this.fps.Text = this.Game.RuntimeText;
        }
        private void UpdateInput(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Tab))
            {
                this.NextHelicopter();
            }

            if (this.Game.Input.KeyJustReleased(Keys.C))
            {
                this.chaseCamera = !this.chaseCamera;
            }

            if (!this.chaseCamera)
            {
                bool slow = this.Game.Input.KeyPressed(Keys.LShiftKey);

                if (this.Game.Input.KeyPressed(Keys.A))
                {
                    this.Camera.MoveLeft(this.Game.GameTime, slow);
                }

                if (this.Game.Input.KeyPressed(Keys.D))
                {
                    this.Camera.MoveRight(this.Game.GameTime, slow);
                }

                if (this.Game.Input.KeyPressed(Keys.W))
                {
                    this.Camera.MoveForward(this.Game.GameTime, slow);
                }

                if (this.Game.Input.KeyPressed(Keys.S))
                {
                    this.Camera.MoveBackward(this.Game.GameTime, slow);
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
            }
            else
            {
                if (this.Game.Input.KeyPressed(Keys.A))
                {
                    this.helicopters.Instances[this.selectedHelicopter].Manipulator.MoveLeft(gameTime);
                }

                if (this.Game.Input.KeyPressed(Keys.D))
                {
                    this.helicopters.Instances[this.selectedHelicopter].Manipulator.MoveRight(gameTime);
                }

                if (this.Game.Input.KeyPressed(Keys.W))
                {
                    this.helicopters.Instances[this.selectedHelicopter].Manipulator.MoveForward(gameTime);
                }

                if (this.Game.Input.KeyPressed(Keys.S))
                {
                    this.helicopters.Instances[this.selectedHelicopter].Manipulator.MoveBackward(gameTime);
                }

#if DEBUG
                if (this.Game.Input.RightMouseButtonPressed)
#endif
                {
                    if (this.Game.Input.MouseXDelta < 0)
                    {
                        this.helicopters.Instances[this.selectedHelicopter].Manipulator.YawLeft(gameTime);
                    }

                    if (this.Game.Input.MouseXDelta > 0)
                    {
                        this.helicopters.Instances[this.selectedHelicopter].Manipulator.YawRight(gameTime);
                    }

                    if (this.Game.Input.MouseYDelta < 0)
                    {
                        this.helicopters.Instances[this.selectedHelicopter].Manipulator.MoveUp(gameTime);
                    }

                    if (this.Game.Input.MouseYDelta > 0)
                    {
                        this.helicopters.Instances[this.selectedHelicopter].Manipulator.MoveDown(gameTime);
                    }
                }

                Vector3 position = this.helicopters.Instances[this.selectedHelicopter].Manipulator.Position;
                Vector3 interest = (position - (this.helicopters.Instances[this.selectedHelicopter].Manipulator.Forward * 10f));
                position += this.helicopters.Instances[this.selectedHelicopter].Manipulator.Up * 2f;
                position += this.helicopters.Instances[this.selectedHelicopter].Manipulator.Forward * -2f;

                this.Camera.Goto(Vector3.Lerp(this.Camera.Position, position, 0.8f));
                this.Camera.LookTo(Vector3.Lerp(this.Camera.Interest, interest, 0.1f));
            }
        }
        private void UpdateEnvironment(GameTime gameTime)
        {
            #region First lamp

            float r = 500.0f;

            float lampPosX = r * (float)Math.Cos(1f / r * this.Game.GameTime.TotalSeconds);
            float lampPosZ = r * (float)Math.Sin(1f / r * this.Game.GameTime.TotalSeconds);

            Vector3 lampPos;
            if (this.ground.FindGroundPosition(lampPosX, lampPosZ, out lampPos))
            {
                this.lamps.Instances[0].Manipulator.SetPosition(lampPos + (Vector3.UnitY * 30f));
            }

            this.Lights.PointLight.Position = this.lamps.Instances[0].Manipulator.Position;

            #endregion

            if (this.Game.Input.KeyJustReleased(Keys.NumPad0))
            {
                this.Lights.DirectionalLight1Enabled = true;
                this.Lights.DirectionalLight2Enabled = true;
                this.Lights.DirectionalLight3Enabled = true;
                this.Lights.PointLightEnabled = true;
                this.Lights.SpotLightEnabled = true;
            }

            if (this.Game.Input.KeyJustReleased(Keys.NumPad1))
            {
                this.Lights.DirectionalLight1Enabled = !this.Lights.DirectionalLight1Enabled;
            }

            if (this.Game.Input.KeyJustReleased(Keys.NumPad2))
            {
                this.Lights.DirectionalLight2Enabled = !this.Lights.DirectionalLight2Enabled;
            }

            if (this.Game.Input.KeyJustReleased(Keys.NumPad3))
            {
                this.Lights.DirectionalLight3Enabled = !this.Lights.DirectionalLight3Enabled;
            }

            if (this.Game.Input.KeyJustReleased(Keys.NumPad4))
            {
                this.Lights.PointLightEnabled = !this.Lights.PointLightEnabled;
            }

            if (this.Game.Input.KeyJustReleased(Keys.NumPad5))
            {
                this.Lights.SpotLightEnabled = !this.Lights.SpotLightEnabled;
            }

            if (this.Game.Input.KeyJustReleased(Keys.NumPad6))
            {
                if (this.Lights.FogRange == 0)
                {
                    this.Lights.FogStart = this.Camera.FarPlaneDistance * fogStartRel;
                    this.Lights.FogRange = this.Camera.FarPlaneDistance * fogRangeRel;
                }
                else
                {
                    this.Lights.FogStart = 0;
                    this.Lights.FogRange = 0;
                }
            }
        }
        private void UpdateHelicopters(GameTime gameTime)
        {
            #region Second lamp

            Vector3 pos = (this.helicopters.Instances[this.selectedHelicopter].Manipulator.Backward * 3f);
            Quaternion rot = Quaternion.RotationAxis(this.helicopters.Instances[this.selectedHelicopter].Manipulator.Left, MathUtil.DegreesToRadians(45f));

            this.lamps.Instances[1].Manipulator.SetPosition(pos + this.helicopters.Instances[this.selectedHelicopter].Manipulator.Position);
            this.lamps.Instances[1].Manipulator.SetRotation(rot * this.helicopters.Instances[this.selectedHelicopter].Manipulator.Rotation);

            this.Lights.SpotLight.Position = this.lamps.Instances[1].Manipulator.Position;
            this.Lights.SpotLight.Direction = this.lamps.Instances[1].Manipulator.Down;

            #endregion
        }
        private void NextHelicopter()
        {
            this.selectedHelicopter++;
            if (this.selectedHelicopter >= this.helicopters.Count)
            {
                this.selectedHelicopter = 0;
            }
        }
        private void PrevHelicopter()
        {
            this.selectedHelicopter--;
            if (this.selectedHelicopter < 0)
            {
                this.selectedHelicopter = this.helicopters.Count - 1;
            }
        }
    }
}
