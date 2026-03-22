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

### 1. Setting

[Setting Documentation](https://github.com/Armangi1312/unity-am-modular-behavior-system/blob/main/Documentation~/Setting.md)

---

### 2. Context
[Context Documentation](https://github.com/Armangi1312/unity-am-modular-behavior-system/blob/main/Documentation~/Context.md)

---

### 3. Processor
[Processor Documentation](https://github.com/Armangi1312/unity-am-modular-behavior-system/blob/main/Documentation~/Processor.md)

---

### 4. Controller
[Controller Documentation](https://github.com/Armangi1312/unity-am-modular-behavior-system/blob/main/Documentation~/Controller.md)