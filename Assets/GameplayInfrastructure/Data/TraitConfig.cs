using GameplayInfrastructure.Core;
using UnityEngine;

namespace GameplayInfrastructure.Data
{
    // Asset = authoring data + factory. Each call must return a fresh runtime trait.
    public abstract class TraitConfig : ScriptableObject
    {
        public abstract ITrait CreateTrait();
    }
}
