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

<img width="3232" height="10528" alt="2  machine-controller-flow" src="https://github.com/user-attachments/assets/f98a471e-25e2-44f3-8250-ba4ce03c2c2f" />

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

<img width="2944" height="8160" alt="4  stage-execution-flow" src="https://github.com/user-attachments/assets/0d58ea04-b322-44b5-830a-0abbf6920191" />




