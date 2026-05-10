using System.Linq;
using Game.Application.Abstractions;
using Game.Application.Movement.Commands;
using Game.Application.Movement.Results;
using Game.Application.Movement.Services;
using Game.Domain.ValueObjects;

namespace Game.Application.Movement.UseCases
{
    public sealed class FindPathToCellUseCase
    {
        private readonly INavigationGrid _navigationGrid;
        private readonly GridPathfinder _gridPathfinder;

        public FindPathToCellUseCase(
            INavigationGrid navigationGrid,
            GridPathfinder gridPathfinder)
        {
            _navigationGrid = navigationGrid;
            _gridPathfinder = gridPathfinder;
        }

        public FindPathResult Execute(FindPathCommand command)
        {
            var start = new CellPosition(command.StartX, command.StartY);
            var target = new CellPosition(command.TargetX, command.TargetY);

            var path = _gridPathfinder.FindPath(
                start,
                target,
                _navigationGrid,
                command.BlockedCells);

            if (path.Count == 0)
            {
                return FindPathResult.CreateFailure("No path found.");
            }

            var dtoPath = path
                .Select(x => new PathPointDto(x.X, x.Y))
                .ToList();

            return FindPathResult.CreateSuccess(dtoPath);
        }
    }
}