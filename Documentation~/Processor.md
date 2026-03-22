# Processor Document

## 1. Features

Processor has the following characteristics:

- Responsible for the actual execution logic
- Operates by receiving injected Setting and Context
- Independently replaceable and composable

A Processor is the unit that executes actual game logic based on Setting (configuration data) and Context (runtime state).  
Each Processor has a single responsibility, and multiple Processors are combined to define a character's behavior.

Processors require the `[Serializable]` attribute and must declare their dependencies using the `[RequireSetting]` and `[RequireContext]` attributes.

---

## 1.1 Design Intent

Processor is designed to **"separate logic into independent units"**.

In traditional MonoBehaviour-based designs, all logic — movement, jumping, attacking — tends to be lumped into a single class.  
Processor addresses this problem by following these principles:

| Principle | Description |
|-----------|-------------|
| Single Responsibility | Each Processor handles exactly one behavior |
| Explicit Dependencies | Required data is declared via `[RequireSetting]` and `[RequireContext]` |
| Composable | Multiple Processors are arranged in order in the Inspector to define behavior |
| Separated Execution Timing | `InvokeTiming` selects between Update / FixedUpdate / LateUpdate |

Processors do not own data directly.  
Setting and Context are injected through the Registry, and the Processor focuses solely on executing logic.

---

## 1.2 Attribute Descriptions

### `[RequireSetting(typeof(T))]`

Declares the Setting type this Processor requires.  
The Controller reads this attribute to verify that the corresponding Setting is registered in the Registry.  
If missing, a runtime error or warning will occur.

```csharp
[RequireSetting(typeof(MoveSetting))]
[RequireSetting(typeof(Rigidbody2DSetting))]
public class MoveProcessor : MovementProcessor { }
```

---

### `[RequireContext(typeof(T))]`

Declares the Context type this Processor requires.  
The Controller validates this through the Registry in the same way as Setting.

```csharp
[RequireContext(typeof(MoveContext))]
[RequireContext(typeof(GroundContext))]
public class MoveProcessor : MovementProcessor { }
```

---

### `InvokeTiming`

Specifies the Unity loop timing at which the Processor executes.  
Processors that include physics calculations must use `FixedUpdate`.

```csharp
public override InvokeTiming InvokeTiming => InvokeTiming.FixedUpdate;
```

**Only available on Processors that inherit from `LifeCycleProcessor`.**

---

## 1.3 Processor Structure

Every Processor must implement the following two methods:

| Method | When Called | Role |
|--------|-------------|------|
| `Initialize()` | Once at game start | Cache Setting / Context from the Registry |
| `Process()` | Every frame (based on InvokeTiming) | Execute the actual logic |

In `Initialize()`, retrieve the required Setting and Context from the Registry and cache them in fields.  
In `Process()`, use the cached references to execute logic.

---

## 1.4 Example Code

### Basic Movement Processor

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
    private Rigidbody2DSetting rigidbody2DSetting;

    private MoveContext context;
    private GroundContext groundContext;

    public override void Initialize(
        IReadOnlyRegistry<IMovementSetting> settingRegistry,
        IReadOnlyRegistry<IMovementContext> contextRegistry)
    {
        setting = settingRegistry.Get<MoveSetting>();
        rigidbody2DSetting = settingRegistry.Get<Rigidbody2DSetting>();

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

        var rigidBody = rigidbody2DSetting.Rigidbody2D;

        if (context.MoveDirection != 0)
        {
            rigidBody.AddForceX(context.MoveDirection * context.Acceleration);
        }
        else
        {
            rigidBody.linearVelocityX = Mathf.MoveTowards(
                rigidBody.linearVelocityX,
                0f,
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

### Jump Processor Example

```csharp
[Serializable]
[RequireSetting(typeof(JumpSetting))]
[RequireSetting(typeof(Rigidbody2DSetting))]
[RequireContext(typeof(JumpContext))]
[RequireContext(typeof(GroundContext))]
public class JumpProcessor : MovementProcessor
{
    public override InvokeTiming InvokeTiming => InvokeTiming.Update;

    private JumpSetting setting;
    private Rigidbody2DSetting rigidbody2DSetting;

    private JumpContext context;
    private GroundContext groundContext;

    public override void Initialize(
        IReadOnlyRegistry<IMovementSetting> settingRegistry,
        IReadOnlyRegistry<IMovementContext> contextRegistry)
    {
        setting = settingRegistry.Get<JumpSetting>();
        rigidbody2DSetting = settingRegistry.Get<Rigidbody2DSetting>();

        context = contextRegistry.Get<JumpContext>();
        groundContext = contextRegistry.Get<GroundContext>();
    }

    public override void Process()
    {
        if (groundContext.IsGrounded)
            context.RemainingJumpCount = setting.MaxJumpCount;

        if (Input.GetButtonDown("Jump") && context.RemainingJumpCount > 0)
        {
            var rigidBody = rigidbody2DSetting.Rigidbody2D;
            rigidBody.linearVelocityY = 0f;
            rigidBody.AddForceY(setting.JumpForce, ForceMode2D.Impulse);

            context.RemainingJumpCount--;
            context.IsJumping = true;
        }

        if (groundContext.IsGrounded)
            context.IsJumping = false;
    }
}
```

---

### Animation Processor Example

```csharp
[Serializable]
[RequireContext(typeof(MoveContext))]
[RequireContext(typeof(JumpContext))]
public class AnimationProcessor : MovementProcessor
{
    public override InvokeTiming InvokeTiming => InvokeTiming.LateUpdate;

    private MoveContext moveContext;
    private JumpContext jumpContext;

    [field: SerializeField] public Animator Animator { get; private set; }

    public override void Initialize(
        IReadOnlyRegistry<IMovementSetting> settingRegistry,
        IReadOnlyRegistry<IMovementContext> contextRegistry)
    {
        moveContext = contextRegistry.Get<MoveContext>();
        jumpContext = contextRegistry.Get<JumpContext>();
    }

    public override void Process()
    {
        Animator.SetBool("IsMoving", moveContext.IsMoving);
        Animator.SetBool("IsJumping", jumpContext.IsJumping);
        Animator.SetFloat("Speed", Mathf.Abs(moveContext.LinearVelocityX));
    }
}
```

---

## 1.5 Common Mistakes / Cautions

### Querying the Registry Every Frame in `Process()` Instead of Caching in `Initialize()`

```csharp
// Wrong - querying the Registry every frame incurs unnecessary overhead
public override void Process()
{
    var setting = settingRegistry.Get<MoveSetting>();
}

// Correct - cache once in Initialize()
public override void Initialize(...)
{
    setting = settingRegistry.Get<MoveSetting>();
}
```

---

### Missing `[RequireSetting]` / `[RequireContext]` Attributes

Omitting attributes can cause you to forget to register dependencies in the Controller.

```csharp
// Wrong - without attributes, validation is not possible when fetching from the Registry
public class MoveProcessor : MovementProcessor { }

// Correct
[RequireSetting(typeof(MoveSetting))]
[RequireContext(typeof(MoveContext))]
public class MoveProcessor : MovementProcessor { }
```

---

### Attempting to Modify Setting Values Inside a Processor

Setting is immutable at runtime. Attempting to modify a Setting value inside a Processor will cause a compile error.  
Any value that needs to change at runtime must be stored in Context.

```csharp
// Compile error
setting.MoveSpeedOnGround = 10f;

// Correct approach
context.Speed = 10f;
```