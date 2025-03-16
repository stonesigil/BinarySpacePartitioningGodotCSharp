using System.Collections.Generic;
using System.Linq;
using BinarySpacePartitioningGodotCSharp.BinarySpacePartitioning;
using Godot;

namespace BinarySpacePartitioningGodotCSharp.Corridors;

public interface ICorridor
{
    public IEnumerable<Vector2I> GetCoords(); 
    public IEnumerable<Area> Sections { get; }
    public Area FromArea { get; }
    public Area ToArea { get;  }
}