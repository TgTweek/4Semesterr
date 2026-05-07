namespace BackendApi.Domain.Entities
{
    public static class InventoryItemLocation
    {
        public const string Stash = "Stash";
        public const string Loadout = "Loadout";

        public static bool IsValid(string value)
        {
            return value == Stash || value == Loadout;
        }
    }
}