﻿using System.Runtime.InteropServices;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// A span is a range of integers which represents a range of voxels in a <see cref="HeightFieldCell"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct HeightFieldSpan
    {
        /// <summary>
        /// The lowest value in the span.
        /// </summary>
        public int Minimum;
        /// <summary>
        /// The highest value in the span.
        /// </summary>
        public int Maximum;
        /// <summary>
        /// The span area id
        /// </summary>
        public Area Area;
        /// <summary>
        /// Gets the height of the span.
        /// </summary>
        public int Height
        {
            get
            {
                return this.Maximum - this.Minimum;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HeightFieldSpan"/> struct.
        /// </summary>
        /// <param name="min">The lowest value in the span.</param>
        /// <param name="max">The highest value in the span.</param>
        /// <param name="area">The area flags for the span.</param>
        public HeightFieldSpan(int min, int max, Area area)
        {
            this.Minimum = min;
            this.Maximum = max;
            this.Area = area;
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns a string represening the instance</returns>
        public override string ToString()
        {
            return string.Format("Minimum: {0}; Maximum: {1}; Height: {3}; Area: {2}", this.Minimum, this.Maximum, this.Area, this.Height);
        }
    }
}
