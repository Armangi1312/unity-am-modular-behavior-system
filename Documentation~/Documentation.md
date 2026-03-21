# Unity Modular Behavior System - Documentation

---

## 1. Overview

This document describes the structure, design philosophy, and usage of the Unity Modular Behavior System.

The goal of this system is to manage game object behaviors by **separating data and logic into distinct units.**  
Moving away from the traditional monolithic MonoBehaviour-centered design, it is built to allow behaviors to be composed and combined as independent modules.

The core design goals are as follows:

- Separation of Concerns
- High Reusability
- Runtime Performance Optimization (Minimal GC)
- Inspector-based Extensible Architecture

Each behavior consists of the following three components:

| Component | Role |
|-----------|------|
| Setting | Immutable or semi-immutable data |
| Context | Runtime state data |
| Processor | Actual execution logic |

This structure clearly separates data from execution flow, maximizing maintainability.

---

## 2. Detailed Usage

---

## 2.1 Package Installation

Install the package via Git URL through the Unity Package Manager.

```
https://github.com/Armangi1312/unity-am-modular-behavior-system.git
```

Select **Add package from Git URL** in the Package Manager and enter the URL above.

---

## 2.2 Behavior Configuration

Add Setting, Processor, and Context in the Unity Inspector to configure behaviors.

The combination and order of Processors defines a single character's behavior.  
Each Processor operates independently but interacts through shared Contexts.

---

### Component Details

#### 1. Setting

- Data that does not change during runtime
- Configuration values that express design intent
- Tuning data adjusted by designers

Examples:
- Movement speed
- Jump height
- Attack cooldown

It is recommended to use `private set` where possible to maintain immutability.

---

#### 2. Context

- State data that continuously changes during runtime
- Medium for passing data between Processors
- Represents the current state of a behavior

Examples:
- Current velocity
- Input direction
- Current health

Context acts as a state store. Keeping it as a simple data structure is advantageous for performance.

---

#### 3. Processor

- The unit that executes actual behavior logic
- Invoked according to a specific InvokeTiming
- Operates by referencing Setting and Context

It is recommended that Processors follow the Single Responsibility Principle.  
Designing each Processor to perform only one function is better for maintainability.

---

# 3. Code Writing Guide

---

## 3.1 Writing a Setting
```csharp
[Serializable]
public class YourName : ISetting
{
    [field: SerializeField]
    public float Value { get; private set; }
}
```

Setting holds data that does not change at runtime.  
Using `private set` is recommended to prevent external modification where possible.

---

## 3.2 Writing a Context
```csharp
[Serializable]
public class YourName : IContext
{
    [field: SerializeField]
    public float Value { get; set; }
}
```

Context stores runtime state.  
For performance-critical cases, consider using plain fields instead of auto-properties.

---

## 3.3 Writing a Processor
```csharp
[Serializable]
[RequireSetting(typeof(-))]
[RequireContext(typeof(-))]
public class YourName 
    : Processor<IRequiredSetting, IRequiredContext>
{
    public override void Initialize(
        Registry<IRequiredSetting> settingRegistry,
        Registry<IRequiredContext> contextRegistry)
    {
        var setting = settingRegistry.Get<IRequiredSetting>();
        var context = contextRegistry.Get<IRequiredContext>();
    }

    public override void Process()
    {
    }
}
```

### RequireSetting / RequireContext

These attributes perform the following:

- Declare dependencies explicitly
- Enable automatic registration
- Ensure structural safety

By clearly declaring the Settings and Contexts a Processor requires, initialization omissions are prevented.

---

# 4. Movement Processor Example

(Preserve existing code)
```
[Keep previous code block as-is]
```

---

# 5. Controller

## 5.1 Role of the Controller

The Controller is a component that groups multiple Processors and manages them as a single execution unit.

The Controller is responsible for:

- Processor initialization
- Setting/Context Registry construction
- Execution order management
- Execution control based on InvokeTiming

In short, the Controller acts as the orchestrator of the execution pipeline.

---

## 5.2 Controller Structure

The Controller is used by inheriting from it as follows:
```csharp
public class Name : Controller<IRequiredSetting, IRequiredContext>
{
}
```

Internally, the Controller:

- Registers all Settings into the Registry
- Registers all Contexts into the Registry
- Validates dependencies based on each Processor's Require information
- Calls Initialize on each Processor
- Calls Process at the designated timing

---

## 5.3 Execution Flow

- Controller creation
- Setting/Context composition
- Processor registration
- Initialization phase
- Process execution based on InvokeTiming

Processors do not directly reference each other.  
They interact indirectly through the Context.

This structure minimizes dependencies and improves testability.

---

## 5.4 Controller Design Advantages

- Clear separation of behavior units
- Centralized management of execution flow
- Easy module replacement
- Designer-friendly structure
- GC-minimized design possible

The Controller is not simply a MonoBehaviour —  
it is the management layer of the behavior execution pipeline.