using System.Collections.Generic;

namespace Discitur.Legacy.Migration.Infrastructure
{
    public class MigrationProcess
    {
        private readonly IList<IMigrationStep> _steps;

        protected MigrationProcess(IList<IMigrationStep> steps)
        {
            _steps = steps;
        }

        /// <summary>
        /// Initialize and create Migration Process container
        /// </summary>
        /// <returns></returns>
        public static MigrationProcess Init()
        {
            var _steps = new List<IMigrationStep>();
            return new MigrationProcess(_steps);
        }

        /// <summary>
        /// Add sequential migration step
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="step"></param>
        /// <returns></returns>
        public MigrationProcess Then<T>(T step)
        {
            _steps.Add((IMigrationStep)step);
            return this;
        }

        /// <summary>
        /// Execute Migration's steps
        /// </summary>
        public void Execute()
        {
            foreach (var step in _steps)
            {
                step.Execute();
            }
        }
    }

}
