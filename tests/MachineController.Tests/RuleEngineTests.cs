using MachineController.Common;
using MachineController.Engine;
using MachineController.Resources;
using MachineController.Stages;

namespace MachineController.Tests
{
    public class RuleEngineTests
    {
        /// <summary>
        /// Rule 1: Temperature > 10, Pressure < 100 → stage_1, stage_2
        /// </summary>
        [Fact]
        public void Evaluate_Rule1_ShouldReturnStage1AndStage2()
        {
            // Arrange
            var resourceA = new Resource(MachineControllerConstants.ResourceA, 1);
            var resourceB = new Resource(MachineControllerConstants.ResourceB, 2);
            var resourceC = new Resource(MachineControllerConstants.ResourceC, 3);

            var stage1 = new Stage(MachineControllerConstants.Stage1, new List<Resource> { resourceA, resourceB });
            var stage2 = new Stage(MachineControllerConstants.Stage2, new List<Resource> { resourceB, resourceC });
            var stage3 = new Stage(MachineControllerConstants.Stage3, new List<Resource> { resourceA, resourceC });

            var ruleEngine = new RuleEngine(new List<Stage> { stage1, stage2, stage3 });

            // Act
            // Temperature > 10, Pressure < 100 → stage_1, stage_2
            var result = ruleEngine.Evaluate(15, 80);

            // Assert
            Assert.Contains(result, s => s.Name == MachineControllerConstants.Stage1);
            Assert.Contains(result, s => s.Name == MachineControllerConstants.Stage2);
        }

        /// <summary>
        /// Rule 2: Temperature > 5, Pressure < 50 → stage_3, stage_2
        /// </summary>
        [Fact]
        public void Evaluate_Rule2_ShouldReturnStage3AndStage2()
        {
            // Arrange
            var resourceA = new Resource(MachineControllerConstants.ResourceA, 1);
            var resourceB = new Resource(MachineControllerConstants.ResourceB, 2);
            var resourceC = new Resource(MachineControllerConstants.ResourceC, 3);

            var stage1 = new Stage(MachineControllerConstants.Stage1, new List<Resource> { resourceA, resourceB });
            var stage2 = new Stage(MachineControllerConstants.Stage2, new List<Resource> { resourceB, resourceC });
            var stage3 = new Stage(MachineControllerConstants.Stage3, new List<Resource> { resourceA, resourceC });

            var ruleEngine = new RuleEngine(new List<Stage> { stage1, stage2, stage3 });

            // Act
            // Temperature > 5, Pressure < 50 → stage_3, stage_2
            var result = ruleEngine.Evaluate(8, 30);

            // Assert
            Assert.Contains(result, s => s.Name == MachineControllerConstants.Stage3);
            Assert.Contains(result, s => s.Name == MachineControllerConstants.Stage2);
        }

        /// <summary>
        /// Rule 3: Temperature > 20, Pressure < 100 → stage_1, stage_3
        /// </summary>
        [Fact]
        public void Evaluate_Rule3_ShouldReturnStage1AndStage3()
        {
            // Arrange
            var resourceA = new Resource(MachineControllerConstants.ResourceA, 1);
            var resourceB = new Resource(MachineControllerConstants.ResourceB, 2);
            var resourceC = new Resource(MachineControllerConstants.ResourceC, 3);

            var stage1 = new Stage(MachineControllerConstants.Stage1, new List<Resource> { resourceA, resourceB });
            var stage2 = new Stage(MachineControllerConstants.Stage2, new List<Resource> { resourceB, resourceC });
            var stage3 = new Stage(MachineControllerConstants.Stage3, new List<Resource> { resourceA, resourceC });

            var ruleEngine = new RuleEngine(new List<Stage> { stage1, stage2, stage3 });

            // Act
            // Temperature > 20, Pressure < 100 → stage_1, stage_3
            var result = ruleEngine.Evaluate(25, 80);

            // Assert
            Assert.Contains(result, s => s.Name == MachineControllerConstants.Stage1);
            Assert.Contains(result, s => s.Name == MachineControllerConstants.Stage3);
        }

        /// <summary>
        /// Stages are deduplicated
        /// Scenario where multiple rules match and some stages are triggered by more than one rule.
        /// </summary>
        [Fact]
        public void Evaluate_MultipleRulesMatch_ShouldDeduplicateStages()
        {
            // Arrange
            var resourceA = new Resource(MachineControllerConstants.ResourceA, 1);
            var resourceB = new Resource(MachineControllerConstants.ResourceB, 2);
            var resourceC = new Resource(MachineControllerConstants.ResourceC, 3);

            var stage1 = new Stage(MachineControllerConstants.Stage1, new List<Resource> { resourceA, resourceB });
            var stage2 = new Stage(MachineControllerConstants.Stage2, new List<Resource> { resourceB, resourceC });
            var stage3 = new Stage(MachineControllerConstants.Stage3, new List<Resource> { resourceA, resourceC });

            var ruleEngine = new RuleEngine(new List<Stage> { stage1, stage2, stage3 });

            // Act
            // Temperature=25, Pressure=30 → all 3 rules match
            // Rule 1: stage_1, stage_2
            // Rule 2: stage_3, stage_2
            // Rule 3: stage_1, stage_3
            // After dedup: stage_1, stage_2, stage_3 (each once)
            var result = ruleEngine.Evaluate(25, 30);

            // Assert
            Assert.Equal(3, result.Count);
        }

        /// <summary>
        /// Evaluate with no rules matching should return an empty list
        /// </summary>
        [Fact]
        public void Evaluate_NoRulesMatch_ShouldReturnEmptyList()
        {
            // Arrange
            var resourceA = new Resource(MachineControllerConstants.ResourceA, 1);
            var resourceB = new Resource(MachineControllerConstants.ResourceB, 2);
            var resourceC = new Resource(MachineControllerConstants.ResourceC, 3);

            var stage1 = new Stage(MachineControllerConstants.Stage1, new List<Resource> { resourceA, resourceB });
            var stage2 = new Stage(MachineControllerConstants.Stage2, new List<Resource> { resourceB, resourceC });
            var stage3 = new Stage(MachineControllerConstants.Stage3, new List<Resource> { resourceA, resourceC });

            var ruleEngine = new RuleEngine(new List<Stage> { stage1, stage2, stage3 });

            // Act
            // Temperature=3, Pressure=120 → no rules match
            var result = ruleEngine.Evaluate(3, 120);

            // Assert
            Assert.Empty(result);
        }
    }
}
