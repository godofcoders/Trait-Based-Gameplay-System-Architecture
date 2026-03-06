// A utility to convert strings to integer hashes at startup
namespace GameplayInfrastructure.Core
{
    public static class AttributeHashes
    {
        public static readonly int Health = UnityEngine.Animator.StringToHash("Health");
        public static readonly int MaxHealth = UnityEngine.Animator.StringToHash("MaxHealth");
        public static readonly int IsBurning = UnityEngine.Animator.StringToHash("IsBurning");
    }
}