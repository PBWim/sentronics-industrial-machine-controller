using MachineController.Common;

namespace MachineController.Sensors
{
    public class TemperatureSensor : ISensor
    {
        // 1. When the sensor first starts, it hasn't produced any reading yet.
        //    The Machine Controller should wait until the sensor has its first reading.
        private readonly ManualResetEventSlim _ready = new(false);

        private readonly Random _random = new();

        public string Type => MachineControllerConstants.TemperatureSensorType;

        public double CurrentValue { get; private set; }

        /// <summary>
        /// Start a background task that generates a new temperature reading every 100ms.
        /// Each sensor runs on its own thread
        /// </summary>
        public void Start(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    CurrentValue = _random.Next(0, 30);

                    // 2. Then when the sensor gets its first reading, it should signal the Machine Controller that it is ready to be used.
                    _ready.Set();

                    // 3. The sensor should continue to produce readings every 100ms until the Machine Controller tells it to stop.
                    await Task.Delay(100, cancellationToken);
                }
            }, cancellationToken);
        }

        /// <summary>
        /// The Machine Controller should be able to wait for the sensor to be ready before using it.
        /// </summary>
        public void WaitUntilReady()
        {
            _ready.Wait();
        }
    }
}
