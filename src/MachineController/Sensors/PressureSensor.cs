using MachineController.Common;

namespace MachineController.Sensors
{
    public class PressureSensor : ISensor
    {
        private readonly ManualResetEventSlim _ready = new(false);
        private readonly Random _random = new();

        private readonly int _minValue;
        private readonly int _maxValue;

        public string Type => MachineControllerConstants.PressureSensorType;

        public double CurrentValue { get; private set; }

        public PressureSensor(int minValue, int maxValue)
        {
            _minValue = minValue;
            _maxValue = maxValue;
        }

        public void Start(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    CurrentValue = _random.Next(_minValue, _maxValue);
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
