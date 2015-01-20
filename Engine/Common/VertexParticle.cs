﻿using System;
using System.Runtime.InteropServices;
using SharpDX;
using InputClassification = SharpDX.Direct3D11.InputClassification;
using InputElement = SharpDX.Direct3D11.InputElement;

namespace Engine.Common
{
    /// <summary>
    /// Particle data buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexParticle : IVertexData
    {
        /// <summary>
        /// Defined input colection
        /// </summary>
        public static InputElement[] GetInput()
        {
            return new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("VELOCITY", 0, SharpDX.DXGI.Format.R32G32B32_Float, 12, 0, InputClassification.PerVertexData, 0),
                new InputElement("SIZE", 0, SharpDX.DXGI.Format.R32G32_Float, 24, 0, InputClassification.PerVertexData, 0),
                new InputElement("AGE", 0, SharpDX.DXGI.Format.R32_Float, 32, 0, InputClassification.PerVertexData, 0),
                new InputElement("TYPE", 0, SharpDX.DXGI.Format.R32_UInt, 36, 0, InputClassification.PerVertexData, 0),
            };
        }

        /// <summary>
        /// Initial position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Initial velocity
        /// </summary>
        public Vector3 Velocity;
        /// <summary>
        /// Size
        /// </summary>
        public Vector2 Size;
        /// <summary>
        /// Particle age
        /// </summary>
        public float Age;
        /// <summary>
        /// Particle type
        /// </summary>
        public uint Type;
        /// <summary>
        /// Vertex type
        /// </summary>
        public VertexTypes VertexType
        {
            get
            {
                return VertexTypes.Particle;
            }
        }
        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Stride
        {
            get
            {
                return Marshal.SizeOf(typeof(VertexParticle));
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
            else if (channel == VertexDataChannels.Size) return true;
            else return false;
        }
        /// <summary>
        /// Gets data channel value
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="channel">Data channel</param>
        /// <returns>Returns data for the specified channel</returns>
        public T GetChannelValue<T>(VertexDataChannels channel) where T : struct
        {
            if (channel == VertexDataChannels.Position) return (T)Convert.ChangeType(this.Position, typeof(T));
            else if (channel == VertexDataChannels.Size) return (T)Convert.ChangeType(this.Size, typeof(T));
            else throw new Exception(string.Format("Channel data not found: {0}", channel));
        }

        /// <summary>
        /// Text representation of vertex
        /// </summary>
        /// <returns>Returns the text representation of vertex</returns>
        public override string ToString()
        {
            return string.Format("Position: {0}; Size: {1}", this.Position, this.Size);
        }
    }
}