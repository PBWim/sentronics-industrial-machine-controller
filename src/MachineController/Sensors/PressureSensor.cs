using MachineController.Common;

namespace MachineController.Sensors
{
    public class PressureSensor : ISensor
    {
        private readonly ManualResetEventSlim _ready = new(false);
        private readonly Random _random = new();

        public string Type => MachineControllerConstants.PressureSensorType;

        public double CurrentValue { get; private set; }

        public void Start(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    CurrentValue = _random.Next(0, 120);
                    _ready.Set();
                    await Task.Delay(100, cancellationToken);
                }
            }, cancellationToken);
        }

        public void WaitUntilReady()
        {
            _ready.Wait();
        }
    }
}
