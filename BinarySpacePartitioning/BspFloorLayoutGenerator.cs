using System.Collections.Generic;
using System.Linq;
using BinarySpacePartitioningGodotCSharp.Corridors;
using BinarySpacePartitioningGodotCSharp.Floors;
using BinarySpacePartitioningGodotCSharp.Rooms;
using CSharpFunctionalExtensions;
using Godot;

namespace BinarySpacePartitioningGodotCSharp.BinarySpacePartitioning;

/// <summary>
/// Generates a floor by using binary space partitioning.
/// </summary>
[GlobalClass]
public partial class BspFloorLayoutGenerator: Node
{
    [Export] private CorridorGenerator _corridorGenerator;
    [Export] private RoomGenerator _roomGenerator;
    [Export] private RandomService _randomService;
    [Export] private Config _config;

    private Area _floorArea;
    private Bsp _bsp;
    private Vector2I Size => _config.FloorSize;

    public FloorLayout Generate(int depth)
    {
        _randomService.Reset(depth);
        var padding = new Vector2I(_config.FloorBorderPadding, _config.FloorBorderPadding);
        _floorArea = new Area(new Vector2I(padding.X, padding.Y), Size - padding - new Vector2I(1, 1));
        _bsp = BuildBsp(_floorArea, _config.MinPartitionSize, _config.SplitNoise);

        _bsp.SampleRooms(_roomGenerator);
        var rooms = _bsp.Rooms().ToArray();
        var overlapCounter = _bsp.CreateRoomOverlapCounter();
        _bsp.SampleCorridors(_corridorGenerator, overlapCounter);
        var corridors = _bsp.Corridors().ToList();
        var floorGraph = new FloorGraph(corridors, rooms);

        for (var i = 0; i < _config.NumLoops; i++)
        {
            var extraCorridor = floorGraph.CorridorToConnectWorstDeadEnd(
                _corridorGenerator,
                _config.LoopCorridorGraphDistanceWeight,
                _config.LoopCorridorManhattanDistanceWeight,
                overlapCounter);
            corridors.Add(extraCorridor);
            floorGraph.AddCorridor(extraCorridor);
        }
        
        return new FloorLayout(_bsp, rooms, corridors);
    }

    /// <summary>
    /// Builds a partition tree over the area covering <paramref name="floorArea"/> (which may be a subset of the entire floor)
    /// and uses it to sample rooms.
    /// The partition size is limited by <paramref name="minPartitionSize"/>.
    /// The <paramref name="splitNoise"/> parameter controls how much the split point can vary.
    /// Increase this to enable larger room samples.
    /// </summary>
    /// <param name="floorArea"></param>
    /// <param name="minPartitionSize"></param>
    /// <param name="splitNoise"></param>
    /// <returns></returns>
    private Bsp BuildBsp(Area floorArea, int minPartitionSize, float splitNoise)
    {
        var root = floorArea;
        var tree = new Bsp(root);
        var queue = new Stack<Bsp>();
        queue.Push(tree);
        while (queue.Count > 0)
        {
            var node = queue.Pop();
            node.Area.Split(_randomService, minPartitionSize, splitNoise).Execute(split =>
            {
                node.Left = new Bsp(split.Item1);
                node.Right = new Bsp(split.Item2);

                // process children in random order
                if (_randomService.Next(2) == 0)
                {
                    queue.Push(node.Left);
                    queue.Push(node.Right);
                }
                else
                {
                    queue.Push(node.Right);
                    queue.Push(node.Left);
                }
            });
        }

        return tree;
    }
}