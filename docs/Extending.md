# Add a trait: a regeneration exercise

Implement regeneration to learn how a new behavior fits the existing boundaries. The example below is an exercise, not an additional built-in mechanic.

## Write the domain behavior first

Create `Assets/GameplayInfrastructure/Trait/RegenerationTrait.cs`. Derive from `Trait` and implement `ITickableTrait`. Accept plain constructor values and validate them; do not accept a ScriptableObject or GameObject.

```csharp
using System;
using GameplayInfrastructure.Core;

namespace GameplayInfrastructure.Traits
{
    public sealed class RegenerationTrait : Trait, ITickableTrait
    {
        private readonly float _perSecond;

        public RegenerationTrait(float perSecond)
        {
            Numeric.RequireNonNegative(perSecond, nameof(perSecond));
            _perSecond = perSecond;
        }

        protected override void OnInitialize()
        {
            if (!Context.HasAttribute(AttributeHashes.Health) ||
                !Context.HasAttribute(AttributeHashes.MaxHealth))
                throw new InvalidOperationException("Regeneration requires health.");
        }

        public void Tick(float deltaTime)
        {
            Numeric.RequireNonNegative(deltaTime, nameof(deltaTime));
            var context = Context;
            float health = context.GetValue(AttributeHashes.Health);
            // No resurrection, and no healing while burning.
            if (health <= 0f || context.GetValue(AttributeHashes.IsBurning) > 0f)
                return;
            double healed = health + (double)_perSecond * deltaTime;
            float maximum = context.GetValue(AttributeHashes.MaxHealth);
            context.SetValue(AttributeHashes.Health, (float)Math.Min(maximum, healed));
        }
    }
}
```

This trait reads the optional burning capability using its default of zero. If your design requires burning to exist, check for that attribute during initialization instead. Make this dependency choice explicit.

## Add an authoring factory

Create `Assets/GameplayInfrastructure/Data/RegenerationConfig.cs`:

```csharp
using GameplayInfrastructure.Core;
using GameplayInfrastructure.Traits;
using UnityEngine;

namespace GameplayInfrastructure.Data
{
    [CreateAssetMenu(menuName = "Traits/Regeneration Configuration")]
    public sealed class RegenerationConfig : TraitConfig
    {
        [Min(0f)] public float HealthPerSecond = 5f;
        public override ITrait CreateTrait() => new RegenerationTrait(HealthPerSecond);
    }
}
```

Create the asset and add it to an entity's **Trait Configs** list. Neither `EntityController` nor `GameplayEntity` needs a new type-specific branch.

## Specify interaction rules and test them

Add NUnit tests to the EditMode test assembly before relying on the mechanic:

- Damage 100 health down to 50, tick two seconds at 5 health/second, and expect 60.
- Heal near maximum and verify the result clamps to maximum.
- Ignite the entity and verify regeneration pauses.
- Kill the entity and verify it stays dead.
- Reject a negative or non-finite regeneration rate.
- Dispose the entity and verify further ticks throw.

Composition order is observable. If burning extinguishes during a tick and regeneration runs after it, regeneration sees the extinguished state in that same tick. Reverse the list and it sees the burning state first. Decide whether that behavior suits your game. If you need simultaneous effects or strict phase ordering, introduce an explicit simulation phase model rather than relying on accidental list order.

The standalone test script explicitly includes the two domain test files it runs. Add any new domain test file there too; Unity Test Runner discovers new NUnit tests in the test assembly automatically.

For an event-only mechanic, implement only `Trait` and subscribe in `OnInitialize`. Unsubscribe in `OnDispose` using the provided context parameter, and guard in-flight callbacks with `IsDisposed`, as the built-in health and burn traits do.
