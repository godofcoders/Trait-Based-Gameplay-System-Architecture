using System;
using GameplayInfrastructure.Core;

namespace GameplayInfrastructure.Traits
{
    public sealed class BurnableTrait : Trait, ITickableTrait
    {
        private readonly float _damagePerSecond;
        private readonly float _extinguishThreshold;

        public BurnableTrait(float damagePerSecond, float extinguishThreshold = 0f)
        {
            Numeric.RequireNonNegative(damagePerSecond, nameof(damagePerSecond));
            Numeric.RequireNonNegative(extinguishThreshold, nameof(extinguishThreshold));
            _damagePerSecond = damagePerSecond;
            _extinguishThreshold = extinguishThreshold;
        }

        protected override void OnInitialize()
        {
            if (!Context.HasAttribute(AttributeHashes.Health) || !Context.HasAttribute(AttributeHashes.MaxHealth))
                throw new InvalidOperationException("BurnableTrait requires HealthTrait to initialize first.");
            if (_extinguishThreshold > Context.GetValue(AttributeHashes.MaxHealth))
                throw new InvalidOperationException("Extinguish threshold cannot exceed maximum health.");
            Context.Register(AttributeHashes.IsBurning, 0f, 0f, 1f);
            Context.OnAttributeChanged += HandleAttributeChanged;
        }

        private void HandleAttributeChanged(int id, float oldValue, float newValue)
        {
            // Unsubscription does not remove callbacks from an in-flight event snapshot.
            if (IsDisposed) return;
            if (id == AttributeHashes.Health || id == AttributeHashes.IsBurning)
                ExtinguishIfNeeded(Context);
        }

        private void ExtinguishIfNeeded(IAttributeContext context)
        {
            if (context.GetValue(AttributeHashes.Health) <= _extinguishThreshold &&
                context.GetValue(AttributeHashes.IsBurning) > 0f)
                context.SetValue(AttributeHashes.IsBurning, 0f);
        }

        public void Tick(float deltaTime)
        {
            Numeric.RequireNonNegative(deltaTime, nameof(deltaTime));
            var context = Context;
            if (deltaTime == 0f || context.GetValue(AttributeHashes.IsBurning) <= 0f) return;
            float health = context.GetValue(AttributeHashes.Health);
            // Double intermediates prevent overflow for large but finite inputs.
            float next = (float)Math.Max(_extinguishThreshold,
                (double)health - (double)_damagePerSecond * deltaTime);
            context.SetValue(AttributeHashes.Health, Math.Min(health, next));
            // A death observer can synchronously dispose the entity; retain this local context.
            ExtinguishIfNeeded(context);
        }

        protected override void OnDispose(IAttributeContext context)
        {
            context.OnAttributeChanged -= HandleAttributeChanged;
        }
    }
}
