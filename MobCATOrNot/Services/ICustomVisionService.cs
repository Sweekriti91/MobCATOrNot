using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MobCATOrNot.Models;

namespace MobCATOrNot.Services
{
    public interface ICustomVisionService
    {
        Task LoadModel();

        Task LoadModelFromUrl(string url);

        Task<PredictionsResult> GetPredictions(PredictionsRequest request);

        Task SubmitImages(SubmitImagesRequest request);

        Task StartModelTraining();
    }
}
