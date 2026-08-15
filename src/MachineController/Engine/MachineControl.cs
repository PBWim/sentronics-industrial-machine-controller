using MachineController.Common;
using MachineController.Sensors;

namespace MachineController.Engine
{
    /// <summary>
    /// Starts all sensors
    /// Waits for sensors to be ready (order violation fix)
    /// Enters main loop: read sensors → evaluate rules → launch stages in parallel → repeat
    /// </summary>
    public class MachineControl
    {
        private readonly List<ISensor> _sensors;
        private readonly RuleEngine _ruleEngine;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public MachineControl(List<ISensor> sensors, RuleEngine ruleEngine)
        {
            _sensors = sensors;
            _ruleEngine = ruleEngine;
        }

        public async Task StartAsync()
        {
            // 1. Start all sensors on their own threads
            foreach (var sensor in _sensors)
            {
                sensor.Start(_cancellationTokenSource.Token);
            }

            // 2. Wait for ALL sensors to produce their first reading
            //    This prevents order violation — no processing with uninitialized values
            foreach (var sensor in _sensors)
            {
                sensor.WaitUntilReady();
                Console.WriteLine($"[Controller] {sensor.Type} sensor is ready.");
            }

            Console.WriteLine("[Controller] All sensors ready. Starting main loop.");

            // 3. Main loop
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                // Read current sensor values
                var temperature = _sensors.First(s => s.Type == MachineControllerConstants.TemperatureSensorType).CurrentValue;
                var pressure = _sensors.First(s => s.Type == MachineControllerConstants.PressureSensorType).CurrentValue;
                Console.WriteLine($"[Controller] Temperature: {temperature}, Pressure: {pressure}");

                // Evaluate rules
                var stagesToRun = _ruleEngine.Evaluate(temperature, pressure);

                if (stagesToRun.Any())
                {
                    Console.WriteLine($"[Controller] Running stages: {string.Join(", ", stagesToRun.Select(s => s.Name))}");

                    // Launch all stages in parallel and wait for all to complete
                    // This is where concurrency happens. Stages compete for shared resources.
                    var tasks = stagesToRun.Select(s => s.ExecuteAsync()).ToArray();
                    await Task.WhenAll(tasks);

                    Console.WriteLine("[Controller] All stages completed.");
                }
                else
                {
                    Console.WriteLine("[Controller] No rules matched. Skipping.");
                }

                // Wait before next cycle
                await Task.Delay(1000, _cancellationTokenSource.Token);
            }
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            Console.WriteLine("[Controller] Shutting down.");
        }

    }
}
