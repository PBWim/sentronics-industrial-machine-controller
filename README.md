# Sentronics Industrial Machine Controller

A simulated industrial manufacturing machine controller built with C# / .NET 8. The system monitors sensor readings (Temperature, Pressure), evaluates rules to determine which processing stages to execute, and runs them in parallel with safe concurrency management.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Build

```bash
dotnet build
```

## Run

```bash
dotnet run --project src/MachineController
```

The machine will start reading sensor values and executing stages based on the rules. Press `Ctrl+C` to stop.

## Run Tests

```bash
dotnet test
```

## Project Structure

```
├── docs/                        # Design document and architecture diagrams
├── src/
│   └── MachineController/       # Main application
│       ├── Common/              # Constants and enums
│       ├── Sensors/             # ISensor, TemperatureSensor, PressureSensor
│       ├── Resources/           # Resource with Idle/Busy/Error states
│       ├── Stages/              # Stage with lock ordering
│       └── Engine/              # RuleEngine, MachineControl
├── tests/
│   └── MachineController.Tests/ # xUnit tests
└── MachineController.sln
```

## Design Documentation

See [docs/README.md](docs/README.md) for the full design document including architecture diagrams, component breakdown, concurrency strategy, and design considerations.
