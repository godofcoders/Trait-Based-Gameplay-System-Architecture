# Contributing

Keep the project small enough to learn from and explicit enough to reuse.

- Core and Traits must compile without Unity engine references. Keep authoring assets in Data and scene behavior in Unity or Demo.
- Pass numeric settings into runtime constructors. Configuration factories must create a fresh trait per entity.
- Define required attributes, bounds, dependencies, tick order, and cleanup ownership. Avoid hidden initialization order or service lookups.
- Validate configuration at runtime as well as through Inspector hints. Test finite/invalid numbers and relevant boundary values.
- Add domain tests for behavior and lifecycle changes. Add PlayMode coverage when changing scene composition, Unity lifetime, or adapters.
- Run both Unity suites before merging. The standalone domain runner is useful for fast feedback, but does not validate Unity native behavior.
- Update the walkthrough and demo when public APIs or serialized fields change. Include migration instructions for custom scenes/configurations.
- Support performance claims with a reproducible workload and measurements. Avoid claims such as AAA-ready, zero allocations, or arbitrary entity counts without evidence.

Use Unity 6000.0.66f2 for reproducible project imports. Commit new asset `.meta` files. Exclude Library, Temp, generated solutions, and local test results from changes. Describe any validation you could not run and its exact blocker.
