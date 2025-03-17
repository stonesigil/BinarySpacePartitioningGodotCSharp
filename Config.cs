using Godot;

namespace BinarySpacePartitioningGodotCSharp;

[GlobalClass]
public partial class Config: Node
{
    [Export] public int CorridorWidth { get; set; } = 1;
    [Export] public Vector2I FloorSize { get; set; } = new(50,50);
    [Export] public int FloorBorderPadding { get; set; } = 1;
    [Export] public int MinPartitionSize { get; set; } = 15;
    [Export] public int RoomMinSize { get; set; } = 4;
    [Export] public int RoomMinMargin { get; set; } = 1;
    [Export] public int NumLoops { get; set; } = 2;
    [Export] public float SplitNoise { get; set; } = 0.1f;
    [Export] public float LoopCorridorManhattanDistanceWeight { get; set; } = 2f;
    [Export] public float LoopCorridorGraphDistanceWeight { get; set; } = 1f;
}