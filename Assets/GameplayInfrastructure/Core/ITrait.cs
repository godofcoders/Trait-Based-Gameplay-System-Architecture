using System;

namespace GameplayInfrastructure.Core
{
    public interface ITrait : IDisposable
    {
        void Initialize(IAttributeContext context);
    }

    // Only time-based behaviors participate in the update loop.
    public interface ITickableTrait : ITrait
    {
        void Tick(float deltaTime);
    }
}
