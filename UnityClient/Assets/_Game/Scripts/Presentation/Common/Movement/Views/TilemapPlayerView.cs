using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Presentation.Common.Movement.Views
{
    public sealed class TilemapPlayerView : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3f;

        private Grid _grid;
        private Coroutine _moveRoutine;

        public void Initialize(Grid grid)
        {
            _grid = grid;
        }

        public Vector3Int GetCurrentCell()
        {
            return _grid.WorldToCell(transform.position);
        }

        public void SnapToCell(Vector3Int cell)
        {
            transform.position = _grid.GetCellCenterWorld(cell);
        }

        public void MoveAlong(IReadOnlyList<Vector3Int> path)
        {
            if (_moveRoutine != null)
            {
                StopCoroutine(_moveRoutine);
            }

            _moveRoutine = StartCoroutine(MoveRoutine(path));
        }

        private IEnumerator MoveRoutine(IReadOnlyList<Vector3Int> path)
        {
            if (path == null || path.Count <= 1)
            {
                _moveRoutine = null;
                yield break;
            }

            for (var i = 1; i < path.Count; i++)
            {
                var targetPosition = _grid.GetCellCenterWorld(path[i]);

                while ((transform.position - targetPosition).sqrMagnitude > 0.0001f)
                {
                    transform.position = Vector3.MoveTowards(
                        transform.position,
                        targetPosition,
                        moveSpeed * Time.deltaTime);

                    yield return null;
                }

                transform.position = targetPosition;
            }

            _moveRoutine = null;
        }
    }
}