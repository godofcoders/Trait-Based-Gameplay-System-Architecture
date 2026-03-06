using System.Collections.Generic;
using UnityEngine;
using GameplayInfrastructure.Core;
using GameplayInfrastructure.Traits;
using GameplayInfrastructure.Data;

namespace GameplayInfrastructure.Unity
{
    public class EntityController : MonoBehaviour
    {
        [SerializeField] private BurnConfig _burnConfig;
        [SerializeField] private float _startingHealth = 100f;

        private readonly AttributeMap _attributeMap = new AttributeMap();
        private readonly List<ITrait> _traits = new List<ITrait>();

        private void Start()
        {
            // Initialize traits via Dependency Injection
            AddTrait(new HealthTrait(gameObject, _startingHealth));

            if (_burnConfig != null)
            {
                AddTrait(new BurnableTrait(_burnConfig));
            }
        }

        private void AddTrait(ITrait trait)
        {
            trait.Initialize(_attributeMap);
            _traits.Add(trait);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            // Iterate backwards if traits might remove themselves during execution
            for (int i = _traits.Count - 1; i >= 0; i--)
            {
                _traits[i].Tick(dt);
            }
        }

        private void OnDestroy()
        {
            foreach (var trait in _traits)
            {
                trait.Dispose();
            }
            _traits.Clear();
        }

        // Exposing the context for external systems (like UI or Weapons) to interact with
        public IAttributeContext GetContext() => _attributeMap;
    }
}