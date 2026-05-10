using System;
using System.Collections.Generic;
using Game.Domain.ValueObjects;

namespace Game.Application.Movement.Commands
{
    public sealed class FindPathCommand
    {
        public int StartX { get; }
        public int StartY { get; }
        public int TargetX { get; }
        public int TargetY { get; }

        public IReadOnlyList<CellPosition> BlockedCells { get; }

        public FindPathCommand(
            int startX,
            int startY,
            int targetX,
            int targetY,
            IReadOnlyList<CellPosition>? blockedCells = null)
        {
            StartX = startX;
            StartY = startY;
            TargetX = targetX;
            TargetY = targetY;
            BlockedCells = blockedCells ?? Array.Empty<CellPosition>();
        }
    }
}