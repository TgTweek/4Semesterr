namespace Game.Application.Abstractions
{
    public interface ICardDefinitionCatalog
    {
        string GetName(string cardDefinitionId);
    }
}