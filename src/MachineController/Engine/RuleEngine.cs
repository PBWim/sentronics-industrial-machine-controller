using MachineController.Common;
using MachineController.Stages;

namespace MachineController.Engine
{
    public class RuleEngine
    {
        private readonly List<Rule> _rules = new();

        public RuleEngine(List<Stage> allStages)
        {
            // Define the rules — each rule has a condition and the stages it triggers
            // Adding a new rule is just adding one more entry. No existing code changes needed (extensibility).
            _rules.Add(new Rule(
                MachineControllerConstants.Rule1,
                (temp, pressure) => temp > 10 && pressure < 100,
                allStages.Where(s => s.Name == MachineControllerConstants.Stage1 || s.Name == MachineControllerConstants.Stage2).ToList()
            ));

            _rules.Add(new Rule(
                MachineControllerConstants.Rule2,
                (temp, pressure) => temp > 5 && pressure < 50,
                allStages.Where(s => s.Name == MachineControllerConstants.Stage3 || s.Name == MachineControllerConstants.Stage2).ToList()
            ));

            _rules.Add(new Rule(
                MachineControllerConstants.Rule3,
                (temp, pressure) => temp > 20 && pressure < 100,
                allStages.Where(s => s.Name == MachineControllerConstants.Stage1 || s.Name == MachineControllerConstants.Stage3).ToList()
            ));
        }

        /// <summary>
        /// Evaluate all rules against current sensor values.
        /// Returns a deduplicated list of stages that should execute.
        /// </summary>
        public List<Stage> Evaluate(double temperature, double pressure)
        {
            var stagesToRun = new List<Stage>();

            foreach (var rule in _rules)
            {
                if (rule.Condition(temperature, pressure))
                {
                    stagesToRun.AddRange(rule.Stages);
                }
            }

            // Deduplicate — a stage should only run once even if multiple rules trigger it
            return stagesToRun.Distinct().ToList();
        }
    }
}
