﻿
namespace Engine.PathFinding.RecastNavigation
{
    struct RasterizationContext
    {
        public const int MaxLayers = 32;

        public Heightfield solid;
        public TileCacheAreas[] triareas;
        public HeightfieldLayerSet lset;
        public CompactHeightfield chf;
        public TileCacheData[] tiles;
        public int ntiles;
    }
}
