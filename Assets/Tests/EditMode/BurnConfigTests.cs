using GameplayInfrastructure.Core;
using GameplayInfrastructure.Data;
using GameplayInfrastructure.Traits;
using NUnit.Framework;
using UnityEngine;

namespace GameplayInfrastructure.Tests
{
    public class BurnConfigTests
    {
        [Test]
        public void SharedConfigCreatesIndependentRuntimeSnapshots()
        {
            var config = ScriptableObject.CreateInstance<BurnConfig>();
            try
            {
                config.DamagePerSecond = 20f;
                using (var first = new GameplayEntity(100f, config.CreateTrait()))
                using (var second = new GameplayEntity(100f, config.CreateTrait()))
                {
                    config.DamagePerSecond = 99f;
                    first.Attributes.SetValue(AttributeHashes.IsBurning, 1f);
                    first.Tick(1f);
                    second.Tick(1f);
                    Assert.That(first.Attributes.GetValue(AttributeHashes.Health), Is.EqualTo(80f));
                    Assert.That(second.Attributes.GetValue(AttributeHashes.Health), Is.EqualTo(100f));
                }
            }
            finally { Object.DestroyImmediate(config); }
        }
    }
}
