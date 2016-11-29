﻿using SharpDX;
using System;
using System.Runtime.InteropServices;
using InputClassification = SharpDX.Direct3D11.InputClassification;
using InputElement = SharpDX.Direct3D11.InputElement;

namespace Engine.Common
{
    /// <summary>
    /// Particle data buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexGPUParticle : IVertexData
    {
        /// <summary>
        /// Defined input colection
        /// </summary>
        public static InputElement[] GetInput()
        {
            return new InputElement[]
            {
                new InputElement("TYPE", 0, SharpDX.DXGI.Format.R32_UInt, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("EMISSION_TIME", 0, SharpDX.DXGI.Format.R32_Float, 4, 0, InputClassification.PerVertexData, 0),
                new InputElement("EMISSION_RATE", 0, SharpDX.DXGI.Format.R32_Float, 8, 0, InputClassification.PerVertexData, 0),
                new InputElement("ENERGY", 0, SharpDX.DXGI.Format.R32_Float, 12, 0, InputClassification.PerVertexData, 0),
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 16, 0, InputClassification.PerVertexData, 0),
                new InputElement("VELOCITY", 0, SharpDX.DXGI.Format.R32G32B32_Float, 28, 0, InputClassification.PerVertexData, 0),
                new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 40, 0, InputClassification.PerVertexData, 0),
            };
        }

        /// <summary>
        /// Particle type
        /// </summary>
        public uint Type;
        /// <summary>
        /// Total emission time
        /// </summary>
        public float EmissionTime;
        /// <summary>
        /// Emission rate
        /// </summary>
        public float EmissionRate;
        /// <summary>
        /// Particle energy
        /// </summary>
        public float Energy;
        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Velocity
        /// </summary>
        public Vector3 Velocity;
        /// <summary>
        /// Particle color
        /// </summary>
        public Color4 Color;
        /// <summary>
        /// Vertex type
        /// </summary>
        public VertexTypes VertexType
        {
            get
            {
                return VertexTypes.GPUParticle;
            }
        }
        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Stride
        {
            get
            {
                return Marshal.SizeOf(typeof(VertexGPUParticle));
            }
        }

        /// <summary>
        /// Gets if structure contains data for the specified channel
        /// </summary>
        /// <param name="channel">Data channel</param>
        /// <returns>Returns true if structure contains data for the specified channel</returns>
        public bool HasChannel(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return true;
            if (channel == VertexDataChannels.Color) return true;
            else if (channel == VertexDataChannels.Size) return true;
            else return false;
        }
        /// <summary>
        /// Gets data channel value
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="channel">Data channel</param>
        /// <returns>Returns data for the specified channel</returns>
        public T GetChannelValue<T>(VertexDataChannels channel)
        {
            if (channel == VertexDataChannels.Position) return this.Position.Cast<T>();
            else if (channel == VertexDataChannels.Color) return this.Color.Cast<T>();
            else throw new Exception(string.Format("Channel data not found: {0}", channel));
        }
        /// <summary>
        /// Sets the channer value
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="channel">Channel</param>
        /// <param name="value">Value</param>
        public void SetChannelValue<T>(VertexDataChannels channel, T value)
        {
            if (channel == VertexDataChannels.Position) this.Position = value.Cast<Vector3>();
            else if (channel == VertexDataChannels.Color) this.Color = value.Cast<Color4>();
            else throw new Exception(string.Format("Channel data not found: {0}", channel));
        }

        /// <summary>
        /// Text representation of vertex
        /// </summary>
        /// <returns>Returns the text representation of vertex</returns>
        public override string ToString()
        {
            return string.Format("Position: {0}; Color: {1}", this.Position, this.Color);
        }
    }
}
