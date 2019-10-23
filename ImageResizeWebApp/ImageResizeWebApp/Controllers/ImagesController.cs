using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ImageResizeWebApp.Models;
using Microsoft.Extensions.Options;
using System.IO;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using ImageResizeWebApp.Helpers;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace ImageResizeWebApp.Controllers
{
    [Route("api/[controller]")]
    public class ImagesController : Controller
    {
        // make sure that appsettings.json is filled with the necessary details of the azure storage
        private readonly AzureStorageConfig storageConfig = null;
        const string subscriptionKey = "23acf09bf8b94de483f81134a75233e4";

        const string uriBase =
            "/face/v1.0/detect";

        private string latestImage = "";

        public ImagesController(IOptions<AzureStorageConfig> config)
        {
            storageConfig = config.Value;
        }

        // POST /api/images/upload
        [HttpPost("[action]")]
        public async Task<IActionResult> Upload(ICollection<IFormFile> files)
        {
            bool isUploaded = false;

            try
            {

                if (files.Count == 0)

                    return BadRequest("No files received from the upload");

                if (storageConfig.AccountKey == string.Empty || storageConfig.AccountName == string.Empty)

                    return BadRequest("sorry, can't retrieve your azure storage details from appsettings.js, make sure that you add azure storage details there");

                if (storageConfig.ImageContainer == string.Empty)

                    return BadRequest("Please provide a name for your image container in the azure blob storage");

                foreach (var formFile in files)
                {
                    if (StorageHelper.IsImage(formFile))
                    {
                        if (formFile.Length > 0)
                        {
                            using (Stream stream = formFile.OpenReadStream())
                            {
                                isUploaded = await StorageHelper.UploadFileToStorage(stream, formFile.FileName, storageConfig);
                            }
                        }
                    }
                    else
                    {
                        return new UnsupportedMediaTypeResult();
                    }
                }

                if (isUploaded)
                {
                    if (storageConfig.ThumbnailContainer != string.Empty)

                        return new AcceptedAtActionResult("GetThumbNails", "Images", null, null);

                    else

                        return new AcceptedResult();
                }
                else

                    return BadRequest("Look like the image couldnt upload to the storage");


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET /api/images/thumbnails
        [HttpGet("thumbnails")]
        public async Task<IActionResult> GetThumbNails()
        {

            try
            {
                if (storageConfig.AccountKey == string.Empty || storageConfig.AccountName == string.Empty)

                    return BadRequest("sorry, can't retrieve your azure storage details from appsettings.js, make sure that you add azure storage details there");

                if (storageConfig.ImageContainer == string.Empty)

                    return BadRequest("Please provide a name for your image container in the azure blob storage");

                List<string> thumbnailUrls = await StorageHelper.GetThumbNailUrls(storageConfig);

                return new ObjectResult(thumbnailUrls);
            
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        



       private async Task<string> GetTGetLastImagehumbNail()
        {

            try
            {
                if (storageConfig.AccountKey == string.Empty || storageConfig.AccountName == string.Empty)
                    return "Nu ai account key";



                if (storageConfig.ImageContainer == string.Empty)

                    return "Nu ai image container";

                List<string> thumbnailUrls = await StorageHelper.GetThumbNailUrls(storageConfig);
                
                return thumbnailUrls.LastOrDefault();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message).ToString();
            }

        }

        [HttpGet("lastemotion")]
        public async Task<string> GetLatestImageEmotion()
        {

            try
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", storageConfig.EmotionApiKey);

                // Request parameters. A third optional parameter is "details".
                string requestParameters = "returnFaceId=true&returnFaceLandmarks=false" +
                                           "&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses," +
                                           "emotion";

                // Assemble the URI for the REST API Call.
                string uri = storageConfig.EmotionApiEndpoint + uriBase + "?" + requestParameters;

                HttpResponseMessage response;
                var url = GetTGetLastImagehumbNail();
                // Request body. Posts a locally stored JPEG image.
                byte[] byteData = GetImageAsByteArray(url.Result);

                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This example uses content type "application/octet-stream".
                    // The other content types you can use are "application/json"
                    // and "multipart/form-data".
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");

                    // Execute the REST API call.
                    response = await client.PostAsync(uri, content);

                    // Get the JSON response.
                    string contentString = await response.Content.ReadAsStringAsync();

                    return contentString;
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message).ToString();
            }

        }

        byte[] GetImageAsByteArray(string imageFilePath)
        {
            var webClient = new WebClient();
             var  imageBytes = webClient.DownloadData(imageFilePath);

            return imageBytes;
        }

    }
}