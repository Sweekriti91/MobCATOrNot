using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace MobCATOrNot.Functions
{
    public static class MLMonitorNewModelFunction
    {
        [FunctionName("MLMonitorNewModel")]
        //CRON every second: * * * * * *
        public async static Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            const string visionAPIUrl = "https://southcentralus.api.cognitive.microsoft.com/customvision/v2.0";
            var iterationsAPI = $"{visionAPIUrl}/Training/projects/{Constants.CustomVisionProjectId}/iterations";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Training-Key", Constants.CustomVisionTrainingKey);
            var iterationsResponse = await client.GetStringAsync(iterationsAPI);
            var iterations = JsonConvert.DeserializeObject<VisionIterations>(iterationsResponse);
            var iteration = iterations?
                .Where(i => i.exportable)
                .OrderByDescending(i => i.created)
                .FirstOrDefault();

            if (iteration == null || iteration.isDefault)
            {
                log.Info($"No (new) iterations for the model found. Finishing...");
                return;
            }

            // TODO: check if build has been started already in order not to queue multiple builds
            // ...

            log.Info($"Non-default iteration [{iteration.id}] has been detected, starting a new build...");

            var vstsAPIUrl = $"https://{FunctionsConstants.VSTSTeam}.visualstudio.com/{FunctionsConstants.VSTSProject}/_apis";
            var queueBuildAPI = $"{vstsAPIUrl}/build/builds?api-version=4.1";
            var vstsClient = new HttpClient();
            vstsClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", FunctionsConstants.VSTSToken);
            var queuePayloadObject = new { definition = new { id = FunctionsConstants.VSTSBuildId } };
            var queuePayloadString = JsonConvert.SerializeObject(queuePayloadObject);
            var queueContent = new StringContent(queuePayloadString, Encoding.UTF8, "application/json");
            var queueResponse = await vstsClient.PostAsync(queueBuildAPI, queueContent);
            if (!queueResponse.IsSuccessStatusCode)
            {
                log.Info($"Unable to queue a build. Finishing...");
                return;
            }

            // TODO: implement marking as default as a separate azure function to be called from the build definition once upon completion
            log.Info($"Build for the iteration [{iteration.id}] has been started. Marking the iteration as default...");

            var patchIterationAPI = $"{visionAPIUrl}/Training/projects/{Constants.CustomVisionProjectId}/iterations/{iteration.id}";
            var patchRequest = new HttpRequestMessage(new HttpMethod("PATCH"), patchIterationAPI);
            var payloadObject = new { isDefault = true };
            var payloadString = JsonConvert.SerializeObject(payloadObject);
            patchRequest.Content = new StringContent(payloadString, Encoding.UTF8, "application/json");
            var patchResponse = await client.SendAsync(patchRequest);

            if (!patchResponse.IsSuccessStatusCode)
            {
                log.Info($"Unable to patch iteration to make it default. Finishing...");
                return;
            }
        }
    }
}
