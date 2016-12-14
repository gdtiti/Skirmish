﻿using SharpDX;

namespace Engine
{
    /// <summary>
    /// Sun controller
    /// </summary>
    public class Sun
    {
        /// <summary>
        /// Game instance
        /// </summary>
        protected Game Game;

        /// <summary>
        /// Time of day
        /// </summary>
        public TimeOfDay TimeOfDayController;
        /// <summary>
        /// Light
        /// </summary>
        public SceneLightDirectional Light;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        public Sun(Game game)
        {
            this.Game = game;

            this.TimeOfDayController = new TimeOfDay();
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public virtual void Update(GameTime gameTime)
        {
            this.TimeOfDayController.Update(gameTime.ElapsedSeconds);

            this.UpdateLight();
        }
        /// <summary>
        /// Updates light state
        /// </summary>
        private void UpdateLight()
        {
            float yaw = this.TimeOfDayController.Azimuth;
            float pitch = this.TimeOfDayController.Elevation;

            Matrix rot = Matrix.RotationYawPitchRoll(yaw, pitch, 0);

            Vector3 lightDirection = Vector3.TransformNormal(Vector3.ForwardLH, rot);

            this.Light.Direction = -lightDirection;
            this.Light.DiffuseColor = this.TimeOfDayController.SunColor;
            this.Light.SpecularColor = this.TimeOfDayController.SunBandColor;
        }
    }
}