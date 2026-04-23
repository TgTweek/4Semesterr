using System;
using System.Collections.Generic;
using Game.Application.Abstractions;
using Game.Domain.ValueObjects;

namespace Game.Application.Movement.Services
{
    public sealed class GridPathfinder
    {
        public IReadOnlyList<CellPosition> FindPath(
            CellPosition start,
            CellPosition target,
            INavigationGrid navigationGrid)
        {
            if (start.Equals(target))
            {
                return new List<CellPosition> { start };
            }

            if (!navigationGrid.IsWalkable(start) || !navigationGrid.IsWalkable(target))
            {
                return Array.Empty<CellPosition>();
            }

            var frontier = new Queue<CellPosition>();
            var visited = new HashSet<CellPosition>();
            var cameFrom = new Dictionary<CellPosition, CellPosition>();

            frontier.Enqueue(start);
            visited.Add(start);

            var pathFound = false;

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                foreach (var neighbor in current.GetCardinalNeighbors())
                {
                    if (visited.Contains(neighbor))
                    {
                        continue;
                    }

                    if (!navigationGrid.IsWalkable(neighbor))
                    {
                        continue;
                    }

                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;

                    if (neighbor.Equals(target))
                    {
                        pathFound = true;
                        frontier.Clear();
                        break;
                    }

                    frontier.Enqueue(neighbor);
                }
            }

            if (!pathFound)
            {
                return Array.Empty<CellPosition>();
            }

            var path = new List<CellPosition> { target };
            var step = target;

            while (!step.Equals(start))
            {
                step = cameFrom[step];
                path.Add(step);
            }

            path.Reverse();
            return path;
        }
    }
}