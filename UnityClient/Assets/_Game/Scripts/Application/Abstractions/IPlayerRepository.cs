using Game.Domain.ValueObjects;

namespace Game.Application.Abstractions
{
    public interface IPlayerRepository
    {
        Game.Domain.Entities.Player GetById(PlayerId playerId);
        void Save(Game.Domain.Entities.Player player);
    }
}