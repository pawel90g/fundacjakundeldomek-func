using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Identity;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;

namespace FundacjaKundelDomek;

public class KundelDomekFunc
{
    private const string StorageName = "kundeldomekresources";
    private const string GalleryContainerName = "gallery";
    private const string CharityContainerName = "charity";
    private const string ForAdoptionContainerName = "for-adoption";


    private static readonly Uri storageUri = new($"https://{StorageName}.blob.core.windows.net");

    private readonly ILogger<KundelDomekFunc> _logger;

    public KundelDomekFunc(ILogger<KundelDomekFunc> logger)
    {
        _logger = logger;
    }

    [Function("ListGallery")]
    public IActionResult ListGallery(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req) => new OkObjectResult(ListBlobs(GalleryContainerName));

    [Function("GetCharityData")]
    public IActionResult GetCharityData(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        var data = GetBlob(CharityContainerName, "charity.json");
        var deserialised = DeserializeArrayJson<Charity>(data);

        var response = new List<Charity>();

        foreach (var item in deserialised)
        {
            if (!item.Visible) continue;
            item.Photo = BuildBlobUrl(CharityContainerName, item.Photo);
            response.Add(item);
        }

        return new OkObjectResult(response);
    }

    [Function("GetForAdoptionData")]
    public IActionResult GetForAdoptionData(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        var data = GetBlob(ForAdoptionContainerName, "for-adoption.json");
        var deserialised = DeserializeArrayJson<ForAdoption>(data);

        var response = new List<ForAdoption>();

        foreach (var item in deserialised)
        {
            if (item.Expired) continue;

            var photos = new List<string>();
            foreach (var p in item.Photos)
            {
                photos.Add(BuildBlobUrl(ForAdoptionContainerName, p));
            }
            item.Photos = photos;

            response.Add(item);
        }

        return new OkObjectResult(response);
    }

    private Uri GetContainerUri(string container) =>
        new($"{storageUri}{container}");

    private BlobServiceClient GetManagedBlobServiceClient() =>
        new(storageUri, credential: new ManagedIdentityCredential());

    private BlobContainerClient GetContainerClient(string container) =>
        GetManagedBlobServiceClient().GetBlobContainerClient(container);

    private List<string> ListBlobs(string container)
    {
        var blobs = GetContainerClient(container).GetBlobs();

        var blobsNamesList = new List<string>();

        foreach (var blob in blobs)
            blobsNamesList.Add(BuildBlobUrl(container, blob.Name));

        return blobsNamesList;
    }

    private string GetBlob(string container, string blob)
    {
        var content = GetContainerClient(container).GetBlobClient(blob).DownloadContent();
        var binaryData = content.Value.Content;
        return Encoding.UTF8.GetString(binaryData);
    }

    private string BuildBlobUrl(string container, string blob) =>
        $"{GetContainerUri(container)}/{blob}";

    private T DeserializeObjectJson<T>(string json) => JsonSerializer.Deserialize<T>(json);
    private List<T> DeserializeArrayJson<T>(string json) => JsonSerializer.Deserialize<List<T>>(json);

}
