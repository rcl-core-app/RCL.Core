#nullable disable

using Microsoft.Extensions.Options;
using System.Diagnostics;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Azure.Storage;
using System.Text;

namespace RCL.Core.Azure.BlobStorage
{
    internal class AzureBlobStorageService : IAzureBlobStorageService
    {
        private readonly IOptions<AzureBlobStorageOptions> _options;

        private static BlobServiceClient _blobServiceClient;

        public AzureBlobStorageService(IOptions<AzureBlobStorageOptions> options)
        {
            try
            {
                _options = options;
                _blobServiceClient = new BlobServiceClient(_options.Value.ConnectionString);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        #region Containers


        public async Task<BlobContainerClient> CreateOrGetContainerAsync(string ContainerName, ContainerType ContainerType)
        {
            try
            {
                BlobContainerClient container = _blobServiceClient.GetBlobContainerClient(ContainerName);

                if (ContainerType == ContainerType.Public)
                {
                   await container.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);
                }
                else
                {
                    await container.CreateIfNotExistsAsync();
                }

                await container.CreateIfNotExistsAsync();

                return container;
            }

            catch (Exception ex)
            {
                throw new Exception($"Could not create blob container, {ex.Message}");
            }
        }

        public async Task<int> DeleteContainerAsync(string ContainerName)
        {
            try
            {
                BlobContainerClient existingContainer = _blobServiceClient.GetBlobContainerClient(ContainerName);

                if(existingContainer != null)
                {
                    await existingContainer.DeleteIfExistsAsync();
                }

                return 1;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot delete blob container, {ex.Message}");
            }
        }

        #endregion

        #region Blobs

        public async Task<BlobClient> UploadBlobAsync(string ContainerName, 
            ContainerType ContainerType, string BlobName, Stream Stream, string ContentType)
        {
            try
            {
                BlobContainerClient container = await CreateOrGetContainerAsync(ContainerName, ContainerType);
                BlobClient blobClient = container.GetBlobClient(BlobName);
                using Stream uploadStream = Stream;
                BlobHttpHeaders blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = ContentType
                };

                if(ContainerType != ContainerType.Private)
                {
                }

                await blobClient.UploadAsync(content: uploadStream, httpHeaders: blobHttpHeaders);
                uploadStream.Close();

                return blobClient;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot upload blob, {ex.Message}");
            }
        }

        public async Task<BlobClient> UploadBlobAsync(string ContainerName,
            ContainerType ContainerType, string BlobName, byte[] byteArray, string ContentType)
        {
            try
            {
                BlobContainerClient container = await CreateOrGetContainerAsync(ContainerName, ContainerType);
                BlobClient blobClient = container.GetBlobClient(BlobName);
                using Stream uploadStream = new MemoryStream(byteArray);
                BlobHttpHeaders blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = ContentType
                };

                if (ContainerType != ContainerType.Private)
                {
                }

                await blobClient.UploadAsync(content: uploadStream, httpHeaders: blobHttpHeaders);
                uploadStream.Close();

                return blobClient;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot upload blob, {ex.Message}");
            }
        }

        public async Task<MemoryStream> DownloadBlobToStreamAsync(string ContainerName, string BlobName)
        {
            try
            {
                BlobContainerClient container = _blobServiceClient.GetBlobContainerClient(ContainerName);
                BlobClient blobClient = container.GetBlobClient(BlobName);
                MemoryStream stream = new MemoryStream();
                await blobClient.DownloadToAsync(stream);
                stream.Position = 0;
                return stream;
            }
            catch(Exception ex)
            {
                throw new Exception($"Cannot download blob to stream, {ex.Message}");
            }
        }

        public async Task<byte[]> DownloadBlobToByteArrayAsync(string ContainerName, string BlobName)
        {
            try
            {
                BlobContainerClient container = _blobServiceClient.GetBlobContainerClient(ContainerName);
                BlobClient blobClient = container.GetBlobClient(BlobName);
                
                if (await blobClient.ExistsAsync())
                {
                    using (var ms = new MemoryStream())
                    {
                        blobClient.DownloadTo(ms);
                        return ms.ToArray();
                    }
                }
                return new byte[0];
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot download blob to byte array, {ex.Message}");
            }
        }

        public async Task<string> ReadTextFromBlobAsync(string ContainerName, string BlobName)
        {
            try
            {
                MemoryStream ms = await DownloadBlobToStreamAsync(ContainerName, BlobName);
                return Encoding.ASCII.GetString(ms.ToArray());
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task WriteTextToBlobAsync(string ContainerName, ContainerType ContainerType, string BlobName, string Content)
        {
            try
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(Content);
                Stream stream = new MemoryStream(byteArray);
                await UploadBlobAsync(ContainerName, ContainerType, BlobName, stream, "text/plain");
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot write text to blob, {ex.Message}");
            }
        }

        public async Task<int> DeleteBlobAsync(string ContainerName, string BlobName)
        {
            try
            {
                BlobContainerClient container = _blobServiceClient.GetBlobContainerClient(ContainerName);
                BlobClient blobClient = container.GetBlobClient(BlobName);
                await blobClient.DeleteIfExistsAsync();
                return 1;
            }
            catch(Exception ex)
            {
                throw new Exception($"Cannot download blob, {ex.Message}");
            }
        }

        public string GetBlobSasUri(string ContainerName, string BlobName)
        {
            try
            {
                string uri = string.Empty;

                BlobContainerClient container = _blobServiceClient.GetBlobContainerClient(ContainerName);
                BlobClient blobClient = container.GetBlobClient(BlobName);

                string accountName = StringOperations
                    .GetValueFromConnectionString(_options.Value.ConnectionString,";", "AccountName");
                string accountKey = StringOperations
                   .GetValueFromConnectionString(_options.Value.ConnectionString, ";", "AccountKey");
                StorageSharedKeyCredential credential = new StorageSharedKeyCredential(accountName,accountKey);
              
                BlobClient sasBlobClient = new BlobClient(blobClient.Uri, credential);

                if(sasBlobClient.CanGenerateSasUri)
                {
                    BlobSasBuilder sasBuilder = new BlobSasBuilder()
                    {
                        BlobContainerName = ContainerName,
                        BlobName = BlobName,
                        Resource = "b"
                    };

                    sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
                    sasBuilder.SetPermissions(BlobSasPermissions.Read);

                    Uri sasUri = sasBlobClient.GenerateSasUri(sasBuilder);
                    uri = sasUri.ToString();
                }

                return uri;
            }
            catch(Exception ex)
            {
                throw new Exception($"Cannot get blob sas uri, {ex.Message}");
            }
        }

        public string GetBlobUri(string ContainerName, string BlobName)
        {
            try
            {
                string uri = string.Empty;

                BlobContainerClient container = _blobServiceClient.GetBlobContainerClient(ContainerName);
                BlobClient blobClient = container.GetBlobClient(BlobName);
                uri = blobClient.Uri.ToString();
               
                return uri;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot get blob sas uri, {ex.Message}");
            }
        }
    }

    #endregion
}
