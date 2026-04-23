using System;
using System.Collections.Generic;

namespace Game.Domain.ValueObjects
{
    public readonly struct CellPosition : IEquatable<CellPosition>
    {
        public int X { get; }
        public int Y { get; }

        public CellPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public IEnumerable<CellPosition> GetCardinalNeighbors()
        {
            yield return new CellPosition(X + 1, Y);
            yield return new CellPosition(X - 1, Y);
            yield return new CellPosition(X, Y + 1);
            yield return new CellPosition(X, Y - 1);
        }

        public bool Equals(CellPosition other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is CellPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}