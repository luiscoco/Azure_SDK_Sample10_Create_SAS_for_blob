using System.Text;
using Azure.Core;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;

string accountName = "luisstorageaccount1974";
string accountKey = "C02Nnhafwm7Xh5d99zADxBoKMJLNizpa8A1F4Crk7rWTYTkQPdlbslueH5aSAUD/Uq3l2tms5VAw+ASthFPudw==";
StorageSharedKeyCredential storageSharedKeyCredential = new(accountName, accountKey);
BlobServiceClient blobServiceClient = new BlobServiceClient(new Uri($"https://{accountName}.blob.core.windows.net"), storageSharedKeyCredential);

// Create a Uri object with a service SAS appended
BlobClient blobClient = blobServiceClient 
    .GetBlobContainerClient("mynewblob1999")
    .GetBlobClient("sample-blob.txt");
Uri blobSASURI = await CreateServiceSASBlob(blobClient);

// Create a blob client object representing 'sample-blob.txt' with SAS authorization
BlobClient blobClientSAS = new BlobClient(blobSASURI);

BlobProperties properties = await blobClientSAS.GetPropertiesAsync();
Console.WriteLine($"Blob content type: {properties.ContentType}, Size: {properties.ContentLength} bytes");

BlobDownloadInfo blobDownloadInfo = await blobClientSAS.DownloadAsync();
string content = await new StreamReader(blobDownloadInfo.Content).ReadToEndAsync();
Console.WriteLine($"Downloaded blob content: {content}");

byte[] newBlobContent = Encoding.UTF8.GetBytes("New content for the blob.");
using (MemoryStream stream = new MemoryStream(newBlobContent))
{
    await blobClientSAS.UploadAsync(stream, true);
    Console.WriteLine("Uploaded new version of the blob.");
}


static async Task<Uri> CreateServiceSASBlob(BlobClient blobClient, string storedPolicyName = null)
{
    // Check if BlobContainerClient object has been authorized with Shared Key
    if (blobClient.CanGenerateSasUri)
    {
        // Create a SAS token that's valid for one day
        BlobSasBuilder sasBuilder = new BlobSasBuilder()
        {
            BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
            BlobName = blobClient.Name,
            Resource = "b"
        };

        if (storedPolicyName == null)
        {
            sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddDays(1);
            sasBuilder.SetPermissions(BlobContainerSasPermissions.Read | BlobContainerSasPermissions.Write);
        }
        else
        {
            sasBuilder.Identifier = storedPolicyName;
        }

        Uri sasURI = blobClient.GenerateSasUri(sasBuilder);

        return sasURI;
    }
    else
    {
        // Client object is not authorized via Shared Key
        return null;
    }
}