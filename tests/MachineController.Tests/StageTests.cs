using MachineController.Common;
using MachineController.Resources;
using MachineController.Stages;

namespace MachineController.Tests
{
    public class StageTests
    {
        /// <summary>
        /// No deadlock when all three stages run in parallel
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AllStages_ShouldCompleteWithoutDeadlock()
        {
            var resourceA = new Resource("R_A", 1);
            var resourceB = new Resource("R_B", 2);
            var resourceC = new Resource("R_C", 3);

            var stage1 = new Stage("stage_1", new List<Resource> { resourceA, resourceB });
            var stage2 = new Stage("stage_2", new List<Resource> { resourceB, resourceC });
            var stage3 = new Stage("stage_3", new List<Resource> { resourceA, resourceC });

            // Run all three in parallel — should complete within 5 seconds
            var allStages = Task.WhenAll(
                stage1.ExecuteAsync(),
                stage2.ExecuteAsync(),
                stage3.ExecuteAsync()
            );

            // If stages finish first → completed = true → no deadlock.
            // If timer finishes first → completed = false → stages are stuck (deadlock).
            var completed = await Task.WhenAny(allStages, Task.Delay(5000)) == allStages;

            // If this is true, no deadlock occurred
            Assert.True(completed, "All stages should complete without deadlock");

            // All resources should be released
            Assert.Equal(ResourceState.Idle, resourceA.State);
            Assert.Equal(ResourceState.Idle, resourceB.State);
            Assert.Equal(ResourceState.Idle, resourceC.State);
        }
    }
}
