using Discitur.Legacy.Migration.Infrastructure.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Discitur.Legacy.Migration.Infrastructure
{
    public class MigrationProcess
    {
        private readonly IList<IMigrationStep> _steps;
        private List<string> _migrationLogs;
        static private Status _status = Status.Off;

        private enum Status
        {
            Off,
            Initializing,
            Configured,
            Executing,
            Error
        }

        protected MigrationProcess(IList<IMigrationStep> steps, IList<string> _logs)
        {
            _steps = steps;
            _migrationLogs = (List<string>)_logs;
        }

        /// <summary>
        /// Initialize and create Migration Process container
        /// </summary>
        /// <returns></returns>
        public static MigrationProcess Init(IList<string> _logs)
        {
            if (!_status.Equals(Status.Off))
            {
                throw new Exception("Migration Process cannot be Re-initialized, because already in progress! Current status is " + _status);
            }
            _status = Status.Initializing;
            var _steps = new List<IMigrationStep>();
            return new MigrationProcess(_steps, _logs);
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
        public IList<string> Execute()
        {
            if (!_status.Equals(Status.Configured))
            {
                throw new Exception("Migration Process cannot start because it is not on Configured status! Current status is " + _status);
            }
            _status = Status.Executing;
            _migrationLogs.Add(string.Format("{0} - Migration Process begins...", DateTime.Now));
            Trace.WriteLine("Migration Process begins...","Migration Process");
            try
            {
                foreach (var step in _steps)
                {
                    _migrationLogs.Add(string.Format("{0} - Migration Process - step: {1} begins...", DateTime.Now, step.GetType().Name));
                    Trace.WriteLine(String.Format("Migration Process - step: {0} begins...", step.GetType().Name), "Migration Process");
                    _migrationLogs.AddRange( step.Execute());
                    _migrationLogs.Add(string.Format("{0} - Migration Process - step: {1} completed...", DateTime.Now, step.GetType().Name));
                    Trace.WriteLine(String.Format("Migration Process - step: {0} completed...", step.GetType().Name), "Migration Process");
                }
            }
            catch (RecoverableException ex)
            {
                _status = Status.Off;
                Trace.WriteLine(String.Format("Migration Process exit: {0}", ex.Message), "Migration Process");
                throw;
            }
            _migrationLogs.Add(string.Format("{0} - Migration Process successfully completed.", DateTime.Now));
            Trace.WriteLine("Migration Process successfully completed.", "Migration Process");
            _status = Status.Off;
            return _migrationLogs;
        }
    }

}
