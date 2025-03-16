using System.Collections.Generic;
using System.Linq;
using BinarySpacePartitioningGodotCSharp.BinarySpacePartitioning;
using Godot;

namespace BinarySpacePartitioningGodotCSharp.Corridors;
public class BentCorridor(
    ICorridor section1,
    ICorridor section2,
    Area corner,
    Area fromArea,
    Area toArea) : ICorridor
{
    public IEnumerable<Area> Sections => section1.Sections.Concat(section2.Sections);
    public Area FromArea => fromArea;
    public Area ToArea => toArea;
   
    public IEnumerable<Vector2I> GetCoords()
    {
        foreach (var coord in section1.GetCoords())
            yield return coord;

        foreach (var coord in section2.GetCoords())
            yield return coord;

        foreach (var coord in corner.GetCoords())
            yield return coord;
    }
}