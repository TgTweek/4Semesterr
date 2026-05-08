using System.Collections.Generic;
using System.Linq;
using Game.Domain.ValueObjects;

namespace Game.Application.Combat.UseCases
{
    public sealed class MonsterAdvanceUseCase
    {
        public CellPosition Execute(
            CellPosition monsterCell,
            CellPosition playerCell,
            int maxSteps,
            IReadOnlyCollection<CellPosition> blockedCells)
        {
            var currentCell = monsterCell;

            for (var step = 0; step < maxSteps; step++)
            {
                var nextCell = GetNextStep(currentCell, playerCell, blockedCells);

                if (nextCell.Equals(currentCell))
                {
                    break;
                }

                currentCell = nextCell;
            }

            return currentCell;
        }

        private static CellPosition GetNextStep(
            CellPosition currentCell,
            CellPosition targetCell,
            IReadOnlyCollection<CellPosition> blockedCells)
        {
            var currentDistance = GetDistance(currentCell, targetCell);

            var candidates = currentCell
                .GetCardinalNeighbors()
                .Where(x => !blockedCells.Contains(x))
                .OrderBy(x => GetDistance(x, targetCell))
                .ThenBy(x => x.X)
                .ThenBy(x => x.Y)
                .ToList();

            foreach (var candidate in candidates)
            {
                if (GetDistance(candidate, targetCell) < currentDistance)
                {
                    return candidate;
                }
            }

            return currentCell;
        }

        private static int GetDistance(CellPosition from, CellPosition to)
        {
            var dx = System.Math.Abs(from.X - to.X);
            var dy = System.Math.Abs(from.Y - to.Y);
            return dx + dy;
        }
    }
}