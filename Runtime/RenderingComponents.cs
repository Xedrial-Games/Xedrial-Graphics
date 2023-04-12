using System;

using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Xedrial.Rendering.Tilemaps;

namespace Xedrial.Rendering
{
    public struct TilemapGroupComponent : ISharedComponentData, IEquatable<TilemapGroupComponent>
    {
        public Grid<Tilemap> Tilemaps;
        public Material Material;
        
        private Func<Grid<Tilemap>, int2, Tilemap> ObjectInitializer;

        public TilemapGroupComponent(int tilemapSize, Material material)
        {
            ObjectInitializer = (grid, cell) =>
            {
                float3 origin = grid.GetWorldPosition(cell);
                return new Tilemap(tilemapSize, tilemapSize, 1f, origin);
            };
            
            Tilemaps = new Grid<Tilemap>(1, 1, tilemapSize, float3.zero, ObjectInitializer);
            Material = material;
        }

        public Tile SetTile(float3 worldPosition, Tile tile)
        {
            int2 cellPosition = Tilemaps.GetCellPosition(worldPosition);
            if (Tilemaps.GridArray.ContainsKey(cellPosition))
                return Tilemaps.GridArray[cellPosition].SetTile(worldPosition, tile);
            
            return Tilemaps.GridArray.TryAdd(cellPosition, ObjectInitializer(Tilemaps, cellPosition)) 
                ? Tilemaps.GridArray[cellPosition].SetTile(worldPosition, tile) : Tile.Null;
        }

        public bool Equals(TilemapGroupComponent other)
        {
            return Tilemaps.Equals(other.Tilemaps) && Equals(Material, other.Material);
        }

        public override bool Equals(object obj)
        {
            return obj is TilemapGroupComponent other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Tilemaps, Material);
        }
    }
}
