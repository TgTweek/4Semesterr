using Game.Domain.ValueObjects;

namespace Game.Application.Combat.UseCases
{
    public sealed class CheckTargetInRangeUseCase
    {
        public bool Execute(CellPosition fromCell, CellPosition toCell, int range)
        {
            var dx = System.Math.Abs(fromCell.X - toCell.X);
            var dy = System.Math.Abs(fromCell.Y - toCell.Y);

            return (dx + dy) <= range;
        }
    }
}