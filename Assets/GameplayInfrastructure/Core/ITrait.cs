using System;

namespace GameplayInfrastructure.Core
{
    public interface ITrait : IDisposable
    {
        void Initialize(IAttributeContext context);
        void Tick(float deltaTime);
    }
}