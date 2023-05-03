using System;
using Unity.Mathematics;

namespace Xedrial.Graphics.Tilemaps
{
    public struct Tilemap : IDisposable
    {
        public Grid<Tile> Grid;

        public Tilemap(int width, int height, float cellSize, float3 originPosition)
        {
            Grid = new Grid<Tile>(width, height, cellSize, originPosition, (_,_) => Tile.Null);
        }

        public Tile SetTile(int2 cellPosition, Tile tile)
        {
            if (tile)
            {
                tile.Index = cellPosition;
                float3 translation = Grid.GetCellCenterWorld(cellPosition);
                translation.xy += tile.Offset;

                tile.Transform = float4x4.TRS(
                    translation,
                    quaternion.identity,
                    new float3(1)
                );
            }

            Grid[cellPosition] = tile;
            return tile;
        }

        public Tile SetTile(int x, int y, Tile tile) => SetTile(new int2(x, y), tile);

        public Tile SetTile(float3 worldPosition, Tile tile)
        {
            int2 cellPosition = Grid.GetCellPosition(worldPosition);
            return SetTile(cellPosition, tile);
        }

        public void Dispose() => Grid.Dispose();

        public bool Equals(Tilemap other)
        {
            return Grid.Equals(other.Grid);
        }

        public override bool Equals(object obj)
        {
            return obj is Tilemap other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Grid);
        }
    }
}
