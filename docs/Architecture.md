# Architecture walkthrough

## Start with ownership

`EntityController` is the composition root for Unity. In `Awake`, it asks each configured asset for a fresh trait, creates a `GameplayEntity`, and subscribes to its death event. `GetContext()` can also initialize on demand, so another component's `Awake` does not depend on which object initializes first.

`GameplayEntity` owns its supplied trait instances after validating starting health. It owns a private `AttributeMap` and an always-present `HealthTrait`. Additional traits cannot replace the built-in health trait or duplicate another concrete type. There is no service locator and no `GetComponent` inside domain code.

The runtime initializes health before optional traits. This means `BurnableTrait` can require Health and MaxHealth during initialization. A missing dependency or invalid threshold is an explicit failure rather than silently reading a default zero. Optional traits that depend on each other must appear in dependency order; there is no automatic dependency graph.

A trait derives from `Trait` to share single-use initialization and idempotent cleanup. Only a trait implementing `ITickableTrait` receives time updates. Event-only behaviors pay no empty per-frame call.

## Follow one burning tick

1. The input adapter sets `IsBurning` to 1. That ID only exists if the entity has a burn trait.
2. `EntityController.Update` supplies `Time.deltaTime` to the domain runtime.
3. The burn trait reads health and subtracts damage per second multiplied by elapsed time. Double intermediates avoid multiplication overflow from otherwise finite inputs.
4. Its own damage clamps at the extinguish threshold; the attribute map also enforces the health range.
5. The map stores the new value before invoking `OnAttributeChanged`.
6. Health reacts to a value of zero by setting its terminal death flag and raising `Died` once. The Unity adapter disables updates and schedules GameObject destruction.
7. Burning clears `IsBurning` if health has reached its threshold. It also responds to external damage or attempted ignition below the threshold.
8. The runtime stops ticking later traits after death. Unity's `OnDestroy` unsubscribes and disposes the entity.

The scene's default threshold is zero. A positive threshold models a fire that burns out without killing; unrelated damage can still take health below that threshold.

## Authoring data versus runtime state

`BurnConfig` is a shared ScriptableObject. Its factory copies numeric values into `BurnableTrait`'s readonly fields. Two barrels may reference one asset but have separate health, burning flags, trait instances, and event subscriptions. Editing the asset while running affects newly created entities, not existing snapshots.

This reverses the original dependency from runtime logic to authoring assets:

```text
Core ← Traits ← Data
  ↑       ↑       ↑
  └────── Unity ──┘
```

Core and Traits set `noEngineReferences: true`. Moving `Destroy`, `Debug.Log`, and Unity hashing out of those assemblies is enforced at compilation, rather than just described in documentation.

## Attribute schema

The blackboard is intentionally small and numeric:

| ID | Owner | Initial value | Bounds |
| --- | --- | --- | --- |
| Health | HealthTrait | Starting health | 0 to starting health |
| MaxHealth | HealthTrait | Starting health | Starting health to starting health |
| IsBurning | BurnableTrait | 0 | 0 to 1; any positive value means active |

Registration is silent, including for zero. Unknown reads return the caller's default (zero if omitted); required dependencies must therefore use `HasAttribute`. Unknown writes throw. Definitions are fixed for the entity's lifetime.

IDs are assigned centrally. This avoids hash collisions but requires discipline when adding new attributes. A larger system could introduce typed keys, generated schemas, writable command interfaces, or access control. Here, `IAttributeContext` is a trusted gameplay interface, not an authority boundary for client/network input.

Exact float equality suppresses redundant writes without dropping representable damage increments through an approximate-equality check. It does not solve all floating-point issues or provide cross-platform deterministic lockstep.

## Event semantics and cleanup

Events run synchronously on the simulation thread. An observer can cause another attribute change immediately. Remaining observers may therefore see an earlier event payload after a nested notification. Do not assume globally queued ordering; inspect current context state when reacting to multiple attributes.

An observer throwing follows normal C# semantics: later observers may not execute. Gameplay observers must not throw. Fault isolation or queued dispatch would need a different explicit contract and tests.

Unsubscription does not remove a callback from a delegate invocation already in progress. Built-in traits check their disposed state before responding. This allows a death listener to synchronously dispose the entity without a later callback using disposed context. The domain tests exercise this case.

The runtime disposes traits in reverse composition order, attempts all cleanup, and aggregates failures. Initialization failure also releases owned traits. Custom trait cleanup must tolerate partial initialization; factories should return fresh managed objects without acquiring scene resources. Unity resource ownership belongs in the adapter.

## Deliberate limits

This reference keeps the mechanism visible. It has no automatic dependency discovery, runtime trait mutation, variable maximum health, resurrection, save format, replication, rollback, pooling reset, active-entity scheduler, or worker-thread support. A disabled Unity component retains its state and observers until destruction.

Each entity still receives Unity updates, and each time-based trait is visited while alive. Burning reads its flag every tick. Health is event driven; the entire simulation is not. Dictionary growth, events, startup allocations, demo GUI, and observers can allocate. Claims about entity counts, frame budgets, or GC require measurements in a representative player build.

Before adapting this to a shipped game, establish the requirements that affect these choices. For example: an authoritative multiplayer game needs validated commands and replication rules; pooled enemies need an explicit reset/new-lifetime flow; thousands of entities may benefit from a central scheduler or a different data layout. These features should be added with their own tests and profiling evidence.
