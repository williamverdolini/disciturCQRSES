using Discitur.Legacy.Migration.Infrastructure.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Discitur.Legacy.Migration.Infrastructure
{
    public class MigrationProcess
    {
        private readonly IList<IMigrationStep> _steps;
        static private Status _status = Status.Off;

        private enum Status
        {
            Off,
            Initializing,
            Configured,
            Executing,
            Error
        }

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
            if (!_status.Equals(Status.Off))
            {
                throw new Exception("Migration Process cannot be Re-initialized, because already in progress! Current status is " + _status);
            }
            _status = Status.Initializing;
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
            if (!_status.Equals(Status.Initializing))
            {
                throw new Exception("Migration Process is on uncorrect status! Current status is " + _status + " instead of " + Status.Initializing);
            }
            _steps.Add((IMigrationStep)step);
            return this;
        }

        /// <summary>
        /// Set the Migration configuration as initialized
        /// </summary>
        /// <returns></returns>
        public MigrationProcess Configured()
        {
            _status = Status.Configured;
            return this;
        }

        /// <summary>
        /// Execute Migration's steps
        /// </summary>
        public void Execute()
        {
            if (!_status.Equals(Status.Configured))
            {
                throw new Exception("Migration Process cannot start because it is not on Configured status! Current status is " + _status);
            }
            _status = Status.Executing;
            Trace.WriteLine("Migration Process begins...","Migration Process");
            try
            {
                foreach (var step in _steps)
                {
                    Trace.WriteLine(String.Format("Migration Process - step: {0} begins...", step.GetType().Name), "Migration Process");
                    step.Execute();
                    Trace.WriteLine(String.Format("Migration Process - step: {0} completed...", step.GetType().Name), "Migration Process");
                }
            }
            catch (RecoverableException ex)
            {
                _status = Status.Off;
                Trace.WriteLine(String.Format("Migration Process exit: {0}", ex.Message), "Migration Process");
                throw;
            }
            Trace.WriteLine("Migration Process successfully completed.", "Migration Process");
            _status = Status.Off;
        }
    }

}
