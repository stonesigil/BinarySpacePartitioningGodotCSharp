using System.Collections.Generic;
using System.Linq;
using BinarySpacePartitioningGodotCSharp.BinarySpacePartitioning;
using BinarySpacePartitioningGodotCSharp.Corridors;
using BinarySpacePartitioningGodotCSharp.Rooms;

namespace BinarySpacePartitioningGodotCSharp.Floors;

public class FloorGraph
{
    private readonly List<ICorridor> _corridors = [];
    private readonly List<Room> _rooms = [];
    private readonly Dictionary<Area, List<(Area, ICorridor)>> _connections = [];

    public FloorGraph(IEnumerable<ICorridor> corridors, IEnumerable<Room> rooms)
    {
        _corridors.AddRange(corridors);
        _rooms.AddRange(rooms);
        foreach (var corridor in _corridors)
        {
            AddCorridor(corridor);
        }
    }

    public void AddCorridor(ICorridor corridor)
    {
        if (!_connections.ContainsKey(corridor.FromArea))
            _connections.Add(corridor.FromArea, []);

        if (!_connections.ContainsKey(corridor.ToArea))
            _connections.Add(corridor.ToArea, []);

        _connections[corridor.FromArea].Add((corridor.ToArea, corridor));
        _connections[corridor.ToArea].Add((corridor.FromArea, corridor));
    }

    private int GraphDistance(Area room, Area other)
    {
        var visited = new HashSet<Area>();
        var queue = new Queue<(Area Node, int Steps)>();
        queue.Enqueue((room, 0));

        while (queue.Count != 0)
        {
            var (current, steps) = queue.Dequeue();

            if (Equals(current, other))
                return steps;

            if (!visited.Add(current))
                continue;

            if (_connections.TryGetValue(current, out var corridors))
                foreach (var (toArea, corridor) in corridors)
                    queue.Enqueue((toArea, steps + 1));
        }

        return int.MaxValue; // Return a large value if no path is found
    }

    private int ManhattanDistance(Room first, Room second) =>
        first != null && second != null
            ? first.Distance(second)
            : int.MaxValue;

    /// <summary>
    /// Computes a list of pairwise Manhattan distances between all leaf nodes in the BSP tree.
    /// </summary>
    /// <returns>A list of tuples containing the distance and the pair of leaves.</returns>
    private IEnumerable<NodesWithDistance> ComputeDistancesBetweenDeadEnds()
    {
        for (var i = 0; i < _rooms.Count; i++)
        {
            for (var j = i + 1; j < _rooms.Count; j++)
            {
                if (i == j) continue;
                var firstNode = _rooms[i];
                var secondNode = _rooms[j];
                var manhattanDistance = ManhattanDistance(firstNode, secondNode);
                var graphDistance = GraphDistance(firstNode, secondNode);
                yield return new NodesWithDistance(manhattanDistance, graphDistance, firstNode, secondNode);
            }
        }
    }

    /// <summary>
    /// To prevent tons of backtracking for the player we identify leaf nodes that are close
    /// geographically but far apart in graph/connectedness sense and add corridors between them.
    /// </summary>
    public ICorridor CorridorToConnectWorstDeadEnd(
       CorridorGenerator corridorGenerator,
        OverlapCounter roomOverlapCounter) =>
        ComputeDistancesBetweenDeadEnds()
            .OrderByDescending(x => (float)x.GraphDistance / (2*x.ManhattanDistance)) // / x.GraphDistance)
            .Select(pair => corridorGenerator.New(pair.First, pair.Second, roomOverlapCounter))
            .First();

    public record struct NodesWithDistance(int ManhattanDistance, int GraphDistance, Room First, Room Second);
}