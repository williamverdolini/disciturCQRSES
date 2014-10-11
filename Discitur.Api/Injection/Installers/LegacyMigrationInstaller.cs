using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Discitur.Legacy.Migration.Infrastructure;
using Discitur.Legacy.Migration.Logic;
using Discitur.Legacy.Migration.Model;
using Discitur.Legacy.Migration.Worker;
using Discitur.QueryStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Discitur.Api.Injection.Installers
{
    public class LegacyMigrationInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            // DI Registration for IDatabase (Migration)
            container.Register(Component.For<IDatabase>().ImplementedBy<LegacyDatabase>().LifestyleTransient());
            // DI Registration for Migration Manager, with dependency on specific IDatabase implementation for migration
            container.Register(Component.For<ILegacyMigrationManager>().ImplementedBy<LegacyMigrationManager>().LifestyleTransient());

            // DI Registration for DefaultInterfaces in namespace "Discitur.Legacy.Migration.Model"
            // Note: use of specific IDatabase implementation for Migration (LegacyDatabase)
            container.Register(
                Classes
                .FromAssemblyContaining<UserMigration>().InNamespace("Discitur.Legacy.Migration.Model")
                .WithService.DefaultInterfaces()
                .LifestyleTransient()
                .Configure(component => component.DependsOn(Dependency.OnComponent<IDatabase, LegacyDatabase>()))                
                );

            // DI Registration for Typed Factory for IMigrationStep
            container.Register(Component.For<IMigrationStepFactory>().AsFactory());

            // Register Worker Services
            container.Register(Component.For<LegacyMigrationWorker>().LifeStyle.Transient);
        }
    }
}