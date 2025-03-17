using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BinarySpacePartitioningGodotCSharp.Corridors;
using BinarySpacePartitioningGodotCSharp.Rooms;
using RoomGenerator = BinarySpacePartitioningGodotCSharp.Rooms.RoomGenerator;

namespace BinarySpacePartitioningGodotCSharp.BinarySpacePartitioning;

public class Bsp(Area area) : IEnumerable<Bsp>
{
    public readonly Area Area = area;
    private Room _room;
    private ICorridor _corridor;
    public Bsp Left;
    public Bsp Right;

    private bool IsLeaf => Left == null && Right == null;

    private int ManhattanDistance(Bsp other) => _room != null && other._room != null
        ? _room.Distance(other._room)
        : int.MaxValue;

    public void SampleRooms(RoomGenerator roomGenerator)
    {
        if (IsLeaf) _room = roomGenerator.Sample(Area);
        Left?.SampleRooms(roomGenerator);
        Right?.SampleRooms(roomGenerator);
    }

    public void SampleCorridors(CorridorGenerator corridorGenerator, OverlapCounter roomOverlapCounter)
    {
        if (IsLeaf) return;
        Left.SampleCorridors(corridorGenerator, roomOverlapCounter);
        Right.SampleCorridors(corridorGenerator, roomOverlapCounter);

        var closestPair = ClosestNodes(Left, Right);
        _corridor = corridorGenerator.New(closestPair.First._room, closestPair.Second._room, roomOverlapCounter);
    }

    public OverlapCounter CreateRoomOverlapCounter() => new(Rooms());

    public IEnumerator<Bsp> GetEnumerator()
    {
        if (Left != null)
            foreach (var v in Left)
                yield return v;

        if (Right != null)
            foreach (var v in Right)
                yield 
                    return v;

        yield return this;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerable<Bsp> Leaves() => this.Where(node => node.IsLeaf);
    public IEnumerable<Room> Rooms() => this.Where(node => node._room != null).Select(x => x._room);
    public IEnumerable<ICorridor> Corridors() => this.Where(node => node._corridor != null).Select(x => x._corridor);

    private static NodePair ClosestRoomsWorker(Bsp left, Bsp right, NodePair bestSoFar)
    {
        var newBestSoFar = MinDistance(bestSoFar, new NodePair(left, right));
        return (left.IsLeaf, right.IsLeaf) switch
        {
            (true, true) => newBestSoFar,

            (true, false) => MinDistance(
                ClosestRoomsWorker(left, right.Left, newBestSoFar),
                ClosestRoomsWorker(left, right.Right, newBestSoFar)
            ),

            (false, true) => MinDistance(
                ClosestRoomsWorker(left.Left, right, newBestSoFar),
                ClosestRoomsWorker(left.Right, right, newBestSoFar)
            ),

            (false, false) => new[]
            {
                ClosestRoomsWorker(left.Left, right.Left, newBestSoFar),
                ClosestRoomsWorker(left.Left, right.Right, newBestSoFar),
                ClosestRoomsWorker(left.Right, right.Left, newBestSoFar),
                ClosestRoomsWorker(left.Right, right.Right, newBestSoFar)
            }.Aggregate(bestSoFar, MinDistance)
        };
    }

    private record NodePair(Bsp First, Bsp Second)
    {
        public int ManhattanDistance => First.ManhattanDistance(Second);
    }

    private static NodePair ClosestNodes(Bsp left, Bsp right)
    {
        var initialSolution = new NodePair(left, right);
        return ClosestRoomsWorker(left, right, initialSolution);
    }

    private static NodePair MinDistance(NodePair a, NodePair b) =>
        a.ManhattanDistance < b.ManhattanDistance ? a : b;
}


public class OverlapCounter(IEnumerable<Area> areas)
{
    public int CountOverlaps(ICorridor corridor) => corridor.Sections
        .SelectMany(section => areas.Select(area => area.OverlapsWith(section)))
        .Count(x => x);
}