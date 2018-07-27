using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using MobCATOrNot.Models;
using Newtonsoft.Json;

[assembly: Xamarin.Forms.Dependency(typeof(MobCATOrNot.Services.CustomVisionAPIService))]

namespace MobCATOrNot.Services
{
    public class CustomVisionAPIService : ICustomVisionAPIService
    {
        private const string CustomVisionUrl = "https://southcentralus.api.cognitive.microsoft.com/customvision/v2.0/Training/projects";
 
        private HttpClient _client;

        public CustomVisionAPIService()
        {
            CreateHttpClient();
        }

        public async Task<List<CustomVisionTag>> GetTags()
        {
            var actionUrl = $"{CustomVisionUrl}/{Constants.CustomVisionProjectId}/tags";
            var response = await _client.GetAsync(actionUrl).ConfigureAwait(false);
            var result = await ValidateAndGetResult<List<CustomVisionTag>>(response);
            return result;
        }

        public async Task<CustomVisionTag> CreateTag(string tagName)
        {
            var actionUrl = $"{CustomVisionUrl}/{Constants.CustomVisionProjectId}/tags?name={tagName}";
            var response = await _client.PostAsync(actionUrl, null).ConfigureAwait(false);
            var result = await ValidateAndGetResult<CustomVisionTag>(response);
            return result;
        }

        public async Task<List<CustomVisionUploadedImage>> UploadImages(List<Stream> images, List<string> tagIds)
        {
            var tagsParam = string.Join(",", tagIds);
            var actionUrl = $"{CustomVisionUrl}/{Constants.CustomVisionProjectId}/images?tagIds={tagsParam}";
            using (var content = new MultipartFormDataContent())
            {
                images.Select(i => CreateFileContent(i, $"{Guid.NewGuid()}.png", "image/png"))
                      .ToList()
                      .ForEach(content.Add);

                var response = await _client.PostAsync(actionUrl, content).ConfigureAwait(false);
                var result = await ValidateAndGetResult<CustomVisionUploadImagesResult>(response);
                return result?.images;
            }
        }

        public async Task<CustomVisionIteration> TriggerIterationTraining()
        {
            var actionUrl = $"{CustomVisionUrl}/{Constants.CustomVisionProjectId}/train";
            var response = await _client.PostAsync(actionUrl, null).ConfigureAwait(false);
            var result = await ValidateAndGetResult<CustomVisionIteration>(response);
            return result;
        }

        public async Task<List<CustomVisionIteration>> GetIterations()
        {
            var actionUrl = $"{CustomVisionUrl}/{Constants.CustomVisionProjectId}/iterations";
            var response = await _client.GetAsync(actionUrl).ConfigureAwait(false);
            var result = await ValidateAndGetResult<List<CustomVisionIteration>>(response);
            return result;
        }

        public async Task<CustomVisionIterationExport> GetIterationModelExport(string iterationId, CustomVisionModelType type)
        {
            var actionUrl = $"{CustomVisionUrl}/{Constants.CustomVisionProjectId}/iterations/{iterationId}/export";
            var response = await _client.GetAsync(actionUrl).ConfigureAwait(false);
            var allExports = await ValidateAndGetResult<List<CustomVisionIterationExport>>(response);

            // Filter manually on the client side, seems to be a server issue not filtering the results
            var result = allExports?.Where(e => e.platform == type.ToString()).FirstOrDefault();
            return result;
        }


        private void CreateHttpClient()
        {
            if (_client != null)
                return;

            _client = new HttpClient();
            _client.DefaultRequestHeaders.TryAddWithoutValidation("Training-Key", Constants.CustomVisionTrainingKey);

        }

        private StreamContent CreateFileContent(Stream stream, string fileName, string contentType)
        {
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"files\"",
                FileName = "\"" + fileName + "\""
            }; 
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return fileContent;
        }

        private async Task<TResult> ValidateAndGetResult<TResult>(HttpResponseMessage response)
            where TResult : class
        {
            if (response == null)
                return null;

            var responseString = await response.Content?.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"{response.RequestMessage.Method} {response.RequestMessage.RequestUri}\n{response.StatusCode}: {responseString}");

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = $"Server returned [{response.StatusCode}] error";
                if (!string.IsNullOrWhiteSpace(responseString))
                {
                    var error = JsonConvert.DeserializeObject<CustomVisionErrorResponse>(responseString);
                    errorMessage = $"[{response.StatusCode}] {error?.message}";
                }

                throw new InvalidOperationException(errorMessage);
            }

            var result = JsonConvert.DeserializeObject<TResult>(responseString);
            return result;
        }

    }
}
