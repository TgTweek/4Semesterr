using Game.Application.Movement.Services;
using Game.Application.Movement.UseCases;
using Game.Infrastructure.Navigation;
using Game.Presentation.Common.Movement.Controllers;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Bootstrap
{
    public sealed class TilemapNavigationInstaller : MonoBehaviour
    {
        [SerializeField] private Tilemap groundTilemap;
        [SerializeField] private Tilemap blockingTilemap;
        [SerializeField] private ClickToMoveController clickToMoveController;

        private void Awake()
        {
            var navigationGrid = new UnityTilemapNavigationGrid(groundTilemap, blockingTilemap);
            var pathfinder = new GridPathfinder();
            var findPathUseCase = new FindPathToCellUseCase(navigationGrid, pathfinder);

            clickToMoveController.Initialize(findPathUseCase);
        }
    }
}