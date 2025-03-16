using System.Collections.Generic;
using BinarySpacePartitioningGodotCSharp.BinarySpacePartitioning;

namespace BinarySpacePartitioningGodotCSharp.Corridors;

public class StraightCorridor(Area section, Area fromArea, Area toArea) : Area(section.Coord, section.Size), ICorridor
{
    public IEnumerable<Area> Sections => [section];
    public Area FromArea => fromArea;
    public Area ToArea => toArea;
}