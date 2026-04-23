using System.Linq;
using Game.Application.Movement.Commands;
using Game.Application.Movement.UseCases;
using Game.Presentation.Common.Movement.Views;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Game.Presentation.Common.Movement.Controllers
{
    public sealed class ClickToMoveController : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Grid grid;
        [SerializeField] private TilemapPlayerView playerView;

        private FindPathToCellUseCase _findPathToCellUseCase;

        public void Initialize(FindPathToCellUseCase findPathToCellUseCase)
        {
            _findPathToCellUseCase = findPathToCellUseCase;

            playerView.Initialize(grid);

            var currentCell = playerView.GetCurrentCell();
            playerView.SnapToCell(currentCell);
        }

        private void Update()
        {
            if (_findPathToCellUseCase == null)
            {
                return;
            }

            if (Mouse.current == null)
            {
                return;
            }

            if (!Mouse.current.leftButton.wasPressedThisFrame)
            {
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            var screenPosition = Mouse.current.position.ReadValue();
            var worldPoint = worldCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0f));
            worldPoint.z = 0f;

            var targetCell = grid.WorldToCell(worldPoint);
            var startCell = playerView.GetCurrentCell();

            var result = _findPathToCellUseCase.Execute(
                new FindPathCommand(
                    startCell.x,
                    startCell.y,
                    targetCell.x,
                    targetCell.y));

            if (!result.IsSuccess)
            {
                return;
            }

            var path = result.Path
                .Select(x => new Vector3Int(x.X, x.Y, 0))
                .ToList();

            playerView.MoveAlong(path);
        }
    }
}