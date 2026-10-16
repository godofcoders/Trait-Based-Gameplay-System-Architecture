using System.Collections;
using GameplayInfrastructure.Core;
using GameplayInfrastructure.Unity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GameplayInfrastructure.Tests
{
    public class DemoSceneTests
    {
        [UnityTest]
        public IEnumerator DemoSceneInitializesAndDestroysBarrelAfterLethalDamage()
        {
            yield return SceneManager.LoadSceneAsync("Demo", LoadSceneMode.Single);
            var barrel = GameObject.Find("Barrel");
            Assert.That(barrel, Is.Not.Null);
            var entity = barrel.GetComponent<EntityController>();
            var context = entity.GetContext();
            Assert.That(context.GetValue(AttributeHashes.Health), Is.EqualTo(100f));
            Assert.That(context.HasAttribute(AttributeHashes.IsBurning), Is.True);
            context.SetValue(AttributeHashes.IsBurning, 1f);
            yield return null;
            yield return null;
            Assert.That(context.GetValue(AttributeHashes.Health), Is.LessThan(100f));
            context.SetValue(AttributeHashes.Health, -100f);
            yield return null;
            Assert.That(barrel == null, Is.True, "Unity adapter must destroy the dead entity.");
            // Retained contexts should not invoke disposed traits after OnDestroy.
            Assert.DoesNotThrow(() => context.SetValue(AttributeHashes.Health, 100f));
        }
    }
}
