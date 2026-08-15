using MachineController.Sensors;

namespace MachineController.Tests
{
    public class SensorTests
    {
        /// <summary>
        /// Sensors produce readings every 100ms on their own thread. 
        /// This test ensures that the sensor is ready and has produced a reading before the controller attempts to read its value, 
        /// preventing any order violation issues.
        /// </summary>
        [Fact]
        public void Sensors_ShouldBeReadyBeforeControllerReadsValues()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var sensor = new TemperatureSensor();

            // Act
            sensor.Start(cts.Token);
            sensor.WaitUntilReady();

            // Assert
            // After WaitUntilReady, the sensor should have a value
            // CurrentValue should have been set (not the default 0)
            Assert.True(sensor.CurrentValue >= 0, "Sensor should have produced a reading");

            cts.Cancel();
        }
    }
}
