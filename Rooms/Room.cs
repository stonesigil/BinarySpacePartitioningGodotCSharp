using BinarySpacePartitioningGodotCSharp.BinarySpacePartitioning;
using Godot;

namespace BinarySpacePartitioningGodotCSharp.Rooms;

public class Room(Vector2I coord, Vector2I size) : Area(coord, size);