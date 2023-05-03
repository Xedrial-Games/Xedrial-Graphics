using System;
using Unity.Mathematics;

namespace Xedrial.Graphics.Tilemaps
{
    public struct Tile : IEquatable<Tile>
    {
        public int2 Index;
        public float2 Offset;
        public float4x4 Transform;
        public float4 Uv;

        public static readonly Tile Null = new(){ Index = -1 };

        public static bool operator ==(Tile tile1, Tile tile2) => tile1.Equals(tile2);

        public static bool operator !=(Tile tile1, Tile tile2) => !tile1.Equals(tile2);

        public static implicit operator bool(Tile tile) => tile != Null;

        public bool Equals(Tile other)
            => Index.Equals(other.Index) &&
               Transform.Equals(other.Transform) &&
               Uv.Equals(other.Uv);

        public override bool Equals(object obj) => obj is Tile other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Index);
    }
}
