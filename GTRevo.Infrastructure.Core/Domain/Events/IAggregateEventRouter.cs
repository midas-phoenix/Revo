﻿using System;
using System.Collections.Generic;

namespace GTRevo.Infrastructure.Core.Domain.Events
{
    public interface IAggregateEventRouter
    {
        IReadOnlyCollection<DomainAggregateEvent> UncommitedEvents { get; }

        void ApplyEvent<T>(T evt) where T : DomainAggregateEvent;
        void CommitEvents();
        void Register(Type eventType, Action<DomainAggregateEvent> handler);
        void Register<T>(Action<DomainAggregateEvent> handler) where T : DomainAggregateEvent;
        void ReplayEvents(IEnumerable<DomainAggregateEvent> events);
    }
}