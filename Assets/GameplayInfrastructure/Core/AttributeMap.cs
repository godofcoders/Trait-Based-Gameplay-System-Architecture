using System;
using System.Collections.Generic;

namespace GameplayInfrastructure.Core
{
    public class AttributeMap : IAttributeContext
    {
        private readonly Dictionary<int, float> _attributes = new Dictionary<int, float>(32); // Pre-allocate capacity
        public event Action<int, float, float> OnAttributeChanged;

        public void SetValue(int attributeHash, float value)
        {
            float oldValue = GetValue(attributeHash);

            // Prevent redundant updates and event spam
            if (UnityEngine.Mathf.Approximately(oldValue, value)) return;

            _attributes[attributeHash] = value;
            OnAttributeChanged?.Invoke(attributeHash, oldValue, value);
        }

        public float GetValue(int attributeHash, float defaultValue = 0f)
        {
            return _attributes.TryGetValue(attributeHash, out float val) ? val : defaultValue;
        }

        public bool HasAttribute(int attributeHash) => _attributes.ContainsKey(attributeHash);
    }
}