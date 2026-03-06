using UnityEngine;
using GameplayInfrastructure.Core;
using GameplayInfrastructure.Data;

namespace GameplayInfrastructure.Traits
{
    public class HealthTrait : ITrait
    {
        private IAttributeContext _context;
        private readonly GameObject _owner;

        // Injecting the owner object so we can destroy it, keeping logic decoupled
        public HealthTrait(GameObject owner, float initialHealth)
        {
            _owner = owner;
            // Note: In a full system, initialHealth would also come from a ScriptableObject configuration
            _initialHealth = initialHealth;
        }

        private float _initialHealth;

        public void Initialize(IAttributeContext context)
        {
            _context = context;
            _context.SetValue(AttributeHashes.MaxHealth, _initialHealth);
            _context.SetValue(AttributeHashes.Health, _initialHealth);

            // Subscribe to attribute changes
            _context.OnAttributeChanged += HandleAttributeChanged;
        }

        private void HandleAttributeChanged(int hash, float oldValue, float newValue)
        {
            if (hash == AttributeHashes.Health && newValue <= 0f)
            {
                Die();
            }
        }

        public void Tick(float deltaTime) { /* Empty: We rely on events, not polling! */ }

        private void Die()
        {
            Debug.Log($"[{_owner.name}] Health reached zero. Entity destroyed.");
            Object.Destroy(_owner);
        }

        public void Dispose()
        {
            // Always unsubscribe to prevent memory leaks
            if (_context != null)
            {
                _context.OnAttributeChanged -= HandleAttributeChanged;
            }
        }
    }

}