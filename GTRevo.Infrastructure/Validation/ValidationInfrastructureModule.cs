﻿using GTRevo.Core.Commands;
using Ninject.Modules;

namespace GTRevo.Infrastructure.Validation
{
    public class ValidationInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IPreCommandFilter<ICommandBase>>()
                .To<CommandAttributeValidationFilter>()
                .InSingletonScope();
        }
    }
}
