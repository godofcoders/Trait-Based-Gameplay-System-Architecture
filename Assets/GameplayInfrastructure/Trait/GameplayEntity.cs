using System;
using System.Collections.Generic;
using GameplayInfrastructure.Core;

namespace GameplayInfrastructure.Traits
{
    // Owns a fixed composition for one lifetime. Health always initializes first;
    // optional traits initialize/tick in declaration order and dispose in reverse.
    public sealed class GameplayEntity : IDisposable
    {
        private readonly AttributeMap _attributes = new AttributeMap();
        private readonly List<ITrait> _traits = new List<ITrait>(4);
        private readonly List<ITickableTrait> _tickables = new List<ITickableTrait>(4);
        private readonly HealthTrait _health;
        private bool _disposed;
        private bool _ticking;

        public IAttributeContext Attributes => _attributes;
        public bool IsDead => _health.IsDead;
        public event Action Died;

        // After starting health is validated, ownership of supplied fresh traits transfers here.
        public GameplayEntity(float startingHealth, params ITrait[] traits)
        {
            _health = new HealthTrait(startingHealth);
            _traits.Add(_health);
            try
            {
                if (traits == null) throw new ArgumentNullException(nameof(traits));
                var types = new HashSet<Type> { typeof(HealthTrait) };
                foreach (var trait in traits)
                {
                    // Collect first, so even a null slot releases all other supplied traits.
                    if (trait != null && !_traits.Exists(owned => ReferenceEquals(owned, trait)))
                        _traits.Add(trait);
                }
                foreach (var trait in traits)
                {
                    if (trait == null) throw new ArgumentException("Trait instances cannot be null.", nameof(traits));
                    if (!types.Add(trait.GetType()))
                        throw new ArgumentException("Only one trait of each concrete type is supported.", nameof(traits));
                }
                foreach (var trait in _traits)
                {
                    trait.Initialize(_attributes);
                    if (trait is ITickableTrait tickable) _tickables.Add(tickable);
                }
                _health.Died += HandleDeath;
            }
            catch (Exception initializationError)
            {
                try { Dispose(); }
                catch (Exception cleanupError) { throw new AggregateException(initializationError, cleanupError); }
                throw;
            }
        }

        private void HandleDeath() => Died?.Invoke();

        public void Tick(float deltaTime)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(GameplayEntity));
            Numeric.RequireNonNegative(deltaTime, nameof(deltaTime));
            if (_ticking) throw new InvalidOperationException("Recursive entity ticks are not supported.");
            if (IsDead || deltaTime == 0f) return;
            _ticking = true;
            try
            {
                for (int i = 0; i < _tickables.Count && !_disposed && !IsDead; i++)
                    _tickables[i].Tick(deltaTime);
            }
            finally { _ticking = false; }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _health.Died -= HandleDeath;
            Died = null;
            List<Exception> errors = null;
            for (int i = _traits.Count - 1; i >= 0; i--)
            {
                try { _traits[i].Dispose(); }
                catch (Exception error)
                {
                    if (errors == null) errors = new List<Exception>();
                    errors.Add(error);
                }
            }
            _tickables.Clear();
            _traits.Clear();
            if (errors != null) throw new AggregateException("Trait cleanup failed.", errors);
        }
    }
}
