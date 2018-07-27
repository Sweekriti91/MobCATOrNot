using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using MobCATOrNot.Models;

namespace MobCATOrNot.Services
{
    public interface ICustomVisionAPIService
    {
        Task<CustomVisionTag> CreateTag(string tagName);
        Task<List<CustomVisionTag>> GetTags();
        Task<List<CustomVisionUploadedImage>> UploadImages(List<Stream> images, List<string> tagIds);
        Task<CustomVisionIteration> TriggerIterationTraining();
        Task<List<CustomVisionIteration>> GetIterations();
        Task<CustomVisionIterationExport> GetIterationModelExport(string iterationId, CustomVisionModelType type);
    }
}
