using System.Collections.Generic;
using BinarySpacePartitioningGodotCSharp.BinarySpacePartitioning;
using BinarySpacePartitioningGodotCSharp.Floors;
using Godot;
using Godot.Collections;

namespace BinarySpacePartitioningGodotCSharp.Scenes;


public partial class Main : Node
{
    [Export] private BspFloorLayoutGenerator _bspFloorLayoutGenerator;
    [Export] private TileMapLayer _tileMapLayer;

    public override void _Ready()
    {
        var floorLayout = _bspFloorLayoutGenerator.Generate(depth: 1);
        RenderFloor(floorLayout);
    }

    private void RenderFloor(FloorLayout floorLayout)
    {
        foreach (var section in floorLayout.LeafSections())
            SetCells(section.Shrink(1), TerrainType.BspSection);

        foreach (var room in floorLayout.Rooms)
            SetCells(room, TerrainType.Room);

        foreach (var corridor in floorLayout.Corridors)
            SetCells(corridor.GetCoords(), TerrainType.Corridor);
    }

    private void SetCells(Area area, TerrainType floorTerrain)
    {
        SetCells(area.GetCoords(), floorTerrain);
    }

    private void SetCells(IEnumerable<Vector2I> coords, TerrainType terrain)
    {
        _tileMapLayer.SetCellsTerrainConnect(new Array<Vector2I>(coords), 0, (int)terrain);
    }
    
    public enum TerrainType
    {
        Room,
        Corridor,
        BspSection,
    }
}

