using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Presentation.Common.Movement.Views
{
    public sealed class MovementTargetIndicatorView : MonoBehaviour
    {
        [SerializeField] private Tilemap highlightTilemap;
        [SerializeField] private TileBase highlightTile;

        private Vector3Int? _currentCell;

        public void Show(Vector3Int cell)
        {
            Clear();

            if (highlightTilemap == null || highlightTile == null)
            {
                return;
            }

            highlightTilemap.SetTile(cell, highlightTile);
            _currentCell = cell;
        }

        public void Clear()
        {
            if (highlightTilemap == null || !_currentCell.HasValue)
            {
                return;
            }

            highlightTilemap.SetTile(_currentCell.Value, null);
            _currentCell = null;
        }
    }
}