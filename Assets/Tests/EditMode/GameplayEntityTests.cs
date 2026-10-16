using System;
using System.Collections.Generic;
using GameplayInfrastructure.Core;
using GameplayInfrastructure.Traits;
using NUnit.Framework;

namespace GameplayInfrastructure.Tests
{
    public class GameplayEntityTests
    {
        [Test]
        public void BurningUsesDeltaTimeAndLeavesHealthyIdleEntitiesUntouched()
        {
            using (var entity = new GameplayEntity(100f, new BurnableTrait(20f)))
            {
                entity.Tick(1f);
                Assert.That(entity.Attributes.GetValue(AttributeHashes.Health), Is.EqualTo(100f));
                entity.Attributes.SetValue(AttributeHashes.IsBurning, 1f);
                entity.Tick(0.25f);
                Assert.That(entity.Attributes.GetValue(AttributeHashes.Health), Is.EqualTo(95f));
            }
        }

        [Test]
        public void DifferentTimeStepsProduceEquivalentDamage()
        {
            using (var first = new GameplayEntity(100f, new BurnableTrait(20f)))
            using (var second = new GameplayEntity(100f, new BurnableTrait(20f)))
            {
                first.Attributes.SetValue(AttributeHashes.IsBurning, 1f);
                second.Attributes.SetValue(AttributeHashes.IsBurning, 1f);
                first.Tick(1f);
                for (int i = 0; i < 10; i++) second.Tick(0.1f);
                Assert.That(first.Attributes.GetValue(AttributeHashes.Health),
                    Is.EqualTo(second.Attributes.GetValue(AttributeHashes.Health)).Within(0.0001f));
            }
        }

        [Test]
        public void LargeTickStopsExactlyAtThresholdAndExtinguishes()
        {
            using (var entity = new GameplayEntity(100f, new BurnableTrait(20f, 25f)))
            {
                entity.Attributes.SetValue(AttributeHashes.IsBurning, 1f);
                entity.Tick(100f);
                Assert.That(entity.Attributes.GetValue(AttributeHashes.Health), Is.EqualTo(25f));
                Assert.That(entity.Attributes.GetValue(AttributeHashes.IsBurning), Is.Zero);
                entity.Attributes.SetValue(AttributeHashes.IsBurning, 1f);
                Assert.That(entity.Attributes.GetValue(AttributeHashes.IsBurning), Is.Zero);
                Assert.That(entity.IsDead, Is.False);
            }
        }

        [Test]
        public void ExternalDamageAlsoExtinguishesFire()
        {
            using (var entity = new GameplayEntity(100f, new BurnableTrait(20f, 25f)))
            {
                entity.Attributes.SetValue(AttributeHashes.IsBurning, 1f);
                entity.Attributes.SetValue(AttributeHashes.Health, 10f);
                Assert.That(entity.Attributes.GetValue(AttributeHashes.IsBurning), Is.Zero);
            }
        }

        [Test]
        public void DeathOccursOnceEvenWithReentrantHealthWrites()
        {
            using (var entity = new GameplayEntity(100f, new BurnableTrait(20f)))
            {
                int deaths = 0;
                entity.Died += () =>
                {
                    deaths++;
                    entity.Attributes.SetValue(AttributeHashes.Health, 10f);
                    entity.Attributes.SetValue(AttributeHashes.Health, 0f);
                };
                entity.Attributes.SetValue(AttributeHashes.IsBurning, 1f);
                entity.Tick(10f);
                entity.Tick(10f);
                Assert.That(deaths, Is.EqualTo(1));
                Assert.That(entity.IsDead, Is.True);
                Assert.That(entity.Attributes.GetValue(AttributeHashes.Health), Is.Zero);
                Assert.That(entity.Attributes.GetValue(AttributeHashes.IsBurning), Is.Zero);
            }
        }

        [Test]
        public void DeathSubscriberCanDisposeDuringAnAttributeNotification()
        {
            var entity = new GameplayEntity(100f, new BurnableTrait(20f));
            entity.Died += entity.Dispose;
            entity.Attributes.SetValue(AttributeHashes.IsBurning, 1f);
            Assert.DoesNotThrow(() => entity.Tick(10f));
            Assert.Throws<ObjectDisposedException>(() => entity.Tick(1f));
            Assert.DoesNotThrow(entity.Dispose);
        }

        [Test]
        public void FiniteExtremeInputsDoNotOverflowHealth()
        {
            using (var entity = new GameplayEntity(100f, new BurnableTrait(float.MaxValue)))
            {
                entity.Attributes.SetValue(AttributeHashes.IsBurning, 1f);
                entity.Tick(float.MaxValue);
                Assert.That(entity.Attributes.GetValue(AttributeHashes.Health), Is.Zero);
            }
        }

        [Test]
        public void InvalidConfigurationAndTimeStepsFailExplicitly()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GameplayEntity(0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BurnableTrait(-1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BurnableTrait(float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BurnableTrait(1f, -1f));
            Assert.Throws<InvalidOperationException>(() => new GameplayEntity(10f, new BurnableTrait(1f, 11f)));
            using (var entity = new GameplayEntity(10f))
            {
                foreach (var dt in new[] { -1f, float.NaN, float.PositiveInfinity })
                    Assert.Throws<ArgumentOutOfRangeException>(() => entity.Tick(dt));
            }
        }

        [Test]
        public void DuplicateTraitsAndUninitializedUseAreRejected()
        {
            Assert.Throws<ArgumentException>(() => new GameplayEntity(10f, new BurnableTrait(1f), new BurnableTrait(2f)));
            var burn = new BurnableTrait(1f);
            Assert.Throws<InvalidOperationException>(() => burn.Tick(1f));
            using (var entity = new GameplayEntity(10f, burn))
                Assert.Throws<InvalidOperationException>(() => burn.Initialize(entity.Attributes));
            Assert.Throws<ObjectDisposedException>(() => burn.Tick(1f));
        }

        [Test]
        public void DisposalRemovesAttributeSubscriptions()
        {
            var entity = new GameplayEntity(10f, new BurnableTrait(1f));
            entity.Dispose();
            // A retained context must no longer trigger either health or burn listeners.
            entity.Attributes.SetValue(AttributeHashes.IsBurning, 1f);
            entity.Attributes.SetValue(AttributeHashes.Health, 0f);
            Assert.That(entity.IsDead, Is.False);
            Assert.That(entity.Attributes.GetValue(AttributeHashes.IsBurning), Is.EqualTo(1f));
        }

        private class Probe : Trait, ITickableTrait
        {
            private readonly List<string> _events;
            private readonly bool _fail;
            private readonly Action _tick;
            private readonly string _label;
            private readonly bool _failDispose;
            public Probe(List<string> events, bool fail = false, Action tick = null,
                string label = "", bool failDispose = false)
            { _events = events; _fail = fail; _tick = tick; _label = label; _failDispose = failDispose; }
            protected override void OnInitialize()
            {
                _events.Add(_label + "initialize");
                if (_fail) throw new InvalidOperationException("Expected test failure");
            }
            public void Tick(float dt) { _events.Add(_label + "tick"); _tick?.Invoke(); }
            protected override void OnDispose(IAttributeContext context)
            {
                _events.Add(_label + "dispose");
                if (_failDispose) throw new InvalidOperationException("Expected cleanup failure");
            }
        }

        private sealed class SecondProbe : Probe
        {
            public SecondProbe(List<string> events, bool fail = false, bool failDispose = false)
                : base(events, fail: fail, label: "second-", failDispose: failDispose) { }
        }

        [Test]
        public void TraitsTickInDeclarationOrderAndDisposeInReverseOrder()
        {
            var events = new List<string>();
            using (var entity = new GameplayEntity(10f, new Probe(events), new SecondProbe(events)))
                entity.Tick(1f);
            Assert.That(events, Is.EqualTo(new[] {
                "initialize", "second-initialize", "tick", "second-tick", "second-dispose", "dispose" }));
        }

        [Test]
        public void InitializationFailureUnwindsEarlierTraitsInReverseOrder()
        {
            var events = new List<string>();
            Assert.Throws<InvalidOperationException>(() =>
                new GameplayEntity(10f, new Probe(events), new SecondProbe(events, fail: true)));
            Assert.That(events, Is.EqualTo(new[] { "initialize", "second-initialize", "second-dispose", "dispose" }));
        }

        [Test]
        public void InitializationAndCleanupFailuresAreBothReported()
        {
            var events = new List<string>();
            var error = Assert.Throws<AggregateException>(() => new GameplayEntity(10f,
                new SecondProbe(events, fail: true, failDispose: true)));
            Assert.That(error.InnerExceptions.Count, Is.EqualTo(2));
            Assert.That(error.InnerExceptions[0].Message, Does.Contain("Expected test failure"));
            Assert.That(error.InnerExceptions[1].Message, Does.Contain("Expected cleanup failure"));
        }

        [Test]
        public void CleanupFailureDoesNotPreventOtherTraitsFromDisposing()
        {
            var events = new List<string>();
            var entity = new GameplayEntity(10f, new Probe(events), new SecondProbe(events, failDispose: true));
            Assert.Throws<AggregateException>(entity.Dispose);
            Assert.DoesNotThrow(entity.Dispose);
            Assert.That(events, Is.EqualTo(new[] { "initialize", "second-initialize", "second-dispose", "dispose" }));
        }

        [Test]
        public void LethalDamageStopsLaterTraitsInTheSameTick()
        {
            var events = new List<string>();
            using (var entity = new GameplayEntity(10f, new BurnableTrait(20f), new Probe(events)))
            {
                entity.Attributes.SetValue(AttributeHashes.IsBurning, 1f);
                entity.Tick(1f);
                Assert.That(entity.IsDead, Is.True);
                Assert.That(events, Is.EqualTo(new[] { "initialize" }));
            }
        }

        [Test]
        public void NullCompositionSlotDisposesOtherOwnedInstances()
        {
            var events = new List<string>();
            var probe = new Probe(events);
            Assert.Throws<ArgumentException>(() => new GameplayEntity(10f, null, probe));
            Assert.Throws<InvalidOperationException>(() => probe.Initialize(new AttributeMap()));
        }

        [Test]
        public void MaximumHealthIsFixedAndHealingIsClamped()
        {
            using (var entity = new GameplayEntity(10f))
            {
                entity.Attributes.SetValue(AttributeHashes.MaxHealth, 100f);
                entity.Attributes.SetValue(AttributeHashes.Health, 5f);
                entity.Attributes.SetValue(AttributeHashes.Health, 100f);
                Assert.That(entity.Attributes.GetValue(AttributeHashes.MaxHealth), Is.EqualTo(10f));
                Assert.That(entity.Attributes.GetValue(AttributeHashes.Health), Is.EqualTo(10f));
            }
        }

        [Test]
        public void InitializationFailureDisposesAlreadyInitializedTraits()
        {
            var events = new List<string>();
            Assert.Throws<InvalidOperationException>(() => new GameplayEntity(10f, new Probe(events, true)));
            Assert.That(events, Is.EqualTo(new[] { "initialize", "dispose" }));
        }

        [Test]
        public void RecursiveTicksAreRejectedAndTickGuardRecovers()
        {
            var events = new List<string>();
            GameplayEntity entity = null;
            var probe = new Probe(events, tick: () => entity.Tick(1f));
            using (entity = new GameplayEntity(10f, probe))
            {
                Assert.Throws<InvalidOperationException>(() => entity.Tick(1f));
                Assert.Throws<InvalidOperationException>(() => entity.Tick(1f));
                Assert.That(events, Is.EqualTo(new[] { "initialize", "tick", "tick" }));
            }
        }
    }
}
