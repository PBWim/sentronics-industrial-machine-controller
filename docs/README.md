# Industrial Machine Controller — Design Document

## 1. Overview

This system simulates the software for an industrial manufacturing machine used in automated production environments. The machine has sensors (Temperature, Pressure) that continuously monitor its state. Based on sensor readings, the system determines which processing stages to execute. Stages run in parallel and share physical resources (robotic arm, conveyor belt, heating element), requiring careful concurrency management to prevent deadlocks and race conditions.

## 2. Architecture

The system is composed of five main components:

| Component | Responsibility |
|---|---|
| **Sensor** | Produces sensor readings every 100ms (e.g., Temperature, Pressure). Designed to be extensible — new sensor types can be added without modifying existing code. |
| **Resource** | Represents a physical machine part (R_A, R_B, R_C) with three states: Idle, Busy, and Error. Only one stage can use a resource at a time. |
| **Stage** | A processing step (stage_1, stage_2, stage_3) that acquires the resources it needs, performs work, and releases them. Acquires resources in a fixed order to prevent deadlocks. |
| **Rule Engine** | Evaluates the current sensor values against a set of rules and returns the list of stages that should be executed. |
| **Machine Controller** | The top-level coordinator. Starts sensors, reads their values, passes them to the Rule Engine, and launches the resulting stages in parallel. |

### Architecture Diagram

[Industrial Machine Controller - UML Architecture Diagram](https://www.figma.com/board/8MouWkbcXFUzp5CHbB69ZL/UML-diagram--Copy-?node-id=0-1&t=hXqVk7yM9mEQ7JC2-1)

<img width="5536" height="6080" alt="1  architecture-overview" src="https://github.com/user-attachments/assets/6c16fb6f-c6de-4dab-b856-b01e319e9559" />

## 3. Component Breakdown

### Machine Controller Flow

The Machine Controller is the entry point of the system. It starts all sensors, waits for them to be ready, then enters a continuous loop: read sensor values, evaluate rules, launch stages, wait for completion, repeat.

<img width="3232" height="10528" alt="2  machine-controller-flow" src="https://github.com/user-attachments/assets/e18ec17f-48ef-436e-800c-4624bbde375e" />

### Rule Engine Flow

The Rule Engine receives the current sensor values and checks each rule. If a rule's conditions are met, its associated stages are added to the execution list. The list is deduplicated before being returned.

**Rules:**
 
- Temperature > 10 AND Pressure < 100 → stage_1, stage_2
- Temperature > 5 AND Pressure < 50 → stage_3, stage_2
- Temperature > 20 AND Pressure < 100 → stage_1, stage_3

<img width="4832" height="9088" alt="3  rule-engine-flow" src="https://github.com/user-attachments/assets/4cd81a24-5f7d-485b-9519-665487cc3d15" />

### Stage Execution Flow

Each stage knows which resources it needs. Before executing, it sorts its required resources in a fixed order (R_A → R_B → R_C) to prevent deadlocks. It then acquires each resource in that order, performs its work, and releases them.

**Stage Map:**

- stage_1: R_A, R_B
- stage_2: R_B, R_C
- stage_3: R_A, R_C

<img width="2944" height="8160" alt="4  stage-execution-flow" src="https://github.com/user-attachments/assets/5a69e51a-1db8-4283-ae5b-f738263f6406" />

## 4. Communication Patterns

The components communicate in a top-down flow:
 
1. **Sensors** continuously produce readings independently on their own threads.
2. **Machine Controller** reads the latest value from each sensor.
3. **Machine Controller** passes these values to the **Rule Engine**.
4. **Rule Engine** evaluates the rules and returns a list of stages to run.
5. **Machine Controller** launches the **Stages** in parallel.
6. Each **Stage** acquires its **Resources**, performs work, and releases them.

Stages do not communicate with each other directly. They interact only through shared resources, which is where concurrency management is critical.

## 5. Concurrency Strategy

The system must handle three types of concurrency problems.

### 5.1 Deadlock Prevention
 
**Problem:** Stages run in parallel and share resources. If stages acquire resources in different orders, a circular wait can occur.
 
Example scenario where all three stages run simultaneously:
- stage_1 grabs R_B, then wants R_A
- stage_2 grabs R_C, then wants R_B
- stage_3 grabs R_A, then wants R_C
Result: stage_1 is waiting for R_A (held by stage_3), stage_3 is waiting for R_C (held by stage_2), stage_2 is waiting for R_B (held by stage_1). A circular chain — deadlock.
 
**Solution:** Enforce a total ordering on resource acquisition. All stages must acquire resources in alphabetical order: R_A → R_B → R_C, regardless of which resources they need. This breaks the circular wait condition and makes deadlock impossible.
 
- stage_1 (needs R_A, R_B) → acquires R_A first, then R_B
- stage_2 (needs R_B, R_C) → acquires R_B first, then R_C
- stage_3 (needs R_A, R_C) → acquires R_A first, then R_C
**Mechanism:** Each resource is assigned a numeric priority (R_A=1, R_B=2, R_C=3). Before execution, a stage sorts its required resources by priority and acquires them in that order.
 
### 5.2 Atomicity Violation Prevention
 
**Problem:** When two stages check a resource's state and try to acquire it simultaneously, a race condition can occur.
 
Example:
- stage_1 checks: "Is R_A idle?" → Yes
- stage_3 checks: "Is R_A idle?" → Yes (checked at the same time)
- Both stages set R_A to Busy — both think they own it
The check-then-acquire is two separate operations, and another thread can intervene between them.
 
**Solution:** Use `SemaphoreSlim(1)` for each resource. The semaphore combines the check and acquire into a single atomic operation. If one stage acquires the semaphore, the other automatically waits — there is no gap for a race condition.
 
### 5.3 Order Violation Prevention
 
**Problem:** The Machine Controller starts the sensors and then immediately begins the main loop. If the main loop reads sensor values before the sensors have produced their first reading, the system could operate on default/uninitialized values, leading to incorrect rule evaluation.
 
**Solution:** Use `ManualResetEventSlim` as a signal. Sensors signal when they have produced their first valid reading. The Machine Controller waits for this signal before entering the main loop. This guarantees that sensor data is available before any stages are launched.

## 6. Design Considerations

### Extensibility
 
- **Sensors** implement a common interface (`ISensor`). Adding a new sensor type (e.g., Humidity) requires creating a new class that implements `ISensor` and registering it with the Machine Controller. No existing code needs to change.
- **Rules** are stored as a configurable list in the Rule Engine. Adding a new rule means adding one entry to the list — no changes to existing rules.
- **Resources** are registered dynamically. Adding a new resource (e.g., R_D) requires creating a new `Resource` instance and updating the stage map.
### Maintainability
 
- Each component has a single, clear responsibility. If sensor reading logic needs to change, only the Sensor class is affected.
- Components are loosely coupled — the Rule Engine does not know how sensors produce their values, and Stages do not know how rules work.
- Small, focused classes make the codebase easy to navigate and understand.
### Testability
 
- Each component can be tested independently. The Rule Engine can be tested by passing fake sensor values and verifying which stages it returns — no real sensors needed.
- Stages can be tested with mock resources to verify correct acquisition order and release behavior.
- Concurrency behavior can be tested by running multiple stages in parallel and verifying that no resource is acquired by two stages simultaneously.
- Interfaces allow swapping real implementations with test doubles.
### Reliability
 
- Resources have an `Error` state to handle failure scenarios. If a stage encounters an error during processing, the resource transitions to Error instead of remaining stuck in Busy.
- Stages use `try/finally` to guarantee that resources are always released, even when exceptions occur. This prevents resources from being permanently locked.
- The Machine Controller can implement timeouts to detect stages that take too long and handle them gracefully.
- Sensors that stop producing readings can be detected and reported.

## 7. Technology Choices
 
| Choice | Reason |
|---|---|
| **C# / .NET 8** | Strong async/await support, built-in concurrency primitives (`SemaphoreSlim`, `ManualResetEventSlim`, `lock`), robust threading model |
| **xUnit** | Industry-standard testing framework for .NET |
| **No external dependencies** | All required concurrency mechanisms are available in the .NET base class library |
 
## 8. Assumptions
 
- The system controls a single manufacturing machine.
- Sensor values are simulated (generated randomly or from a predefined pattern).
- Stage processing is simulated (using `Task.Delay` to represent work being done).
- Resources are simple in-memory objects with state tracking, not connected to real hardware.
- All three rules are evaluated on every sensor update cycle; multiple rules can match simultaneously.




