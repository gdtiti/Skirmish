﻿using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;

namespace Engine.Common
{
    /// <summary>
    /// Position normal color vertex format
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionNormalColor : IVertexData
    {
        /// <summary>
        /// Defined input colection
        /// </summary>
        /// <returns></returns>
        public static InputElement[] GetInput()
        {
            return new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32_Float, 12, 0, InputClassification.PerVertexData, 0),
                new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 24, 0, InputClassification.PerVertexData, 0),
            };
        }

        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Normal
        /// </summary>
        public Vector3 Normal;
        /// <summary>
        /// Color
        /// </summary>
        public Color4 Color;
        /// <summary>
        /// Vertex type
        /// </summary>
        public VertexTypes VertexType
        {
            get
            {
                return VertexTypes.PositionNormalColor;
            }
        }
        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Stride
        {
            get
            {
                return Marshal.SizeOf(typeof(VertexPositionNormalColor));
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
            else if (channel == VertexDataChannels.Normal) return true;
            else if (channel == VertexDataChannels.Color) return true;
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
            else if (channel == VertexDataChannels.Normal) return (T)Convert.ChangeType(this.Normal, typeof(T));
            else if (channel == VertexDataChannels.Color) return (T)Convert.ChangeType(this.Color, typeof(T));
            else throw new Exception(string.Format("Channel data not found: {0}", channel));
        }

        /// <summary>
        /// Text representation of vertex
        /// </summary>
        /// <returns>Returns the text representation of vertex</returns>
        public override string ToString()
        {
            return string.Format("Position: {0} Normal: {1}; Color: {2}", this.Position, this.Normal, this.Color);
        }
    };
}
