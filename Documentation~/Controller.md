# Controller Document

## 1. Features

Controller has the following characteristics:

- Unified management of Setting, Context, and Processor
- Automatic dependency validation and Registry synchronization
- Execution based on Unity lifecycle

Controller is the entry point that ties Setting / Context / Processor together into a single execution unit.  
Instead of writing logic directly, you configure a list of Processors and the Controller automatically handles initialization and execution.

Controller inherits from MonoBehaviour, and behavior can be configured simply by adding Processors in the Inspector.

---

## 1.1 Design Intent

Controller is designed to **"integrate Setting / Context / Processor into a single execution unit"**.

A Processor is an independent unit of logic, but it cannot run on its own.  
The Controller builds the Registry, validates dependencies, initializes each Processor, and executes them every frame.

| Principle | Description |
|-----------|-------------|
| Unified Management | Setting, Context, and Processor are managed within a single component |
| Automatic Dependency Validation | Reads `[RequireSetting]` and `[RequireContext]` to automatically synchronize the Registry |
| Duplicate Prevention | Processors, Settings, and Contexts of the same type are automatically removed |
| Lifecycle Delegation | Initializes in Awake; executes Processors at the appropriate timing in Update / FixedUpdate / LateUpdate |

Directly implementing a Controller is rarely necessary.  
Simply inherit `LifeCycleController` and specify the type parameters.

---

## 1.2 Automatic Dependency Validation

The Controller automatically performs the following at editor time:

### Execution Order

```
1. Collect [RequireSetting] / [RequireContext] from the Processor list
2. Validate that the collected types are compatible with the Controller's TSetting / TContext
3. Automatically instantiate and register any types missing from the Registry
4. Remove null entries and duplicate types from the Registry
5. If duplicate Processors are found, display a warning dialog and remove them
```

### Result

The moment a Processor is added in the Inspector, the Settings and Contexts it requires  
are automatically registered in the Registry. No manual addition is needed.

```
MoveProcessor added
> MoveSetting, Rigidbody2DSetting registered automatically
> MoveContext, GroundContext registered automatically
```

### Compatibility Validation Error Example

```
InvalidOperationException:
Context 'SomeContext' is not compatible with controller context 'IMovementContext'.
```

This error occurs when the Context type required by a Processor does not implement the Controller's `TContext` interface.  
Verify that `IMovementContext` is implemented.

---

## 1.3 Example Code

### Basic Controller Setup

Inherit `LifeCycleController` and specify only the type parameters.  
No logic needs to be written.

```csharp
using AM.Module;

public class MovementController : LifeCycleController<IMovementSetting, IMovementContext, MovementProcessor>
{
}
```

---

### When Custom Initialization Is Needed

If additional work is required at initialization time, override `Initialize()`.

```csharp
using AM.Module;

public class MovementController : LifeCycleController<IMovementSetting, IMovementContext, MovementProcessor>
{
    protected override void Initialize()
    {
        base.Initialize();

        // Additional initialization work
        Debug.Log("MovementController initialized.");
    }
}
```

---

### Multi-Domain Character Setup Example

Multiple Controllers can be attached to a single character to separate domains.

```
PlayerCharacter (GameObject)
├── MovementController     ← Movement, jump, ground detection
├── CombatController       ← Attack, hit, death
└── AnimationController    ← Animation synchronization
```

Each Controller has an independent Registry, so they do not interfere with each other.

---

### `IMovementSetting` / `IMovementContext` Interface Example

These are marker interfaces used as type parameters for the Controller.  
They are used solely for type classification with no additional implementation required.

```csharp
public interface IMovementSetting : ISetting { }
public interface IMovementContext : IContext { }
```

Settings and Contexts must implement these interfaces to be registered with the corresponding Controller.

```csharp
[Serializable]
public class MoveSetting : IMovementSetting
{
    [field: SerializeField] public float MoveSpeedOnGround { get; private set; }
    [field: SerializeField] public float MoveSpeedOffGround { get; private set; }
}

[Serializable]
public class MoveContext : IMovementContext
{
    [field: SerializeField] public float Speed { get; set; }
    [field: SerializeField] public bool IsMoving { get; set; }
}
```

---

### Missing `IMovementSetting` / `IMovementContext` Implementation

If a Processor requires a type that is not compatible with the Controller's type parameters (`TSetting`, `TContext`),  
an `InvalidOperationException` will be thrown in the editor.

```csharp
// Wrong - does not implement IMovementContext
[Serializable]
public class SomeContext : IContext { }

// Correct
[Serializable]
public class SomeContext : IMovementContext { }
```

---

### Adding Duplicate Processors of the Same Type

The Controller does not allow multiple Processors of the same type.  
Duplicates are automatically removed and a warning dialog is displayed.

```
Duplicate Processor Removed
Duplicate processors are not allowed:

MoveProcessor
```

---

### Avoid Calling `PerformInvoke()` Directly

`PerformInvoke()` is called internally by the Controller at the appropriate timing.  
Calling it externally may result in Processors executing multiple times per frame.

---

### Dynamically Adding Processors at Runtime

The Controller initializes only once in `Awake()`.  
Dynamically adding a Processor at runtime will not trigger `Initialize()`,  
so that Processor will not function correctly.