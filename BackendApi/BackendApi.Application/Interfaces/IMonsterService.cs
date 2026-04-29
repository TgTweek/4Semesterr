using BackendApi.Application.DTOs.Combat;

namespace BackendApi.Application.Interfaces
{
    public interface IMonsterService
    {
        Task<IEnumerable<MonsterDto>> GetAllAsync();
        Task<MonsterDto?> GetByKeyAsync(string monsterKey);
    }
}