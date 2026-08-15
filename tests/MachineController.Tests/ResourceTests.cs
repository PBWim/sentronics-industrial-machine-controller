using MachineController.Common;
using MachineController.Resources;

namespace MachineController.Tests
{
    public class ResourceTests
    {
        /// <summary>
        /// We create a resource (starts as Idle by default), call AcquireAsync(), and then check if the state changed to Busy. 
        /// If it did, the test passes.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AcquireAsync_ShouldSetStateToBusy()
        {
            // Arrange
            // 1. Create a resource
            var resource = new Resource("R_A", 1);

            // Act
            // 2. Acquire it
            await resource.AcquireAsync();

            // Assert
            // 3. Check: is the state now Busy?
            Assert.Equal(ResourceState.Busy, resource.State);
        }

        /// <summary>
        /// We first acquire the resource (making it Busy), then release it, and check if it went back to Idle. 
        /// If it did, the test passes.
        /// </summary>
        [Fact]
        public async Task Release_ShouldSetStateToIdle()
        {
            // Arrange
            // 1. Create a resource
            var resource = new Resource("R_A", 1);

            // 2. Acquire it first (so it becomes Busy)
            await resource.AcquireAsync();

            // Act
            // 3. Release it
            resource.Release();

            // Assert
            // 4. Check: is the state now Idle?
            Assert.Equal(ResourceState.Idle, resource.State);
        }

        /// <summary>
        /// This test is proving the atomicity violation fix — two stages can never hold the same resource at the same time.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AcquireAsync_ShouldBlockSecondThread()
        {
            // Arrange
            // 1. Create a resource
            var resource = new Resource("R_A", 1);

            // 2. First thread acquires it
            await resource.AcquireAsync();

            // Act
            // Try to acquire from another thread — should not complete within 200ms
            // 3. Second thread tries to acquire — should be blocked
            var secondAcquire = resource.AcquireAsync();

            // 4. Wait 200ms and check — did the second acquire complete?
            var completed = await Task.WhenAny(secondAcquire, Task.Delay(200)) == secondAcquire;

            // Assert
            // 5. It should NOT have completed — the resource is still held
            Assert.False(completed, "Second acquire should be blocked");

            // 6. Clean up — release so the second thread can proceed
            resource.Release();
        }
    }
}