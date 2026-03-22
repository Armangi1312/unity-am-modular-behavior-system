# Setting Document

## 1. Features

Setting has the following characteristics:

- Runtime quasi-immutability
- Data and configuration values

Setting values must not change at runtime.  
Therefore, use `[field: SerializeField] public ~ { get; private set; }` to allow editing in the Inspector  
while enforcing immutability in runtime code.

Setting requires the `[Serializable]` attribute to be displayed in the Inspector.  
It must also implement an interface that implements `ISetting`, or inherit from `ISetting`.

---

## 1.1 Design Intent

Setting is designed to clearly separate **"read-only configuration data"**.

In traditional MonoBehaviour-based designs, data and logic are often mixed together in a single class.  
Setting addresses this problem by following these principles:

| Principle | Description |
|-----------|-------------|
| Inspector Editable | Designers and planners must be able to adjust values directly in the Unity Inspector |
| Runtime Immutable | Once the game is running, values must not be changeable through code |
| Separated from Processor | Logic (Processor) depends on data (Setting), but not the other way around |

This is precisely why `private set` is used.  
`SerializeField` allows Inspector serialization, while write access from external code is blocked at compile time.

---

## 1.2 Example Code

### Basic Example

```csharp
[Serializable]
public class TestSetting : ISetting
{
    [field: SerializeField] public int TestNumber1 { get; private set; }
    [field: SerializeField] public int TestNumber2 { get; private set; }
}
```

---

### Using Attributes

Some Unity attributes such as `[Header()]` and `[Space]` can be used via the `field:` target.

```csharp
[Serializable]
public class MovementSetting : ISetting
{
    [field: Header("Speed")]
    [field: SerializeField] public float MoveSpeed { get; private set; }
    [field: SerializeField] public float SprintSpeed { get; private set; }

    [field: Space]
    [field: Header("Jump")]
    [field: SerializeField] public float JumpForce { get; private set; }
    [field: SerializeField] public int MaxJumpCount { get; private set; }
}
```

---

### Nested Setting Example

A Setting can contain other Settings.  
This is useful for grouping related data together to clarify structure.

```csharp
[Serializable]
public class AttackSetting : ISetting
{
    [field: SerializeField] public float Damage { get; private set; }
    [field: SerializeField] public float AttackRange { get; private set; }
    [field: SerializeField] public float AttackCooldown { get; private set; }
}

[Serializable]
public class CharacterSetting : ISetting
{
    [field: SerializeField] public MovementSetting Movement { get; private set; }
    [field: SerializeField] public AttackSetting Attack { get; private set; }
}
```

---

## 1.3 Common Mistakes / Cautions

### Confusing `[SerializeField]` with `[field: SerializeField]`

```csharp
// Wrong - applying [SerializeField] directly to a property does not serialize it
[SerializeField] public int Speed { get; private set; }

// Correct - the field: target must be specified for the backing field to be serialized
[field: SerializeField] public int Speed { get; private set; }
```

---

### Missing `[Serializable]`

```csharp
// Wrong - will not appear in the Inspector
public class MovementSetting : ISetting
{
    [field: SerializeField] public float MoveSpeed { get; private set; }
}

// Correct
[Serializable]
public class MovementSetting : ISetting
{
    [field: SerializeField] public float MoveSpeed { get; private set; }
}
```

---

### Attempting to Modify Setting Values at Runtime

Setting is designed to be immutable at runtime.  
Modifying a Setting value inside a Processor violates the design principle.

```csharp
// Compile error - private set prevents external writes
movementSetting.MoveSpeed = 10f;

// Correct approach - mutable data should be stored in Context
context.CurrentSpeed = 10f;
```

> Values that need to change at runtime should be stored in **Context** — that is the correct design.

---

### Unsupported Attribute Types

Attributes that do not support the `field:` target will not work when applied to properties.

| Attribute | `field:` Support |
|-----------|-----------------|
| `[SerializeField]` | O |
| `[Header]` | O |
| `[Space]` | O |
| `[Range]` | O |
| `[Tooltip]` | O |
| `[HideInInspector]` | O |
| `[NonSerialized]` | X |
| `[Min]`, `[Max]` | X |

---

### Unsupported Serializable Types

Types that do not support `field:` serialization will not function correctly when used with properties.

| Type | `field:` Support |
|------|-----------------|
| `int`, `long`, `float`... | O |
| `Vector2 / 3 / 4` | O |
| `Color` | O |
| `Enum` | O |
| `AnimationCurve` | O |
| `LayerMask` | O |
| `Gradient` | O |
| `UnityEngine.Object` | O |
| `List`, `Array` | O |
| `Dictionary` | X |