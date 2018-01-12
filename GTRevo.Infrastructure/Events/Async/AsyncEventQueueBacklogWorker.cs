﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Core;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Core.Domain.Events;
using NLog;

namespace GTRevo.Infrastructure.Events.Async
{
    public class AsyncEventQueueBacklogWorker : IAsyncEventQueueBacklogWorker
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IAsyncEventQueueManager asyncEventQueueManager;
        private readonly IServiceLocator serviceLocator;

        public AsyncEventQueueBacklogWorker(IAsyncEventQueueManager asyncEventQueueManager,
            IServiceLocator serviceLocator)
        {
            this.asyncEventQueueManager = asyncEventQueueManager;
            this.serviceLocator = serviceLocator;
        }

        public async Task RunQueueBacklogAsync(string queueName)
        {
            IAsyncEventQueueState queue = await asyncEventQueueManager.GetQueueStateAsync(queueName);
            if (queue == null)
            {
                return;
            }

            IReadOnlyCollection<IAsyncEventQueueRecord> records = await asyncEventQueueManager.GetQueueEventsAsync(queueName);

            if (records.Count == 0)
            {
                return;
            }

            bool processOnlyNonsequential = false;

            var firstSequential = records.FirstOrDefault(x => x.SequenceNumber != null);
            long? firstSequenceNumber = firstSequential?.SequenceNumber;
            if (firstSequenceNumber != null
                && queue.LastSequenceNumberProcessed != null
                && queue.LastSequenceNumberProcessed != firstSequenceNumber.Value - 1)
            {
                bool anyNonsequential = records.Any(x => x.SequenceNumber == null);
                if (anyNonsequential)
                {
                    Logger.Debug($"Processing only non-sequential async events in {queueName} queue: missing some events in sequence at #{queue.LastSequenceNumberProcessed.Value + 1}");
                    processOnlyNonsequential = true;
                }
                else
                {
                    string error = $"Skipping processing of {queueName} async event queue: missing events in sequence at #{queue.LastSequenceNumberProcessed.Value + 1}";
                    Logger.Warn(error);
                    throw new AsyncEventProcessingSequenceException(error, queue.LastSequenceNumberProcessed.Value + 1);
                }
            }

            var nonSequential = records.Where(x => x.SequenceNumber == null).ToList();
            if (nonSequential.Count > 0)
            {
                await ProcessEventsAsync(nonSequential, queueName);
            }

            if (!processOnlyNonsequential)
            {
                var sequential = records.Where(x => x.SequenceNumber != null).ToList();
                if (sequential.Count > 0)
                {
                    await ProcessEventsAsync(sequential, queueName);
                }
            }

            if (processOnlyNonsequential)
            {
                throw new AsyncEventProcessingSequenceException(
                    $"Processed only non-sequential async events in {queueName} queue: missing some events in sequence at #{queue.LastSequenceNumberProcessed.Value + 1}",
                    queue.LastSequenceNumberProcessed.Value + 1);
            }
        }

        private async Task ProcessEventsAsync(IReadOnlyCollection<IAsyncEventQueueRecord> records, string queueName)
        {
            HashSet<IAsyncEventListener> usedListeners = new HashSet<IAsyncEventListener>();
            List<IAsyncEventQueueRecord> processedEvents = new List<IAsyncEventQueueRecord>();

            foreach (var record in records)
            {
                Type listenerType = typeof(IAsyncEventListener<>).MakeGenericType(record.EventMessage.Event.GetType());
                var handleAsyncMethod = listenerType.GetMethod(nameof(IAsyncEventListener<IEvent>.HandleAsync));
                
                IEnumerable<IAsyncEventListener> listeners = serviceLocator.GetAll(listenerType).Cast<IAsyncEventListener>();
                foreach (IAsyncEventListener listener in listeners)
                {
                    IAsyncEventSequencer sequencer = listener.EventSequencer;
                    IEnumerable<EventSequencing> sequences = sequencer.GetEventSequencing(record.EventMessage);
                    if (sequences.Any(x => x.SequenceName == queueName))
                    {
                        try
                        {
                            await (Task)handleAsyncMethod.Invoke(listener, new object[] { record.EventMessage, queueName });
                            usedListeners.Add(listener);
                        }
                        catch (Exception e)
                        {
                            string error = $"Failed processing of an async event {record.EventMessage.GetType().FullName} (ID: {record.EventId}) in queue '{queueName}' with listener {listener.GetType().FullName}";
                            Logger.Error(e, error);
                            throw new AsyncEventProcessingException(error, e);
                        }
                    }
                }

                processedEvents.Add(record);
            }

            foreach (IAsyncEventListener listener in usedListeners)
            {
                try
                {
                    await listener.OnFinishedEventQueueAsync(queueName);
                }
                catch (Exception e)
                {
                    string error = $"Failed to finish processing of an async event queue '{queueName}' with listener {listener.GetType().FullName}";
                    Logger.Error(e, error);
                    throw new AsyncEventProcessingException(error, e);
                }
            }

            foreach (var processedEvent in processedEvents)
            {
                await asyncEventQueueManager.DequeueEventAsync(processedEvent.Id);
            }

            await asyncEventQueueManager.CommitAsync();
        }
    }
}
