using System;
using System.Collections.Generic;

namespace GameplayInfrastructure.Core
{
    public sealed class AttributeMap : IAttributeContext
    {
        private struct Attribute
        {
            public float Value;
            public float Minimum;
            public float Maximum;
        }

        private readonly Dictionary<int, Attribute> _attributes = new Dictionary<int, Attribute>(8);
        public event Action<int, float, float> OnAttributeChanged;

        public void Register(int attributeId, float initialValue, float minimum, float maximum)
        {
            Numeric.RequireFinite(initialValue, nameof(initialValue));
            Numeric.RequireFinite(minimum, nameof(minimum));
            Numeric.RequireFinite(maximum, nameof(maximum));
            if (minimum > maximum || initialValue < minimum || initialValue > maximum)
                throw new ArgumentOutOfRangeException(nameof(initialValue), "Initial value must be within valid bounds.");
            if (_attributes.ContainsKey(attributeId))
                throw new InvalidOperationException($"Attribute {attributeId} is already registered.");
            _attributes.Add(attributeId, new Attribute { Value = initialValue, Minimum = minimum, Maximum = maximum });
        }

        public void SetValue(int attributeId, float value)
        {
            Numeric.RequireFinite(value, nameof(value));
            if (!_attributes.TryGetValue(attributeId, out var attribute))
                throw new KeyNotFoundException($"Register attribute {attributeId} before writing it.");
            value = Math.Max(attribute.Minimum, Math.Min(attribute.Maximum, value));
            float oldValue = attribute.Value;
            // Exact equality preserves small damage-over-time changes.
            if (oldValue == value) return;
            attribute.Value = value;
            _attributes[attributeId] = attribute;
            OnAttributeChanged?.Invoke(attributeId, oldValue, value);
        }

        public float GetValue(int attributeId, float defaultValue = 0f)
        {
            return _attributes.TryGetValue(attributeId, out var attribute) ? attribute.Value : defaultValue;
        }

        public bool HasAttribute(int attributeId) => _attributes.ContainsKey(attributeId);
    }
}
