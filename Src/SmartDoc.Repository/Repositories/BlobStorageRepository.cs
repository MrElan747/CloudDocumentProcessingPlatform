using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SmartDoc.Models.APIModels.DocumentsModel;
using SmartDoc.Models.Enums;
using SmartDoc.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDoc.Services.Services
{
    public class BlobStorageRepository : IBlobStorageRepository
    {
        private readonly BlobContainerClient _containerClient;
        public BlobStorageRepository(
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            var accountName = configuration["AzureStorage:AccountName"];
            var containerName = configuration["AzureStorage:ContainerName"];
            var serviceUri = new Uri($"https://{accountName}.blob.core.windows.net");

            var credential = new DefaultAzureCredential(
                new DefaultAzureCredentialOptions
                {
                    ExcludeManagedIdentityCredential = environment.IsDevelopment()
                });

            var blobServiceClient = new BlobServiceClient(serviceUri, credential);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        }

        public async Task<BlobUploadedResponse> UploadFileAsync(UploadDocumentRequest request)
        {
            Guid docId = Guid.NewGuid();
            string fileName = $"{docId}-{request.File.FileName}";

            var blobName = $"{request.DocumentTypeEnum.ToString().ToLowerInvariant()}/{fileName}";

            var blobClient = _containerClient.GetBlobClient(blobName);

            await using var stream = request.File.OpenReadStream();

            await blobClient.UploadAsync(
                stream,
                overwrite: false);

            BlobProperties properties = await blobClient.GetPropertiesAsync();

            string contentType = properties.ContentType;
            long fileSize = properties.ContentLength;

            return new BlobUploadedResponse
            {
                DocumentId = docId,
                FileName = fileName,
                DocumentType = request.DocumentTypeEnum,
                ContainerName = _containerClient.Name,
                BlobPath = blobName,
                Status = "Uploaded",
                ContentType = contentType,
                Size = fileSize
            };
        }

        public async Task<bool> DeleteBlobAsync(string fileName)
        {
            BlobClient blobClient = _containerClient.GetBlobClient(fileName);
            return await blobClient.DeleteIfExistsAsync();
        }
    }
}
