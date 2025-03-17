using BinarySpacePartitioningGodotCSharp.BinarySpacePartitioning;
using CSharpFunctionalExtensions;
using Godot;

namespace BinarySpacePartitioningGodotCSharp.Corridors;

[GlobalClass]
public partial class CorridorGenerator: Node
{
    [Export] private RandomService _randomService;
    [Export] private Config _config;

    public ICorridor New(Area fromArea, Area toArea, OverlapCounter overlapCounter) =>
        TryNewStraightCorridor(fromArea, toArea)
            .GetValueOrDefault(() => CreateBentCorridor(fromArea, toArea, overlapCounter));

    private Maybe<ICorridor> TryNewStraightCorridor(Area from, Area to)
    {
        var overlaps = from.CardinalDirectionsOverlappingAreas(to);
        if (overlaps.IsEmpty) return Maybe<ICorridor>.None;
        
        // attach the corridor to the correct edge of the `from` area
        var fromX = overlaps.Direction == Direction.Vertical
            ? overlaps.Overlap1.Center.X
            : from.Center.X < to.Center.X ? from.XMax + 1 : from.XMin - 1;
        
        var fromY = overlaps.Direction == Direction.Horizontal 
            ? overlaps.Overlap1.Center.Y 
            : from.Center.Y < to.Center.Y ? from.YMax + 1 : from.YMin - 1;
        
        var fromCoord = new Vector2I(fromX, fromY);
        
        
        // attach the corridor to the correct edge of the `to` area
        var toX = overlaps.Direction == Direction.Vertical
            ? overlaps.Overlap2.Center.X
            : to.Center.X < from.Center.X ? to.XMax + 1 : to.XMin - 1;
        
        var toY = overlaps.Direction == Direction.Horizontal 
            ? overlaps.Overlap2.Center.Y 
            : to.Center.Y < from.Center.Y ? to.YMax + 1 : to.YMin - 1;
        
        var toCoord = new Vector2I(toX, toY);
          
        var corridor = CreateStraightCorridor(fromCoord, toCoord, from, to);
        return corridor;
    }

    private StraightCorridor CreateStraightCorridor(Vector2I fromCoord, Vector2I toCoord, Area fromArea, Area toArea)
    {
        var segment = Area.FromPoints(fromCoord, toCoord, minSize: _config.CorridorWidth);
        return new StraightCorridor(segment, fromArea, toArea);
    }
    
    private ICorridor CreateBentCorridor(Area fromArea, Area toArea, OverlapCounter overlapCounter)
    {
        var yThenXBendCorridor = NewCorridorWithBend(fromArea, toArea, CorridorBend.YThenX);
        var yThenXOverlaps = overlapCounter.CountOverlaps(yThenXBendCorridor);

        var xThenYBendCorridor = NewCorridorWithBend(fromArea, toArea, CorridorBend.XThenY);
        var xThenYOverlaps = overlapCounter.CountOverlaps(xThenYBendCorridor);

        return (yThenXOverlaps, xThenYOverlaps) switch
        {
            var (yThenX, xThenY) when yThenX < xThenY => yThenXBendCorridor,
            var (yThenX, xThenY) when yThenX > xThenY => xThenYBendCorridor,
            _ => _randomService.Next(1) == 0 ? yThenXBendCorridor : xThenYBendCorridor,
        };
    }

    private BentCorridor NewCorridorWithBend(Area from, Area to, CorridorBend bend)
    {
        var fromCoord = from.Center;
        var toCoord = to.Center;
        var cornerCoord = bend switch
        {
            CorridorBend.YThenX => new Vector2I(fromCoord.X, toCoord.Y),
            CorridorBend.XThenY => new Vector2I(toCoord.X, fromCoord.Y),
            _ => new Vector2I(fromCoord.X, toCoord.Y) // just pick one default
        };

        var width = _config.CorridorWidth;
        var corner = new Area(cornerCoord, new Vector2I(width, width));
        var section1 = TryNewStraightCorridor(from, corner).GetValueOrThrow();
        var section2 = TryNewStraightCorridor(corner, to).GetValueOrThrow();

        var corridor = new BentCorridor(section1, section2, corner, from, to);
        return corridor;
    }
    
    public enum CorridorBend
    {
        YThenX, // First segment is vertical, second is horizontal
        XThenY // First segment is horizontal, second is vertical
    }
}