using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace MobCATOrNot.Functions
{
    public static class MLExportFunction
    {
        [FunctionName("MLExport")]
        public async static Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req, TraceWriter log)
        {
            var platform = req.Query["platform"];
            if (string.IsNullOrWhiteSpace(platform))
                return new BadRequestObjectResult("Please specify the 'platform' url parameter, either 'CoreML' or 'TensorFlow'");

            // TODO: move to a data access layer
            var visionAPIUrl = $"https://southcentralus.api.cognitive.microsoft.com/customvision/v2.0";
            var iterationsAPI = $"{visionAPIUrl}/Training/projects/{Constants.CustomVisionProjectId}/iterations";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Training-Key", Constants.CustomVisionTrainingKey);
            var iterationsResponse = await client.GetStringAsync(iterationsAPI);
            var iterations = JsonConvert.DeserializeObject<VisionIterations>(iterationsResponse);
            var iteration = iterations?
                //.Where(i => i.exportable && i.isDefault)
                .OrderByDescending(i => i.created)
                .FirstOrDefault();

            if (iteration == null || !iteration.exportable)
                return new BadRequestObjectResult("No exportable iterations were found");

            var iterationExportAPI = $"{visionAPIUrl}/Training/projects/{Constants.CustomVisionProjectId}/iterations/{iteration.id}/export?platform={platform}";
            var iterationExportsResponse = await client.GetStringAsync(iterationExportAPI);
            var iterationExports = JsonConvert.DeserializeObject<VisionIterationExports>(iterationExportsResponse);
            var iterationExport = iterationExports?
                .Where(i => string.Equals(i.platform, platform, System.StringComparison.InvariantCultureIgnoreCase) 
                    && string.Equals(i.status, "Done", System.StringComparison.InvariantCultureIgnoreCase) 
                    && !string.IsNullOrWhiteSpace(i.downloadUri))
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(iterationExport?.downloadUri))
                return new BadRequestObjectResult($"An exportable iteration was found but no exports for the [{platform}] platform is available. Try to trigger a manual export via web interface first");

            var format = req.Query["format"];
            if (string.Equals(format, "binary", System.StringComparison.InvariantCultureIgnoreCase))
            {
                var modelStream = await client.GetStreamAsync(iterationExport.downloadUri);
                if (string.Equals(platform, "tensorflow", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    var zipArchive = new System.IO.Compression.ZipArchive(modelStream);
                    var pbFile = zipArchive.Entries.Where(e => e.Name.EndsWith(".pb")).FirstOrDefault();
                    if (pbFile != null)
                    {
                        var pbFileStream = pbFile.Open();
                        var unzippedModelStream = new MemoryStream();
                        pbFileStream.CopyTo(unzippedModelStream);
                        unzippedModelStream.Seek(0, SeekOrigin.Begin);
                        return new OkObjectResult(unzippedModelStream);
                    }
                }

                return new OkObjectResult(modelStream);
            }

            var result = new OkObjectResult(iterationExport.downloadUri);
            return result;
        }
    }
}
