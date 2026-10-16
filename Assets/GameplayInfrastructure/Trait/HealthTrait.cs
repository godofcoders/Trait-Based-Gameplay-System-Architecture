using System;
using GameplayInfrastructure.Core;

namespace GameplayInfrastructure.Traits
{
    public sealed class HealthTrait : Trait
    {
        private readonly float _initialHealth;
        public bool IsDead { get; private set; }
        public event Action Died;

        public HealthTrait(float initialHealth)
        {
            Numeric.RequireFinite(initialHealth, nameof(initialHealth));
            if (initialHealth <= 0f) throw new ArgumentOutOfRangeException(nameof(initialHealth));
            _initialHealth = initialHealth;
        }

        protected override void OnInitialize()
        {
            // Fixed maximum for this example. Equal bounds make MaxHealth immutable.
            Context.Register(AttributeHashes.MaxHealth, _initialHealth, _initialHealth, _initialHealth);
            Context.Register(AttributeHashes.Health, _initialHealth, 0f, _initialHealth);
            Context.OnAttributeChanged += HandleAttributeChanged;
        }

        private void HandleAttributeChanged(int id, float oldValue, float newValue)
        {
            // Unsubscription does not remove callbacks from an in-flight event snapshot.
            if (IsDisposed) return;
            if (id != AttributeHashes.Health || newValue > 0f || IsDead) return;
            IsDead = true; // Reentrant writes cannot repeat death.
            Died?.Invoke();
        }

        protected override void OnDispose(IAttributeContext context)
        {
            context.OnAttributeChanged -= HandleAttributeChanged;
            Died = null;
        }
    }
}
