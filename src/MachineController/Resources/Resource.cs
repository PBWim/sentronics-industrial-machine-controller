using MachineController.Common;

namespace MachineController.Resources
{
    /// <summary>
    /// A Resource represents a physical machine part (R_A, R_B, R_C)
    /// It has 3 states: Idle, Busy, Error
    /// Only one stage can use it at a time → we use SemaphoreSlim(1)
    /// For that, it needs Acquire() and Release() methods
    /// </summary>
    public class Resource
    {
        /// <summary>
        /// Only 1 thread can hold this resource at a time. 
        /// This solves the atomicity violation problem — no gap between checking and acquiring.
        /// </summary>
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public string Name { get; }

        /// <summary>
        /// Tracks Idle/Busy/Error for visibility and testing.
        /// </summary>
        public ResourceState State { get; private set; } = ResourceState.Idle;

        /// <summary>
        /// Priority is used to enforce a fixed acquisition order across all stages.
        /// R_A=1, R_B=2, R_C=3 — this prevents deadlocks by breaking circular wait.
        /// </summary>
        public int Priority { get; }

        public Resource(string name, int priority)
        {
            Name = name;
            Priority = priority;
        }

        /// <summary>
        /// The AcquireAsync method is used to acquire the resource. 
        /// It waits for the semaphore to be available, and then sets the state of the resource to Busy.
        /// </summary>
        /// <returns></returns>
        public async Task AcquireAsync()
        {
            // Atomic — only one thread gets through
            await _semaphore.WaitAsync();   

            State = ResourceState.Busy;
        }

        /// <summary>
        /// Marks it Idle and opens the semaphore so the next waiting stage can proceed.
        /// </summary>
        public void Release()
        {
            State = ResourceState.Idle;

            // Let the next thread in
            _semaphore.Release();           
        }
    }
}
