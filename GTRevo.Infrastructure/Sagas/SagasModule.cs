﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Core;
using GTRevo.Core.Core.Lifecycle;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.Events.Async;
using Ninject.Modules;

namespace GTRevo.Infrastructure.Sagas
{
    public class SagasModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ISagaLocator>()
                .To<SagaLocator>()
                .InRequestOrJobScope();

            Bind<ISagaRegistry>()
                .To<SagaRegistry>()
                .InSingletonScope();

            Bind<ISagaConfigurator>()
                .To<SagaConventionConfigurator>()
                .InSingletonScope();

            Bind<IApplicationStartListener>()
                .To<SagaConfigurationLoader>()
                .InSingletonScope();

            Bind<ISagaRepository>() //itransactionprovider?
                .To<SagaRepository>()
                .InRequestOrJobScope();

            Bind<IAsyncEventSequencer<DomainEvent>, SagaEventListener.SagaEventSequencer>()
                .To<SagaEventListener.SagaEventSequencer>()
                .InRequestOrJobScope();

            Bind<IAsyncEventListener<DomainEvent>>()
                .To<SagaEventListener>()
                .InRequestOrJobScope();
        }
    }
}
