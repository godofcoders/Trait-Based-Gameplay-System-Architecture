using GameplayInfrastructure.Core;
using GameplayInfrastructure.Traits;
using UnityEngine;

namespace GameplayInfrastructure.Data
{
    [CreateAssetMenu(fileName = "NewBurnConfig", menuName = "Traits/Burn Configuration")]
    public sealed class BurnConfig : TraitConfig
    {
        [Min(0f), Tooltip("Health lost per second while burning.")]
        public float DamagePerSecond = 5f;

        [Min(0f), Tooltip("Burn damage stops at this health and clears IsBurning. Must not exceed maximum health.")]
        public float ExtinguishThreshold = 0f;

        // Copy values: editing a shared asset cannot change an existing simulation.
        // Domain constructors validate these values in builds as well as the Editor.
        public override ITrait CreateTrait() => new BurnableTrait(DamagePerSecond, ExtinguishThreshold);
    }
}
