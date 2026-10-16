using System;
using System.Collections.Generic;
using GameplayInfrastructure.Core;
using GameplayInfrastructure.Data;
using GameplayInfrastructure.Traits;
using UnityEngine;

namespace GameplayInfrastructure.Unity
{
    public sealed class EntityController : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float _startingHealth = 100f;
        [SerializeField] private List<TraitConfig> _traitConfigs = new List<TraitConfig>();
        private GameplayEntity _entity;
        private bool _destroyed;
        private bool _initializing;

        private void Awake() => EnsureInitialized();

        private void EnsureInitialized()
        {
            if (_destroyed) throw new ObjectDisposedException(nameof(EntityController));
            if (_entity != null) return;
            if (_initializing) throw new InvalidOperationException("Recursive entity initialization.");
            _initializing = true;
            var traits = new List<ITrait>(_traitConfigs.Count);
            bool transferred = false;
            try
            {
                foreach (var config in _traitConfigs)
                {
                    if (config == null) throw new InvalidOperationException("Assign every trait configuration slot.");
                    var trait = config.CreateTrait();
                    if (trait == null) throw new InvalidOperationException("Trait factories must return a fresh instance.");
                    traits.Add(trait);
                }
                // Validate health before handing over ownership, including on factory failure.
                Numeric.RequireFinite(_startingHealth, nameof(_startingHealth));
                if (_startingHealth <= 0f) throw new ArgumentOutOfRangeException(nameof(_startingHealth));
                transferred = true;
                _entity = new GameplayEntity(_startingHealth, traits.ToArray());
                _entity.Died += HandleDeath;
                if (_entity.IsDead) HandleDeath();
            }
            catch (Exception initializationError)
            {
                enabled = false;
                if (!transferred)
                {
                    var errors = new List<Exception> { initializationError };
                    foreach (var trait in traits)
                    {
                        try { trait.Dispose(); }
                        catch (Exception cleanupError) { errors.Add(cleanupError); }
                    }
                    if (errors.Count > 1) throw new AggregateException(errors);
                }
                throw;
            }
            finally { _initializing = false; }
        }

        private void Update() => _entity?.Tick(Time.deltaTime);

        private void HandleDeath()
        {
            enabled = false;
            Destroy(gameObject); // Unity presentation/lifetime policy belongs here.
        }

        private void OnDestroy()
        {
            _destroyed = true;
            if (_entity == null) return;
            _entity.Died -= HandleDeath;
            _entity.Dispose();
            _entity = null;
        }

        // Lazy initialization also supports callers from another component's Awake.
        public IAttributeContext GetContext()
        {
            EnsureInitialized();
            return _entity.Attributes;
        }
    }
}
