using Azure.Identity;
using Azure.Storage.Blobs;
using DocumentProcessing.Function.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SmartDoc.Models.APIModels.DocumentsModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentProcessing.Function.Services
{
    public class BlobServices : IBlobServices
    {
        private readonly BlobContainerClient _containerClient;
        public BlobServices(IConfiguration configuration,
            IHostEnvironment environment)
        {
            var accountName = configuration["AccountName"];
            var containerName = configuration["ContainerName"];
            var serviceUri = new Uri($"https://{accountName}.blob.core.windows.net");

            var credential = new DefaultAzureCredential(
                new DefaultAzureCredentialOptions
                {
                    ExcludeManagedIdentityCredential = environment.IsDevelopment()
                });

            var blobServiceClient = new BlobServiceClient(serviceUri, credential);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        }

        public async Task<Stream> DownloadDocument(DocumentProcessingMessage message, CancellationToken cancellationToken = default)
        {
            var blobClient =
          _containerClient.GetBlobClient(message.BlobName);

            var response = await blobClient.DownloadStreamingAsync(
                cancellationToken: cancellationToken);

            return response.Value.Content;
        }
    }
}
