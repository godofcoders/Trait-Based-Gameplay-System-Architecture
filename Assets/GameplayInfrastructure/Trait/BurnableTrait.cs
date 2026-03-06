
using GameplayInfrastructure.Core;
using GameplayInfrastructure.Data;

namespace GameplayInfrastructure.Traits
{
    public class BurnableTrait : ITrait
    {
        private IAttributeContext _context;
        private readonly BurnConfig _config;

        public BurnableTrait(BurnConfig config)
        {
            _config = config;
        }

        public void Initialize(IAttributeContext context)
        {
            _context = context;
        }

        public void Tick(float deltaTime)
        {
            // We only process logic if the entity is actually burning
            if (_context.GetValue(AttributeHashes.IsBurning) > 0f)
            {
                float currentHealth = _context.GetValue(AttributeHashes.Health);
                if (currentHealth > _config.ExtinguishThreshold)
                {
                    _context.SetValue(AttributeHashes.Health, currentHealth - (_config.DamagePerSecond * deltaTime));
                }
            }
        }

        public void Dispose() { }
    }
}