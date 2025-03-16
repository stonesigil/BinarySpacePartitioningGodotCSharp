using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Godot;

namespace BinarySpacePartitioningGodotCSharp.BinarySpacePartitioning;

public class Area 
{
    public static readonly Area EmptyArea = new(Vector2I.Zero, Vector2I.One);
    private readonly Vector2I _coord;
    private readonly Vector2I _size;

    public Area(Vector2I coord, Vector2I size)
    {
        if (size.X <= 0 || size.Y <= 0)
            throw new ArgumentException("Areas must be strictly positive!");

        if (coord.X < 0 || coord.Y < 0)
            throw new ArgumentException("Coordinates must be positive!");

        _coord = coord;
        Coord = coord;
        _size = size;
    }

    private Vector2I TopLeft => _coord;
    private Vector2I BottomRight => _coord + _size - Vector2I.One;
    public Vector2I Coord { get; }
    public Vector2I Size => _size;
    public Vector2I Center => _coord + (_size - Vector2I.One) / 2;
    public int XMin => TopLeft.X;
    public int XMax => BottomRight.X;
    public int YMin => TopLeft.Y;
    public int YMax => BottomRight.Y;
    public bool IsEmpty => Equals(this, EmptyArea);
    
    public IEnumerable<Vector2I> GetCoords()
    {
        for (var x = XMin; x <= XMax; x++)
        for (var y = YMin; y <= YMax; y++)
            yield return new (x, y);
    }

    public IEnumerable<Vector2I> GetSurroundingCoords()
    {
        // top and bottom sides, plus corners
        for (var x = XMin - 1; x <= XMax + 1; x++)
        {
            yield return new(x, YMin - 1);
            yield return new(x, YMax + 1);
        }

        // left and right sides
        for (var y = YMin; y <= YMax; y++)
        {
            yield return new(XMin - 1, y);
            yield return new(XMax + 1, y);
        }
    }

    /// <summary>
    /// Returns a new area with its minimum and maximum X coords
    /// inside <paramref name="minX"/> and <paramref name="maxX"/>, inclusively.
    /// </summary>
    /// <param name="minX"></param>
    /// <param name="maxX"></param>
    /// <returns></returns>
    public Area ClampHorizontally(int minX, int maxX) => FromPoints(
        new(Math.Max(minX, XMin), YMin),
        new(Math.Min(maxX, XMax), YMax));

    /// <summary>
    /// Returns a new area with its minimum and maximum Y coords
    /// inside <paramref name="minY"/> and <paramref name="maxY"/>, inclusively.///
    /// </summary>
    /// <param name="minY"></param>
    /// <param name="maxY"></param>
    /// <returns></returns>
    public Area ClampVertically(int minY, int maxY) => FromPoints(
        new(XMin, Math.Max(minY, YMin)),
        new(XMax, Math.Min(maxY, YMax)));

    public CardinalDirectionOverlaps CardinalDirectionsOverlappingAreas(Area area)
    {
        var h = HorizontallyOverlappingAreas(area);
        var v = VerticallyOverlappingAreas(area);
        if (!h.IsEmpty) return h;
        if (!v.IsEmpty) return v;
        return CardinalDirectionOverlaps.EmptyOverlap;
    }

    private CardinalDirectionOverlaps HorizontallyOverlappingAreas(Area area)
    {
        var minY = Math.Max(YMin, area.YMin);
        var maxY = Math.Min(YMax, area.YMax);
        return minY <= maxY
            ? new CardinalDirectionOverlaps(
                ClampVertically(minY, maxY), 
                area.ClampVertically(minY, maxY), 
                Direction.Horizontal)
            : CardinalDirectionOverlaps.EmptyOverlap;
    }

    private CardinalDirectionOverlaps VerticallyOverlappingAreas(Area area)
    {
        var minX = Math.Max(XMin, area.XMin);
        var maxX = Math.Min(XMax, area.XMax);
        return minX <= maxX
            ? new CardinalDirectionOverlaps(
                ClampHorizontally(minX, maxX), 
                area.ClampHorizontally(minX, maxX),
                Direction.Vertical)
            : CardinalDirectionOverlaps.EmptyOverlap;
    }

    public int Distance(Area area)
    {
        var dx = VerticallyOverlappingAreas(area).IsEmpty
            ? Math.Min(Math.Abs(TopLeft.X - area.BottomRight.X), Math.Abs(BottomRight.X - area.TopLeft.X))
            : 0;

        var dy = HorizontallyOverlappingAreas(area).IsEmpty
            ? Math.Min(Math.Abs(TopLeft.Y - area.BottomRight.Y), Math.Abs(BottomRight.Y - area.TopLeft.Y))
            : 0;
        
        return dx + dy;
    }
    
    public int Distance(Vector2I coord)
    {
        if (Contains(coord)) return 0;
        var closestX = Mathf.Clamp(coord.X, XMin, XMax);
        var closestY = Mathf.Clamp(coord.Y, YMin, YMax);
        var closestCoord = new Vector2I() { X = closestX, Y = closestY };
        var distance = Math.Abs(coord.X - closestCoord.X) + Math.Abs(coord.Y - closestCoord.Y);
        return distance;
    }

    public IEnumerable<Vector2I> WallCoords() => GetSurroundingCoords();

    public bool Contains(Vector2I coord) =>
        XMin <= coord.X && coord.X <= XMax &&
        YMin <= coord.Y && coord.Y <= YMax;

    public override int GetHashCode() => HashCode.Combine(Coord, Size);

    public override bool Equals(object obj) => obj switch
    {
        Area otherArea => otherArea._coord == _coord && otherArea._size == _size,
        _ => false
    };

    internal bool BothSidesAreLargerThan(int size) => this._size.X > size && this._size.Y > size;

    /// <summary>
    /// Creates <c>SquareArea</c> spanned by <paramref name="coord1"/> and <paramref name="coord2"/>.
    /// Optionally specify <paramref name="minSize"/> to ensure no dimension is smaller than the specified size.
    /// This is useful when creating corridors or projecting coordinates meant for areas in other contexts.
    /// </summary>
    /// <param name="coord1"></param>
    /// <param name="coord2"></param>
    /// <param name="minSize"></param>
    /// <returns></returns>
    public static Area FromPoints(Vector2I coord1, Vector2I coord2, int minSize = 1)
    {
        var topLeft = new Vector2I(
            Math.Min(coord1.X, coord2.X),
            Math.Min(coord1.Y, coord2.Y));

        var bottomRight = new Vector2I(
            Math.Max(coord1.X, coord2.X) + 1,
            Math.Max(coord1.Y, coord2.Y) + 1);

        var size = bottomRight - topLeft;
        size.X = Math.Max(size.X, minSize);
        size.Y = Math.Max(size.Y, minSize);
        return new(topLeft, size);
    }

    public bool OverlapsWith(Area area)
    {
        // If one rectangle is on left side of other
        if (area.BottomRight.X < XMin || XMax < area.TopLeft.X)
            return false;

        // If one rectangle is above other
        if (YMax < area.TopLeft.Y || area.BottomRight.Y < YMin)
            return false;

        return true;
    }


    public Area Shrink(int n)
    {
        // Calculate the new coordinates and size
        var newCoord = new Vector2I(_coord.X + n, _coord.Y + n);
        var newSize = new Vector2I(_size.X - 2 * n, _size.Y - 2 * n);

        // Ensure the size doesn't go negative
        if (newSize.X < 1) newSize.X = 1;
        if (newSize.Y < 1) newSize.Y = 1;

        return new Area(newCoord, newSize);
    }

    private int[] PossibleSplits(int minPartitionSize)
    {
        var possibleSplits = new List<int>();
        if (minPartitionSize < Size[0]) possibleSplits.Add(0);
        if (minPartitionSize < Size[1]) possibleSplits.Add(1);
        return [.. possibleSplits];
    }

    internal Maybe<(Area, Area)> Split(RandomService random, int minPartitionSize)
    {
        var possibleSplits = PossibleSplits(minPartitionSize);
        if (possibleSplits.IsEmpty()) return Maybe.None;


        var splitIndex = random.Choice(possibleSplits);
        var keepIndex = 1 - splitIndex;

        var size1 = new Vector2I();
        var coord1 = Coord;;
        var mean = Size[splitIndex] / 2;
        var dev = (int)(Size[splitIndex] * 0.05);
        size1[keepIndex] = Size[keepIndex];
        size1[splitIndex] = random.Next(mean - dev, mean + dev);

        var size2 = new Vector2I
        {
            [keepIndex] = Size[keepIndex],
            [splitIndex] = Size[splitIndex] - size1[splitIndex]
        };

        var coord2 = Coord;
        coord2[splitIndex] += size1[splitIndex];

        var section1 = new Area(coord1, size1);
        var section2 = new Area(coord2, size2);
        return Maybe.From((section1, section2));
    }
}