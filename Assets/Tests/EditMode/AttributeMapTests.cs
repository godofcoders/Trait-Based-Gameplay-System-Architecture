using System;
using System.Collections.Generic;
using GameplayInfrastructure.Core;
using NUnit.Framework;

namespace GameplayInfrastructure.Tests
{
    public class AttributeMapTests
    {
        [Test]
        public void RegisterZeroMakesAttributePresentWithoutNotification()
        {
            var map = new AttributeMap();
            int changes = 0;
            map.OnAttributeChanged += (id, oldValue, newValue) => changes++;
            map.Register(42, 0f, 0f, 1f);
            Assert.That(map.HasAttribute(42), Is.True);
            Assert.That(map.GetValue(42, 99f), Is.Zero);
            Assert.That(changes, Is.Zero);
        }

        [Test]
        public void WritesClampBeforeNotificationAndSuppressExactDuplicates()
        {
            var map = new AttributeMap();
            map.Register(42, 50f, 0f, 100f);
            var changes = new List<float>();
            map.OnAttributeChanged += (id, oldValue, newValue) =>
            {
                Assert.That(id, Is.EqualTo(42));
                Assert.That(map.GetValue(id), Is.EqualTo(newValue));
                changes.Add(oldValue);
                changes.Add(newValue);
            };
            map.SetValue(42, -5f);
            map.SetValue(42, -10f);
            map.SetValue(42, 200f);
            Assert.That(changes, Is.EqualTo(new[] { 50f, 0f, 0f, 100f }));
        }

        [Test]
        public void SmallRepresentableChangesAreNotDiscarded()
        {
            var map = new AttributeMap();
            map.Register(42, 100f, 0f, 100f);
            map.SetValue(42, 99.99999f);
            Assert.That(map.GetValue(42), Is.LessThan(100f));
        }

        [Test]
        public void InvalidWritesDoNotCorruptExistingState()
        {
            var map = new AttributeMap();
            map.Register(42, 5f, 0f, 10f);
            foreach (var value in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
                Assert.Throws<ArgumentOutOfRangeException>(() => map.SetValue(42, value));
            Assert.Throws<KeyNotFoundException>(() => map.SetValue(43, 1f));
            Assert.Throws<InvalidOperationException>(() => map.Register(42, 1f, 0f, 2f));
            Assert.Throws<ArgumentOutOfRangeException>(() => map.Register(43, 0f, 1f, 2f));
            Assert.That(map.GetValue(42), Is.EqualTo(5f));
            Assert.That(map.HasAttribute(43), Is.False);
        }
    }
}
