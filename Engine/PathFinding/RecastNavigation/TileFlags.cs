﻿
namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Tile flags used for various functions and fields.
    /// </summary>
    public enum TileFlags
    {
        /// <summary>
        /// The navigation mesh owns the tile memory and is responsible for freeing it.
        /// </summary>
        DT_TILE_FREE_DATA = 0x01,
    }
}
