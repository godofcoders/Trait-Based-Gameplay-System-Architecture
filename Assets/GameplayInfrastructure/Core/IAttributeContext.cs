using System;

namespace GameplayInfrastructure.Core
{
    public interface IAttributeContext
    {
        // Event fired when any attribute changes: HashKey, OldValue, NewValue
        event Action<int, float, float> OnAttributeChanged;

        void SetValue(int attributeHash, float value);
        float GetValue(int attributeHash, float defaultValue = 0f);
        bool HasAttribute(int attributeHash);
    }
}