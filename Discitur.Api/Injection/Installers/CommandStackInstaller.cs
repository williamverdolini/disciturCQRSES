using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Discitur.CommandStack.Logic.CommandHandlers;
using Discitur.CommandStack.Logic.Sagas;
using Discitur.CommandStack.Logic.Validators;
using Discitur.CommandStack.Worker;
using Discitur.Infrastructure.Api;
using Discitur.Infrastructure.Commands;
using Discitur.Infrastructure.Events;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Discitur.Api.Injection.Installers
{
    public class CommandStackInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            // CommandStack Command Handlers Registration
            container.Register(
                Classes
                .FromAssemblyContaining<UserCommandHandlers>()
                .BasedOn(typeof(ICommandHandler<>)) // That implement ICommandHandler Interface
                .WithService.Base()    // and its name contain "CommandHandler"
                .LifestyleSingleton()
                );
            // CommandStack Command Validators Registration
            container.Register(
                Classes
                .FromAssemblyContaining<RegisterUserCommandValidator>()
                .BasedOn(typeof(IValidator<>)) // That implement IValidator Interface
                .WithService.Base()    // and its name contain "Validator"
                .LifestyleSingleton()
                );
            // CommandStack Event Handlers Registration
            container.Register(
                Classes
                .FromAssemblyContaining<UserCreditsSagaEventHandlers>()
                .BasedOn(typeof(IEventHandler<>)) // That implement IEventHandler Interface
                .WithService.Base()    // and its name contain "CommandHandler"
                .LifestyleSingleton()
                );
            // DI Registration for Typed Factory for Command and Event Handlers
            container.AddFacility<TypedFactoryFacility>()
                .Register(Component.For<ICommandHandlerFactory>().AsFactory())
                .Register(Component.For<ICommandValidatorFactory>().AsFactory())
                .Register(Component.For<IEventHandlerFactory>().AsFactory());

            // Register Command Worker Services
            container.Register(
                Classes
                .FromAssemblyContaining<UserCommandWorker>()
                .BasedOn(typeof(ICommandWorker))      // That implement ICommandWorker Interface
                .WithService.DefaultInterfaces()      // in its hierarchy
                .LifestyleTransient()
                );
        }
    }
}