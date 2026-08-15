using MachineController.Resources;

namespace MachineController.Stages
{
    /// <summary>
    /// A stage acquires resources, does work, then releases them
    /// Resources are acquired in fixed order (R_A → R_B → R_C) to prevent deadlocks
    /// </summary>
    public class Stage
    {
        public string Name { get; }

        private readonly List<Resource> _requiredResources;

        public Stage(string name, List<Resource> requiredResources)
        {
            Name = name;

            // Lock Ordering : Sort by priority to prevent deadlock (R_A=1, R_B=2, R_C=3)
            // No matter what order you pass the resources in, they'll always be acquired R_A → R_B → R_C.
            _requiredResources = requiredResources.OrderBy(r => r.Priority).ToList();
        }

        /// <summary>
        /// Flow for a stage is : 
        ///     1. Acquire all resources (one by one, in order)
        ///     2. Do the work (needs all of them)
        ///     3. Release all resources (in the finally block)
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteAsync()
        {
            try
            {
                // Acquire resources in fixed order — prevents circular wait
                foreach (var resource in _requiredResources)
                {
                    await resource.AcquireAsync();
                    Console.WriteLine($"[{Name}] Acquired {resource.Name}");
                }

                // Simulate processing work
                Console.WriteLine($"[{Name}] Processing...");

                // Simulates the stage doing actual work
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                // If something goes wrong, mark all acquired resources as Error
                // Error path: Acquire → Process fails → SetError → Release (state goes Idle → Busy → Error → Idle)
                Console.WriteLine($"[{Name}] Error: {ex.Message}");

                foreach (var resource in _requiredResources)
                {
                    resource.SetError();
                }
            }
            finally
            {
                // Always release resources, even if an exception occurs
                // This prevents a resource from being stuck in Busy forever.
                foreach (var resource in _requiredResources)
                {
                    resource.Release();
                    Console.WriteLine($"[{Name}] Released {resource.Name}");
                }
            }
        }
    }
}
