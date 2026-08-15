using MachineController.Stages;

namespace MachineController.Engine
{
    public class Rule
    {
        public string Name { get; }

        /// <summary>
        /// The Condition is a function that takes temperature and pressure, returns true/false. This makes rules easy to add or change.
        /// </summary>
        public Func<double, double, bool> Condition { get; }

        public List<Stage> Stages { get; }

        public Rule(string name, Func<double, double, bool> condition, List<Stage> stages)
        {
            Name = name;
            Condition = condition;
            Stages = stages;
        }
    }
}
