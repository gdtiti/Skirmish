﻿using Engine;
using SharpDX;
using System;

namespace SceneTest
{
    public class SceneWater : Scene
    {
        private const float fogStart = 150f;
        private const float fogRange = 200f;

        private SceneObject<Water> water = null;
        private SceneObject<SkyScattering> sky = null;

        public SceneWater(Game game)
            : base(game)
        {
            Vector3 L = Vector3.Normalize(new Vector3(-1, -1, 0));
            Vector3 V = Vector3.Normalize(new Vector3(+1, -1, 0));
            Vector3 N = Vector3.Normalize(new Vector3(0, +1, 0));

            float d1 = Vector3.Dot(N, Vector3.Normalize(L + V));
            float d2 = Vector3.Dot(Vector3.Reflect(V, N), L);
        }

        public override void Initialize()
        {
            base.Initialize();

#if DEBUG
            this.Game.VisibleMouse = false;
            this.Game.LockMouse = false;
#else
            this.Game.VisibleMouse = false;
            this.Game.LockMouse = true;
#endif

            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 500;
            this.Camera.Goto(80, 10, 100f);
            this.Camera.LookTo(0, 0, 0);

            this.Lights.BaseFogColor = Color.White;

            this.water = this.AddComponent<Water>(new WaterDescription()
            {
                Name = "Water",
                PlaneSize = 1000f,
                HeightmapIterations = 8,
                GeometryIterations = 4,
                ColorIterations = 8,
            });
            this.sky = this.AddComponent<SkyScattering>(new SkyScatteringDescription()
            {
                Name = "Sky",
                Resolution = SkyScatteringResolutionEnum.High
            });

            this.TimeOfDay.BeginAnimation(new TimeSpan(5, 00, 00), 10f);
        }

        public override void Update(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.SetScene<SceneStart>();
            }

            if (this.Game.Input.KeyJustReleased(Keys.F))
            {
                this.ToggleFog();
            }

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);
            bool rightBtn = this.Game.Input.RightMouseButtonPressed;

            this.UpdateCamera(gameTime, shift, rightBtn);

            base.Update(gameTime);
        }

        private void UpdateCamera(GameTime gameTime, bool shift, bool rightBtn)
        {
#if DEBUG
            if (rightBtn)
#endif
            {
                this.Camera.RotateMouse(
                    gameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
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
        }

        private void ToggleFog()
        {
            this.Lights.FogStart = this.Lights.FogStart == 0f ? fogStart : 0f;
            this.Lights.FogRange = this.Lights.FogRange == 0f ? fogRange : 0f;
        }
    }
}
