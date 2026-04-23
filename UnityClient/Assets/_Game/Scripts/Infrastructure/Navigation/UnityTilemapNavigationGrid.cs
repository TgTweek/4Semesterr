using Game.Application.Abstractions;
using Game.Domain.ValueObjects;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Infrastructure.Navigation
{
    public sealed class UnityTilemapNavigationGrid : INavigationGrid
    {
        private readonly Tilemap _groundTilemap;
        private readonly Tilemap _blockingTilemap;

        public UnityTilemapNavigationGrid(Tilemap groundTilemap, Tilemap blockingTilemap)
        {
            _groundTilemap = groundTilemap;
            _blockingTilemap = blockingTilemap;
        }

        public bool IsWalkable(CellPosition position)
        {
            var cell = new Vector3Int(position.X, position.Y, 0);

            if (_groundTilemap == null || !_groundTilemap.HasTile(cell))
            {
                return false;
            }

            if (_blockingTilemap != null && _blockingTilemap.HasTile(cell))
            {
                return false;
            }

            return true;
        }
    }
}