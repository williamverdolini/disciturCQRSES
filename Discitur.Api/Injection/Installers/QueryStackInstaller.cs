using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Discitur.Infrastructure.Events;
using Discitur.Infrastructure.Events.Replaying;
using Discitur.QueryStack;
using Discitur.QueryStack.Logic.EventHandlers;
using Discitur.QueryStack.Logic.Services;

namespace Discitur.Api.Injection.Installers
{
    public class QueryStackInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            // QueryStack Event Handlers Registration
            container.Register(
                Classes
                .FromAssemblyContaining<UserEventHandlers>()
                .BasedOn(typeof(IEventHandler<>)) // That implement ICommandHandler Interface
                .WithService.Base()    // and its name contain "CommandHandler"
                .LifestyleSingleton()
                );

            // DI Registration for IDatabase (QueryStack)
            container.Register(Component.For<IDatabase>().ImplementedBy<Database>().LifestyleTransient());            
            container.Register(Component.For<IIdentityMapper>().ImplementedBy<IdentityMapper>().LifestyleTransient());
            container.Register(Component.For<IImageConverter>().ImplementedBy<ImageConverter>().LifestyleTransient());

            // DI Registration for Events Replaying
            container.Register(Component.For<IEventsReplayer>().ImplementedBy<EventsReplayer>().LifestyleTransient());
            container.Register(Component.For<IAdminDatabase>().ImplementedBy<AdminDatabase>().LifestyleTransient());
        }
    }
}