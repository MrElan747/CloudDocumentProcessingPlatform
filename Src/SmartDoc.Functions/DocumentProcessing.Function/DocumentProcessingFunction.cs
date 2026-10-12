using Azure.Messaging.ServiceBus;
using DocumentProcessing.Function.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartDoc.Models.APIModels.DocumentsModel;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace DocumentProcessing.Function;

public class DocumentProcessingFunction
{
    private readonly ILogger<DocumentProcessingFunction> _logger;
    private readonly IDocumentProcessingService _documentProcessingService;
    public DocumentProcessingFunction(ILogger<DocumentProcessingFunction> logger, IDocumentProcessingService documentProcessingService)
    {
        _logger = logger;
        _documentProcessingService = documentProcessingService;
    }

    [Function(nameof(DocumentProcessingFunction))]
    public async Task Run(
        [ServiceBusTrigger("%ServiceBusQueueName%", Connection = "ServiceBusConnection", AutoCompleteMessages = false)]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);
        _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        var messageBody = message.Body.ToString();

        var documentMessage =
            JsonSerializer.Deserialize<DocumentProcessingMessage>(messageBody);

        if (documentMessage == null)
        {
            _logger.LogError("Invalid document processing message. MessageId: {MessageId}", message.MessageId);
            throw new InvalidOperationException("Invalid document processing message.");
        }

        await _documentProcessingService.ProcessAsync(documentMessage);

        // Complete the message
        await messageActions.CompleteMessageAsync(message);
    }
}