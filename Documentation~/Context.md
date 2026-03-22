# Context Document

## 1. Features

Context has the following characteristics:

- Runtime mutable state
- Shared data between Processors

Context values must be freely changeable during runtime.  
Therefore, use `{ get; set; }` properties or members so that Processors can read and write them.

Context requires the `[Serializable]` attribute to be displayed in the Inspector.  
It must also implement `IContext` or inherit from `IContext`.

---

## 1.1 Design Intent

Context is designed to clearly separate **"state data that changes during runtime"**.

Intermediate state values generated while a Processor executes logic — such as current speed, direction, and flags — cannot be stored in Setting, because Setting is immutable at runtime.

Context serves this role.  
By sharing the same Context instance across multiple Processors, they can collaborate without directly passing data to each other.

| Principle | Description |
|-----------|-------------|
| Runtime Mutable | Processors must be able to freely read and write values |
| Shared Between Processors | Multiple Processors reference the same Context instance |
| Inspector Visibility | Current state must be inspectable in the Inspector for debugging purposes |

The reason `{ get; set; }` is used is precisely this.  
Unlike Setting's `private set`, Context must allow external (Processor) code to freely modify its values.

---

## 1.2 Differences from Setting

| Item | Setting | Context |
|------|---------|---------|
| Purpose | Configuration data (max speed, cooldown, etc.) | Runtime state (current speed, direction, flags, etc.) |
| Mutability | Runtime immutable (`private set`) | Runtime mutable (`set`) |
| Modified By | Inspector (editor) | Processor (code) |
| Example | `MaxSpeed`, `JumpForce` | `CurrentSpeed`, `IsMoving` |

> Simple rule: **If the value changes during gameplay → Context. If a designer adjusts it → Setting.**

---

## 1.3 Example Code

### Basic Example

```csharp
[Serializable]
public class MoveContext : IContext
{
    [field: Header("Target")]
    [field: SerializeField] public float Speed { get; set; }
    [field: SerializeField] public float Acceleration { get; set; }
    [field: SerializeField] public float Deceleration { get; set; }

    [field: Header("State")]
    [field: SerializeField] public bool IsMoving { get; set; }
    [field: SerializeField] public float MoveDirection { get; set; }
    [field: SerializeField] public float LinearVelocityX { get; set; }
}
```

---

### Jump Context Example

```csharp
[Serializable]
public class JumpContext : IContext
{
    [field: Header("State")]
    [field: SerializeField] public bool IsGrounded { get; set; }
    [field: SerializeField] public bool IsJumping { get; set; }
    [field: SerializeField] public int RemainingJumpCount { get; set; }

    [field: Header("Velocity")]
    [field: SerializeField] public float VerticalVelocity { get; set; }
}
```

---

### Combat Context Example

```csharp
[Serializable]
public class CombatContext : IContext
{
    [field: Header("Attack")]
    [field: SerializeField] public bool IsAttacking { get; set; }
    [field: SerializeField] public float LastAttackTime { get; set; }

    [field: Header("Health")]
    [field: SerializeField] public float CurrentHp { get; set; }
    [field: SerializeField] public bool IsDead { get; set; }
}
```

---

### Using Context in a Processor

Context is used by Processors through direct read and write access.

```csharp
[Serializable]
[RequireSetting(typeof(MoveSetting))]
[RequireSetting(typeof(Rigidbody2DSetting))]
[RequireContext(typeof(MoveContext))]
[RequireContext(typeof(GroundContext))]
public class MoveProcessor : MovementProcessor
{
    public override InvokeTiming InvokeTiming => InvokeTiming.FixedUpdate;

    private MoveSetting setting;
    private Rigidbody2DSetting rigidBody2DSetting;

    private MoveContext context;
    private GroundContext groundContext;

    public override void Initialize(IReadOnlyRegistry<IMovementSetting> settingRegistry, IReadOnlyRegistry<IMovementContext> contextRegistry)
    {
        setting = settingRegistry.Get<MoveSetting>();
        rigidBody2DSetting = settingRegistry.Get<Rigidbody2DSetting>();

        context = contextRegistry.Get<MoveContext>();
        groundContext = contextRegistry.Get<GroundContext>();
    }

    public override void Process()
    {
        bool grounded = groundContext.IsGrounded;

        context.Speed = grounded ? setting.MoveSpeedOnGround : setting.MoveSpeedOffGround;
        context.Acceleration = grounded ? setting.AccelerationOnGround : setting.AccelerationOffGround;
        context.Deceleration = grounded ? setting.DecelerationOnGround : setting.DecelerationOffGround;

        context.MoveDirection = Input.GetAxisRaw("Horizontal");

        var rigidBody = rigidBody2DSetting.Rigidbody2D;

        if (context.MoveDirection != 0)
        {
            rigidBody.AddForceX(context.MoveDirection * context.Acceleration);
        }
        else
        {
            rigidBody.linearVelocityX = Mathf.MoveTowards(
                rigidBody.linearVelocityX,
                0,
                context.Deceleration * Time.fixedDeltaTime
            );
        }

        rigidBody.linearVelocityX = Mathf.Clamp(
            rigidBody.linearVelocityX,
            -context.Speed,
            context.Speed
        );

        context.IsMoving = Mathf.Abs(rigidBody.linearVelocityX) > 0.01f;
        context.LinearVelocityX = rigidBody.linearVelocityX;
    }
}
```

---

## 1.4 Common Mistakes / Cautions

### Storing Runtime State in Setting

```csharp
// Wrong - current speed changes at runtime and should not be in Setting
[Serializable]
public class MovementSetting : ISetting
{
    [field: SerializeField] public float MaxSpeed { get; private set; }
    [field: SerializeField] public float CurrentSpeed { get; private set; } // X
}

// Correct - runtime state belongs in Context
[Serializable]
public class MoveContext : IContext
{
    [field: SerializeField] public float CurrentSpeed { get; set; } // O
}
```

---

### Using `private set`

Context values must be writable by Processors.  
Using `private set` prevents Processors from writing values, causing runtime errors.

```csharp
// Wrong - private set prevents external writes
[field: SerializeField] public float Speed { get; private set; }

// Correct
[field: SerializeField] public float Speed { get; set; }
```

---

### Missing `[Serializable]`

```csharp
// Wrong - will not appear in Inspector, making debugging difficult
public class MoveContext : IContext
{
    [field: SerializeField] public float Speed { get; set; }
}

// Correct
[Serializable]
public class MoveContext : IContext
{
    [field: SerializeField] public float Speed { get; set; }
}
```

---

### Using Context for Initial Value Configuration Like Setting

Inspector exposure in Context is for **debugging purposes only**.  
If initial values are needed, manage them in Setting and apply them to Context during the Processor's initialization phase.

```csharp
public class MoveProcessor : IProcessor
{
    public void Init(MoveContext context, MovementSetting setting)
    {
        // Apply initial values from Setting to Context
        context.Speed = setting.InitialSpeed;
    }
}
```