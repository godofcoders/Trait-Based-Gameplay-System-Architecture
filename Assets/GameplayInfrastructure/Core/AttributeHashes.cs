namespace GameplayInfrastructure.Core
{
    // Stable IDs, not runtime string hashes. Keep this name for source compatibility.
    // Reserve new IDs here to avoid collisions between features.
    public static class AttributeHashes
    {
        public const int Health = 1;
        public const int MaxHealth = 2;
        public const int IsBurning = 3;
    }
}
