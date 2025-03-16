using BinarySpacePartitioningGodotCSharp.BinarySpacePartitioning;
using Godot;

namespace BinarySpacePartitioningGodotCSharp.Rooms;

[GlobalClass]
public partial class RoomGenerator : Node
{
    [Export] private Config _config;
    [Export] private RandomService _randomService;

    private int MinSize => _config.RoomMinSize;
    private int MinMargin => _config.RoomMinMargin;

    public Room Sample(Area area)
    {
        var maxSizeX = area.Size.X - 2 * MinMargin;
        var maxSizeY = area.Size.Y - 2 * MinMargin;
        if (maxSizeX < MinSize || maxSizeY < MinSize)
        {
            GD.PrintErr("Could not create room!");
            return null;
        }

        var sizeX = _randomService.Next(MinSize, maxSizeX);
        var sizeY = _randomService.Next(MinSize, maxSizeY);
        var size = new Vector2I(sizeX, sizeY);

        var minX = area.XMin + MinMargin;
        var minY = area.YMin + MinMargin;
        var maxX = area.XMax - sizeX;
        var maxY = area.YMax - sizeY;

        var coord = new Vector2I(
            _randomService.Next(minX, maxX),
            _randomService.Next(minY, maxY)
        );
        var room  = new Room(coord, size);
        return room;
    }
}