using System.Collections.Generic;
using System.Linq;
using Game.Application.Movement.Commands;
using Game.Application.Movement.UseCases;
using Game.Domain.ValueObjects;
using Game.Presentation.Common.Movement.Views;
using Game.Presentation.Combat.Controllers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Game.Presentation.Common.Movement.Controllers
{
    public sealed class ClickToMoveController : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera = null!;
        [SerializeField] private Grid grid = null!;
        [SerializeField] private TilemapPlayerView playerView = null!;
        [SerializeField] private MovementTargetIndicatorView targetIndicatorView = null!;
        [SerializeField] private BoardCombatController boardCombatController = null!;

        private FindPathToCellUseCase _findPathToCellUseCase = null!;

        private Vector3Int? _lastHoverStartCell;
        private Vector3Int? _lastHoverTargetCell;
        private List<Vector3Int> _cachedHoverPath = new();

        public void Initialize(FindPathToCellUseCase findPathToCellUseCase)
        {
            _findPathToCellUseCase = findPathToCellUseCase;

            playerView.Initialize(grid);

            var currentCell = playerView.GetCurrentCell();
            playerView.SnapToCell(currentCell);
        }

        private void Update()
        {
            if (_findPathToCellUseCase == null || worldCamera == null || grid == null || playerView == null)
            {
                return;
            }

            if (Mouse.current == null)
            {
                return;
            }

            if (boardCombatController != null && !boardCombatController.CanUseMovementInput())
            {
                ClearHoverState();
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                ClearHoverState();
                return;
            }

            var screenPosition = Mouse.current.position.ReadValue();
            var worldPoint = worldCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0f));
            worldPoint.z = 0f;

            var targetCell = grid.WorldToCell(worldPoint);
            var startCell = playerView.GetCurrentCell();

            UpdateHoverState(startCell, targetCell);

            if (!Mouse.current.leftButton.wasPressedThisFrame)
            {
                return;
            }

            if (_cachedHoverPath.Count <= 1)
            {
                return;
            }

            var movementCost = Mathf.Max(0, _cachedHoverPath.Count - 1);

            if (boardCombatController != null && !boardCombatController.TryConsumeMovement(movementCost))
            {
                return;
            }

            playerView.MoveAlong(_cachedHoverPath);
        }

        private void UpdateHoverState(Vector3Int startCell, Vector3Int targetCell)
        {
            if (_lastHoverStartCell.HasValue &&
                _lastHoverTargetCell.HasValue &&
                _lastHoverStartCell.Value == startCell &&
                _lastHoverTargetCell.Value == targetCell)
            {
                return;
            }

            _lastHoverStartCell = startCell;
            _lastHoverTargetCell = targetCell;

            var blockedCells = boardCombatController != null
                ? boardCombatController.GetOccupiedMonsterCells()
                : new List<CellPosition>();

            var result = _findPathToCellUseCase.Execute(
                new FindPathCommand(
                    startCell.x,
                    startCell.y,
                    targetCell.x,
                    targetCell.y,
                    blockedCells));

            if (!result.IsSuccess)
            {
                _cachedHoverPath.Clear();
                targetIndicatorView?.Clear();
                return;
            }

            _cachedHoverPath = result.Path
                .Select(x => new Vector3Int(x.X, x.Y, 0))
                .ToList();

            targetIndicatorView?.Show(targetCell);
        }

        private void ClearHoverState()
        {
            _lastHoverStartCell = null;
            _lastHoverTargetCell = null;
            _cachedHoverPath.Clear();
            targetIndicatorView?.Clear();
        }
    }
}