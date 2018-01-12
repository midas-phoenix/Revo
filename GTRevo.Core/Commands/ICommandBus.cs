﻿using System.Threading;
using System.Threading.Tasks;

namespace GTRevo.Core.Commands
{
    public interface ICommandBus
    {
        Task<TResult> SendAsync<TResult>(ICommand<TResult> command,
            CancellationToken cancellationToken = default(CancellationToken));
        Task SendAsync(ICommandBase command, CancellationToken cancellationToken = default(CancellationToken));
    }
}
