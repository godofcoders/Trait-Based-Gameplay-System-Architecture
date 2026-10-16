using System;

namespace GameplayInfrastructure.Core
{
    public interface IAttributeContext
    {
        // Synchronous notification after a value changes: ID, old, new. Registration
        // is silent. Subscribers may write other attributes, but must avoid cycles
        // and exceptions; dispatch follows ordinary C# event semantics.
        event Action<int, float, float> OnAttributeChanged;
        void Register(int attributeId, float initialValue, float minimum, float maximum);
        void SetValue(int attributeId, float value);
        float GetValue(int attributeId, float defaultValue = 0f);
        bool HasAttribute(int attributeId);
    }
}
