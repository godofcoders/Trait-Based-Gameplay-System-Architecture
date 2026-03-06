using UnityEngine;

namespace GameplayInfrastructure.Data
{
    [CreateAssetMenu(fileName = "NewBurnConfig", menuName = "Traits/Burn Configuration")]
    public class BurnConfig : ScriptableObject
    {
        [Tooltip("Damage taken per second while burning.")]
        public float DamagePerSecond = 5f;

        [Tooltip("At what health threshold should the fire automatically extinguish?")]
        public float ExtinguishThreshold = 0f;
    }
}