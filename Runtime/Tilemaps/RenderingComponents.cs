using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Xedrial.Graphics.Tilemaps
{
    public class TilemapGroupComponent : IComponentData
    {
        public Grid<Tilemap> Tilemaps;
        public Material Material;
        
        private Func<Grid<Tilemap>, int2, Tilemap> m_ObjectInitializer;

        public void Initialize(int tilemapSize, Material material)
        {
            m_ObjectInitializer = (grid, cell) =>
            {
                float3 origin = grid.GetWorldPosition(cell);
                return new Tilemap(tilemapSize, tilemapSize, 1f, origin);
            };
            
            Tilemaps = new Grid<Tilemap>(1, 1, tilemapSize, float3.zero, m_ObjectInitializer);
            Material = material;
        }

        public Tile SetTile(float3 worldPosition, Tile tile)
        {
            int2 cellPosition = Tilemaps.GetCellPosition(worldPosition);
            if (Tilemaps.GridArray.ContainsKey(cellPosition))
                return Tilemaps.GridArray[cellPosition].SetTile(worldPosition, tile);
            
            return Tilemaps.GridArray.TryAdd(cellPosition, m_ObjectInitializer(Tilemaps, cellPosition)) 
                ? Tilemaps.GridArray[cellPosition].SetTile(worldPosition, tile) : Tile.Null;
        }
    }
}
