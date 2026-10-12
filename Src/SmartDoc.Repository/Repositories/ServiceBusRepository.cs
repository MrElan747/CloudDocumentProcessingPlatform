using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using SmartDoc.Models.APIModels.DocumentsModel;
using SmartDoc.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace SmartDoc.Repository.Repositories
{
    public class ServiceBusRepository : IServiceBusRepository
    {
        private readonly ServiceBusSender _sender;
        public ServiceBusRepository(IConfiguration configuration)
        {
            var connectionString = configuration["ServiceBus:ConnectionString"];
            var queueName = configuration["ServiceBus:QueueName"];
            var clientOptions = new ServiceBusClientOptions
            {
                RetryOptions = new ServiceBusRetryOptions
                {
                    Mode = ServiceBusRetryMode.Exponential,
                    MaxRetries = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    MaxDelay = TimeSpan.FromSeconds(5)
                }
            };
            var client = new ServiceBusClient(connectionString, clientOptions);
            _sender = client.CreateSender(queueName);
        }
        public async Task SendDocumentProcessingMessageAsync(DocumentProcessingMessage message)
        {
            var json = JsonSerializer.Serialize(message);

            var serviceBusMessage = new ServiceBusMessage(json);

            await _sender.SendMessageAsync(serviceBusMessage);
        }
    }
}
