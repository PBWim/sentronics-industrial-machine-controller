using MachineController.Common;
using MachineController.Engine;
using MachineController.Resources;
using MachineController.Sensors;
using MachineController.Stages;

/// <summary>
/// Program.cs creates everything
///     → MachineController starts sensors
///     → Waits for sensors (ManualResetEventSlim)
///     → Reads values → RuleEngine evaluates
///     → Stages run in parallel (Task.WhenAll)
///     → Each Stage acquires Resources in order (SemaphoreSlim + lock ordering)
///     → Repeat
/// </summary>
internal class Program
{
    private static async Task Main(string[] args)
    {
        // Create resources with priority for lock ordering
        Resource resourceA = new (MachineControllerConstants.ResourceA, 1);
        Resource resourceB = new (MachineControllerConstants.ResourceB, 2);
        Resource resourceC = new (MachineControllerConstants.ResourceC, 3);

        // Create stages with their required resources
        Stage stage1 = new (MachineControllerConstants.Stage1, new List<Resource> { resourceA, resourceB });
        Stage stage2 = new (MachineControllerConstants.Stage2, new List<Resource> { resourceB, resourceC });
        Stage stage3 = new (MachineControllerConstants.Stage3, new List<Resource> { resourceA, resourceC });

        // Create sensors
        List<ISensor> sensors = new()
        {
            new TemperatureSensor(),
            new PressureSensor()
        };

        // Create rule engine with all stages
        RuleEngine ruleEngine = new (new List<Stage> { stage1, stage2, stage3 });

        // Create and start the machine controller
        MachineControl controller = new (sensors, ruleEngine);

        // Handle Ctrl+C to stop gracefully
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            controller.Stop();
        };

        Console.WriteLine("Machine Controller starting. Press Ctrl+C to stop.");
        await controller.StartAsync();
    }
}