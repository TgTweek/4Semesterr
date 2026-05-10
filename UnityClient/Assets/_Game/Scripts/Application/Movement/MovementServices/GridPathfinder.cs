using System;
using System.Collections.Generic;
using System.Linq;
using Game.Application.Abstractions;
using Game.Domain.ValueObjects;

namespace Game.Application.Movement.Services
{
    public sealed class GridPathfinder
    {
        public IReadOnlyList<CellPosition> FindPath(
            CellPosition start,
            CellPosition target,
            INavigationGrid navigationGrid,
            IReadOnlyCollection<CellPosition>? blockedCells = null)
        {
            if (start.Equals(target))
            {
                return new List<CellPosition> { start };
            }

            if (!navigationGrid.IsWalkable(start) || !navigationGrid.IsWalkable(target))
            {
                return Array.Empty<CellPosition>();
            }

            var blockedSet = blockedCells != null
                ? new HashSet<CellPosition>(blockedCells)
                : new HashSet<CellPosition>();

            if (blockedSet.Contains(target))
            {
                return Array.Empty<CellPosition>();
            }

            var openSet = new List<CellPosition> { start };
            var cameFrom = new Dictionary<CellPosition, CellPosition>();

            var gScore = new Dictionary<CellPosition, int>
            {
                [start] = 0
            };

            var fScore = new Dictionary<CellPosition, int>
            {
                [start] = Heuristic(start, target)
            };

            while (openSet.Count > 0)
            {
                var current = GetLowestFScore(openSet, fScore);

                if (current.Equals(target))
                {
                    return ReconstructPath(cameFrom, current);
                }

                openSet.Remove(current);

                foreach (var neighbor in GetOrderedNeighbors(current, target))
                {
                    if (!navigationGrid.IsWalkable(neighbor))
                    {
                        continue;
                    }

                    if (blockedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    var tentativeGScore = gScore[current] + 1;

                    if (!gScore.TryGetValue(neighbor, out var existingGScore) || tentativeGScore < existingGScore)
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = tentativeGScore + Heuristic(neighbor, target);

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            return Array.Empty<CellPosition>();
        }

        private static IEnumerable<CellPosition> GetOrderedNeighbors(CellPosition current, CellPosition target)
        {
            return current
                .GetCardinalNeighbors()
                .OrderBy(x => Heuristic(x, target))
                .ThenBy(x => x.X)
                .ThenBy(x => x.Y);
        }

        private static int Heuristic(CellPosition a, CellPosition b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private static CellPosition GetLowestFScore(
            List<CellPosition> openSet,
            Dictionary<CellPosition, int> fScore)
        {
            var best = openSet[0];
            var bestScore = fScore.TryGetValue(best, out var score) ? score : int.MaxValue;

            for (var i = 1; i < openSet.Count; i++)
            {
                var candidate = openSet[i];
                var candidateScore = fScore.TryGetValue(candidate, out var value) ? value : int.MaxValue;

                if (candidateScore < bestScore)
                {
                    best = candidate;
                    bestScore = candidateScore;
                }
            }

            return best;
        }

        private static IReadOnlyList<CellPosition> ReconstructPath(
            Dictionary<CellPosition, CellPosition> cameFrom,
            CellPosition current)
        {
            var path = new List<CellPosition> { current };

            while (cameFrom.TryGetValue(current, out var previous))
            {
                current = previous;
                path.Add(current);
            }

            path.Reverse();
            return path;
        }
    }
}