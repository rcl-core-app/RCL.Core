using Azure.Storage.Blobs;

namespace RCL.Core.Azure.BlobStorage
{
    public interface IAzureBlobStorageService
    {
        Task<BlobContainerClient> CreateOrGetContainerAsync(string ContainerName, 
            ContainerType ContainerType);

        Task<int> DeleteContainerAsync(string ContainerName);

        Task<BlobClient> UploadBlobAsync(string ContainerName,
              ContainerType ContainerType, string BlobName,
              Stream Stream, string ContentType);

        Task<BlobClient> UploadBlobAsync(string ContainerName,
            ContainerType ContainerType, string BlobName,
            byte[] byteArray, string ContentType);

        string GetBlobSasUri(string ContainerName, string BlobName);

        string GetBlobUri(string ContainerName, string BlobName);

        Task<MemoryStream> DownloadBlobToStreamAsync(string ContainerName, 
            string BlobName);

        Task<byte[]> DownloadBlobToByteArrayAsync(string ContainerName, string BlobName);

        Task<string> ReadTextFromBlobAsync(string ContainerName, string BlobName);

        Task WriteTextToBlobAsync(string ContainerName, ContainerType ContainerType, 
            string BlobName, string Content);

        Task<int> DeleteBlobAsync(string ContainerName, string BlobName);
    }
}
