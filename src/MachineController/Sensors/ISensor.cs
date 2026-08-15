namespace MachineController.Sensors
{
    /// <summary>
    /// Sensors produce readings every 100ms on their own thread
    /// We have Temperature and Pressure sensors
    /// They implement a common ISensor interface (for extensibility)
    /// They use ManualResetEventSlim to signal when the first reading is ready (solves the order violation)
    /// </summary>
    public interface ISensor
    {
        string Type { get; }

        double CurrentValue { get; }

        void Start(CancellationToken cancellationToken);

        void WaitUntilReady();
    }
}
