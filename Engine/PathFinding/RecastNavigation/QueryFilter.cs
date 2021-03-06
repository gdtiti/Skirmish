﻿using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Defines polygon filtering and traversal costs for navigation mesh query operations.
    /// </summary>
    public class QueryFilter
    {
        /// <summary>
        /// Cost per area type. (Used by default implementation.)
        /// </summary>
        public float[] m_areaCost;
        /// <summary>
        /// Flags for polygons that can be visited. (Used by default implementation.)
        /// </summary>
        public SamplePolyFlags m_includeFlags;
        /// <summary>
        /// Flags for polygons that should not be visted. (Used by default implementation.)
        /// </summary>
        public SamplePolyFlags m_excludeFlags;

        /// <summary>
        /// Constructor
        /// </summary>
        public QueryFilter()
        {
            m_areaCost = new float[Constants.DT_MAX_AREAS];
        }

        /// <summary>
        /// Returns true if the polygon can be visited.  (I.e. Is traversable.)
        /// </summary>
        /// <param name="r">The reference id of the polygon test.</param>
        /// <param name="tile">The tile containing the polygon.</param>
        /// <param name="poly">The polygon to test.</param>
        /// <returns></returns>
        public virtual bool PassFilter(int r, MeshTile tile, Poly poly)
        {
            return (poly.flags & m_includeFlags) != 0 && (poly.flags & m_excludeFlags) == 0;
        }
        /// <summary>
        /// Returns cost to move from the beginning to the end of a line segment that is fully contained within a polygon.
        /// </summary>
        /// <param name="pa">The start position on the edge of the previous and current polygon. [(x, y, z)]</param>
        /// <param name="pb">The end position on the edge of the current and next polygon. [(x, y, z)]</param>
        /// <param name="prevRef">The reference id of the previous polygon. [opt]</param>
        /// <param name="prevTile">The tile containing the previous polygon. [opt]</param>
        /// <param name="prevPoly">The previous polygon. [opt]</param>
        /// <param name="curRef">The reference id of the current polygon.</param>
        /// <param name="curTile">The tile containing the current polygon.</param>
        /// <param name="curPoly">The current polygon.</param>
        /// <param name="nextRef">The refernece id of the next polygon. [opt]</param>
        /// <param name="nextTile">The tile containing the next polygon. [opt]</param>
        /// <param name="nextPoly">The next polygon. [opt]</param>
        /// <returns></returns>
        public virtual float GetCost(
            Vector3 pa, Vector3 pb,
            int prevRef, MeshTile prevTile, Poly prevPoly,
            int curRef, MeshTile curTile, Poly curPoly,
            int nextRef, MeshTile nextTile, Poly nextPoly)
        {
            return Vector3.Distance(pa, pb) * m_areaCost[(int)curPoly.Area];
        }

        /// <summary>
        /// Returns the traversal cost of the area.
        /// </summary>
        /// <param name="i">The id of the area.</param>
        /// <returns>The traversal cost of the area.</returns>
        public float GetAreaCost(int i) { return m_areaCost[i]; }
        /// <summary>
        /// Sets the traversal cost of the area.
        /// </summary>
        /// <param name="i">The id of the area.</param>
        /// <param name="cost">The new cost of traversing the area.</param>
        public void SetAreaCost(int i, float cost) { m_areaCost[i] = cost; }

        /// <summary>
        /// Returns the include flags for the filter.
        /// Any polygons that include one or more of these flags will be included in the operation.
        /// </summary>
        /// <returns></returns>
        public SamplePolyFlags GetIncludeFlags() { return m_includeFlags; }
        /// <summary>
        /// Sets the include flags for the filter.
        /// </summary>
        /// <param name="flags">The new flags.</param>
        public void SetIncludeFlags(SamplePolyFlags flags) { m_includeFlags = flags; }

        /// <summary>
        /// Returns the exclude flags for the filter.
        /// Any polygons that include one ore more of these flags will be excluded from the operation.
        /// </summary>
        /// <returns></returns>
        public SamplePolyFlags GetExcludeFlags() { return m_excludeFlags; }
        /// <summary>
        /// Sets the exclude flags for the filter.
        /// </summary>
        /// <param name="flags">The new flags.</param>
        public void SetExcludeFlags(SamplePolyFlags flags) { m_excludeFlags = flags; }
    }
}
