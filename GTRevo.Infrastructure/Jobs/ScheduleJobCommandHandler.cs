﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GTRevo.Core.Commands;

namespace GTRevo.Infrastructure.Jobs
{
    public class ScheduleJobCommandHandler : ICommandHandler<ScheduleJobCommand>
    {
        private readonly IJobScheduler jobScheduler;

        public ScheduleJobCommandHandler(IJobScheduler jobScheduler)
        {
            this.jobScheduler = jobScheduler;
        }

        public Task HandleAsync(ScheduleJobCommand command, CancellationToken cancellationToken)
        {
            return jobScheduler.ScheduleJobAsync(new ExecuteCommandJob(command.Command), command.EnqueueAt);
        }
    }
}
