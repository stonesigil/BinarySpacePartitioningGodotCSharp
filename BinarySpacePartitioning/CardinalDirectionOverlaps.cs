namespace BinarySpacePartitioningGodotCSharp.BinarySpacePartitioning;

public class CardinalDirectionOverlaps(Area overlap1, Area overlap2, Direction direction)
{
        public static CardinalDirectionOverlaps EmptyOverlap => new(Area.EmptyArea, Area.EmptyArea, Direction.Empty);
        public bool IsEmpty => Overlap1.IsEmpty && Overlap2.IsEmpty;
        public Area Overlap1 { get; } = overlap1;
        public Area Overlap2 { get; } = overlap2;
        public Direction Direction { get; } = direction;
}

public enum Direction { Horizontal, Vertical, Empty }