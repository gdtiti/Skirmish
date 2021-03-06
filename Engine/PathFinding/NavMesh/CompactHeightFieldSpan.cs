﻿using System;
using System.Runtime.InteropServices;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// Represents a voxel span in a <see cref="CompactHeightField"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct CompactHeightFieldSpan
    {
        /// <summary>
        /// A constant that means there is no connection for the values <see cref="ConnectionWest"/>,
        /// <see cref="ConnectionNorth"/>, <see cref="ConnectionEast"/>, and <see cref="ConnectionSouth"/>.
        /// </summary>
        private const byte NotConnected = 0xff;

        /// <summary>
        /// If two CompactSpans overlap, find the minimum of the new overlapping CompactSpans.
        /// </summary>
        /// <param name="left">The first CompactSpan</param>
        /// <param name="right">The second CompactSpan</param>
        /// <param name="min">The minimum of the overlapping ComapctSpans</param>
        public static void OverlapMin(ref CompactHeightFieldSpan left, ref CompactHeightFieldSpan right, out int min)
        {
            min = Math.Max(left.Minimum, right.Minimum);
        }
        /// <summary>
        /// If two CompactSpans overlap, find the maximum of the new overlapping CompactSpans.
        /// </summary>
        /// <param name="left">The first CompactSpan</param>
        /// <param name="right">The second CompactSpan</param>
        /// <param name="max">The maximum of the overlapping CompactSpans</param>
        public static void OverlapMax(ref CompactHeightFieldSpan left, ref CompactHeightFieldSpan right, out int max)
        {
            if (left.Height == int.MaxValue)
            {
                if (right.Height == int.MaxValue)
                    max = int.MaxValue;
                else
                    max = right.Minimum + right.Height;
            }
            else if (right.Height == int.MaxValue)
                max = left.Minimum + left.Height;
            else
                max = Math.Min(left.Minimum + left.Height, right.Minimum + right.Height);
        }
        /// <summary>
        /// Creates a <see cref="CompactHeightFieldSpan"/> from a minimum boundary and a maximum boundary.
        /// </summary>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns>A <see cref="CompactHeightFieldSpan"/>.</returns>
        public static CompactHeightFieldSpan FromMinMax(int min, int max)
        {
            CompactHeightFieldSpan s;
            FromMinMax(min, max, out s);
            return s;
        }
        /// <summary>
        /// Creates a <see cref="CompactHeightFieldSpan"/> from a minimum boundary and a maximum boundary.
        /// </summary>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="span">A <see cref="CompactHeightFieldSpan"/>.</param>
        public static void FromMinMax(int min, int max, out CompactHeightFieldSpan span)
        {
            span.Minimum = min;
            span.Height = max - min;
            span.ConnectionWest = NotConnected;
            span.ConnectionNorth = NotConnected;
            span.ConnectionEast = NotConnected;
            span.ConnectionSouth = NotConnected;
            span.Region = RegionId.Null;
        }
        /// <summary>
        /// Sets connection data to a span contained in a neighboring cell.
        /// </summary>
        /// <param name="dir">The direction of the cell.</param>
        /// <param name="i">The index of the span in the neighboring cell.</param>
        /// <param name="s">The <see cref="CompactHeightFieldSpan"/> to set the data for.</param>
        public static void SetConnection(Direction dir, int i, ref CompactHeightFieldSpan s)
        {
            if (i >= NotConnected)
                throw new ArgumentOutOfRangeException("Index of connecting span is too high to be stored. Try increasing cell height.", "i");

            switch (dir)
            {
                case Direction.West:
                    s.ConnectionWest = (byte)i;
                    break;
                case Direction.North:
                    s.ConnectionNorth = (byte)i;
                    break;
                case Direction.East:
                    s.ConnectionEast = (byte)i;
                    break;
                case Direction.South:
                    s.ConnectionSouth = (byte)i;
                    break;
                default:
                    throw new ArgumentException("dir isn't a valid Direction.");
            }
        }
        /// <summary>
        /// Un-sets connection data from a neighboring cell.
        /// </summary>
        /// <param name="dir">The direction of the cell.</param>
        /// <param name="s">The <see cref="CompactHeightFieldSpan"/> to set the data for.</param>
        public static void UnsetConnection(Direction dir, ref CompactHeightFieldSpan s)
        {
            switch (dir)
            {
                case Direction.West:
                    s.ConnectionWest = NotConnected;
                    break;
                case Direction.North:
                    s.ConnectionNorth = NotConnected;
                    break;
                case Direction.East:
                    s.ConnectionEast = NotConnected;
                    break;
                case Direction.South:
                    s.ConnectionSouth = NotConnected;
                    break;
                default:
                    throw new ArgumentException("dir isn't a valid Direction.");
            }
        }
        /// <summary>
        /// Gets the connection data for a neighboring cell in a specified direction.
        /// </summary>
        /// <param name="s">The <see cref="CompactHeightFieldSpan"/> to get the connection data from.</param>
        /// <param name="dir">The direction.</param>
        /// <returns>The index of the span in the neighboring cell.</returns>
        public static int GetConnection(ref CompactHeightFieldSpan s, Direction dir)
        {
            switch (dir)
            {
                case Direction.West:
                    return s.ConnectionWest;
                case Direction.North:
                    return s.ConnectionNorth;
                case Direction.East:
                    return s.ConnectionEast;
                case Direction.South:
                    return s.ConnectionSouth;
                default:
                    throw new ArgumentException("dir isn't a valid Direction.");
            }
        }

        /// <summary>
        /// The number of voxels contained in the span.
        /// </summary>
        public int Height;
        /// <summary>
        /// The span minimum.
        /// </summary>
        public int Minimum;
        /// <summary>
        /// Gets the upper bound of the span.
        /// </summary>
        public int Maximum
        {
            get
            {
                return this.Minimum + Height;
            }
        }
        /// <summary>
        /// A byte representing the index of the connected span in the cell directly to the west.
        /// </summary>
        public byte ConnectionWest;
        /// <summary>
        /// A byte representing the index of the connected span in the cell directly to the north.
        /// </summary>
        public byte ConnectionNorth;
        /// <summary>
        /// A byte representing the index of the connected span in the cell directly to the east.
        /// </summary>
        public byte ConnectionEast;
        /// <summary>
        /// A byte representing the index of the connected span in the cell directly to the south.
        /// </summary>
        public byte ConnectionSouth;
        /// <summary>
        /// The region the span belongs to.
        /// </summary>
        public RegionId Region;
        /// <summary>
        /// Gets a value indicating whether the span has an upper bound or goes to "infinity".
        /// </summary>
        public bool HasUpperBound
        {
            get
            {
                return Height != int.MaxValue;
            }
        }
        /// <summary>
        /// Gets the number of connections the current CompactSpan has with its neighbors.
        /// </summary>
        public int ConnectionCount
        {
            get
            {
                int count = 0;
                if (ConnectionWest != NotConnected)
                    count++;
                if (ConnectionNorth != NotConnected)
                    count++;
                if (ConnectionEast != NotConnected)
                    count++;
                if (ConnectionSouth != NotConnected)
                    count++;

                return count;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompactHeightFieldSpan"/> struct.
        /// </summary>
        /// <param name="minimum">The span minimum.</param>
        /// <param name="height">The number of voxels the span contains.</param>
        public CompactHeightFieldSpan(int minimum, int height)
        {
            this.Minimum = minimum;
            this.Height = height;
            this.ConnectionWest = NotConnected;
            this.ConnectionNorth = NotConnected;
            this.ConnectionEast = NotConnected;
            this.ConnectionSouth = NotConnected;
            this.Region = RegionId.Null;
        }

        /// <summary>
        /// Gets the connection data for a neighboring call in a specified direction.
        /// </summary>
        /// <param name="dir">The direction.</param>
        /// <returns>The index of the span in the neighboring cell.</returns>
        public int GetConnection(Direction dir)
        {
            return GetConnection(ref this, dir);
        }
        /// <summary>
        /// Gets a value indicating whether the span is connected to another span in a specified direction.
        /// </summary>
        /// <param name="dir">The direction.</param>
        /// <returns>A value indicating whether the specified direction has a connected span.</returns>
        public bool IsConnected(Direction dir)
        {
            switch (dir)
            {
                case Direction.West:
                    return ConnectionWest != NotConnected;
                case Direction.North:
                    return ConnectionNorth != NotConnected;
                case Direction.East:
                    return ConnectionEast != NotConnected;
                case Direction.South:
                    return ConnectionSouth != NotConnected;
                default:
                    throw new ArgumentException("dir isn't a valid Direction.");
            }
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns a string represening the instance</returns>
        public override string ToString()
        {
            return string.Format("Minimum: {0}; Maximum: {1}; Height: {3}; Region: {2}; Connections: {4}", this.Minimum, this.Maximum, this.Region, this.Height, this.ConnectionCount);
        }
    }
}
