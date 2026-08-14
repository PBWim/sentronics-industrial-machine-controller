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
