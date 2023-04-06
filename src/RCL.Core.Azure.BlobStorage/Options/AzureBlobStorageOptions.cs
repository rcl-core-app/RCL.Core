#nullable disable

namespace RCL.Core.Azure.BlobStorage
{
    public class AzureBlobStorageOptions
    {
        public AzureBlobStorageOptions()
        {
        }

        public AzureBlobStorageOptions(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; set; }
    }
}
