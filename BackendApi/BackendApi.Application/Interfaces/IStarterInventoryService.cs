namespace BackendApi.Application.Interfaces
{
    public interface IStarterInventoryService
    {
        Task GrantStarterCardsAsync(Guid playerId);
    }
}