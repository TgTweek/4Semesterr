using Game.Domain.ValueObjects;

namespace Game.Application.Abstractions
{
    public interface INavigationGrid
    {
        bool IsWalkable(CellPosition position);
    }
}