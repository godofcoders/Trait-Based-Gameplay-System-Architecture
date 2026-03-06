# 🧩 Unity Trait-Based Gameplay Architecture

A highly scalable, AAA-standard gameplay infrastructure for Unity. This project demonstrates how to move away from rigid, monolithic `MonoBehaviour` inheritance and instead build a flexible, composition-based entity system using Pure C# and Clean Architecture principles.

![Architecture Diagram](docs/architecture_diagram.png)  
*(Note: Replace this link with a screenshot or diagram of your system)*

## 🚀 Overview
Traditional Unity development often leads to the "God Class" problem, where an entity (like a Player or Enemy) becomes bloated with hardcoded logic (Health, Movement, Combat). 

This architecture solves that by treating an Entity merely as a container for **Traits** (e.g., `HealthTrait`, `BurnableTrait`). Traits communicate indirectly through a centralized **Attribute Blackboard**, resulting in a completely decoupled, event-driven system.

## 🧠 Design Patterns & Principles Used

To ensure high performance and maintainability, this project strictly adheres to several core software engineering patterns:

* **Clean Architecture (Separation of Concerns):** The core gameplay logic (`/Traits` and `/Core`) is written in Pure C# and has absolutely zero dependency on Unity's `MonoBehaviour` lifecycle. The `EntityController` acts solely as an Adapter to bridge the Unity Engine with the pure domain logic.
* **The Observer Pattern (Event-Driven):** Instead of traits polling `Update()` every frame to check if health is zero, they subscribe to an `OnAttributeChanged` event. This eliminates unnecessary CPU overhead; logic only runs when data actually changes.
* **Dependency Injection (DI):** Traits do not use `GetComponent<T>()` or search for their dependencies. Instead, the `IAttributeContext` is injected into them during initialization, making the traits highly modular and extremely easy to Unit Test outside of a Unity scene.
* **Data-Driven Design:** Mechanic configurations (like Burn Damage or Movement Speed) are decoupled from the logic using `ScriptableObjects`. This allows game designers to balance the game without touching C# scripts.

## 📂 Project Structure (Assembly Definitions)

The project uses `.asmdef` files to enforce strict dependency boundaries:

* `GameplayInfrastructure.Core` - Contains the foundational interfaces (`ITrait`, `IAttributeContext`) and zero-allocation Hash utilities.
* `GameplayInfrastructure.Data` - Holds the `ScriptableObject` configurations.
* `GameplayInfrastructure.Traits` - Pure C# domain logic. References `Core` and `Data`, but strictly **cannot** reference `Unity`.
* `GameplayInfrastructure.Unity` - The Composition Root. Contains MonoBehaviours like `EntityController` that assemble the system.

## ⚡ Performance Considerations
* **Zero String Allocations:** The Blackboard system uses integer hashes (`AttributeHashes.cs`) rather than strings for dictionary lookups, completely preventing Garbage Collection (GC) spikes during gameplay.
* **Pre-allocated Memory:** Data structures are initialized with predicted capacities to avoid array resizing at runtime.

## 🛠️ How to Use

1.  Create an empty GameObject in your scene.
2.  Attach the `EntityController` MonoBehaviour.
3.  Inject a configuration `ScriptableObject` (e.g., `BurnConfig`) via the Inspector.
4.  The `EntityController` will automatically initialize the pure C# `HealthTrait` and `BurnableTrait` at startup.
5.  Modify the Entity's blackboard values (e.g., setting `IsBurning` to 1) from an external script to see the traits react dynamically!
