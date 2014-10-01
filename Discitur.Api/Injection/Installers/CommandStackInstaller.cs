using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Discitur.CommandStack.Logic.CommandHandlers;
using Discitur.CommandStack.Logic.Validators;
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
            container.Register(
                Classes
                .FromAssemblyContaining<UserCommandHandlers>()
                .BasedOn(typeof(ICommandHandler<>)) // That implement ICommandHandler Interface
                .WithService.Base()    // and its name contain "CommandHandler"
                .LifestyleSingleton()
                );

            container.Register(
                Classes
                .FromAssemblyContaining<RegisterUserCommandValidator>()
                .BasedOn(typeof(IValidator<>)) // That implement IValidator Interface
                .WithService.Base()    // and its name contain "Validator"
                .LifestyleSingleton()
                );

            // DI Registration for Typed Factory for Command and Event Handlers
            container.AddFacility<TypedFactoryFacility>()
                .Register(Component.For<ICommandHandlerFactory>().AsFactory())
                .Register(Component.For<ICommandValidatorFactory>().AsFactory())
                .Register(Component.For<IEventHandlerFactory>().AsFactory());
        }
    }
}