using System.Collections.Generic;
using System.Linq;
using BinarySpacePartitioningGodotCSharp.BinarySpacePartitioning;
using BinarySpacePartitioningGodotCSharp.Corridors;

namespace BinarySpacePartitioningGodotCSharp.Floors;

public class FloorLayout(Bsp bsp, IEnumerable<Area> rooms, IEnumerable<ICorridor> corridors)
{
    public IEnumerable<Area> LeafSections() => bsp.Leaves().Select(node => node.Area);
    public IEnumerable<Area> Rooms { get; } = rooms;
    public IEnumerable<ICorridor> Corridors { get; } = corridors;
}