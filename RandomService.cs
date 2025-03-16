using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BinarySpacePartitioningGodotCSharp;

[GlobalClass]
public partial class RandomService : Node
{
    private Random _random;
    [Export] private int _seed;

    public int Seed
    {
        set
        {
            _seed = value;
            _random = new Random(value);
        }
    }

    public void Reset(int offset = 0) => Seed = _seed + offset;
    public int Next(int max) => _random.Next(max);
    public int Next(int min, int max) => _random.Next(min, max);
    public T Choice<T>(T[] items) => items[_random.Next(items.Length)];
    public T Choice<T>(IEnumerable<T> items) => Shuffle(items).First();
    public double NextDouble() => _random.NextDouble();
    public IEnumerable<T> Shuffle<T>(IEnumerable<T> items) => items.OrderBy(x => _random.Next());
}