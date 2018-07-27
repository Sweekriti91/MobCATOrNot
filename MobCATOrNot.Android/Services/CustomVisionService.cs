using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Java.IO;
using Java.Util.Zip;
using MobCATOrNot.Models;
using MobCATOrNot.Services;
using Org.Tensorflow.Contrib.Android;
using Xamarin.Forms;

[assembly: Dependency(typeof(MobCATOrNot.Droid.CustomVisionService))]

namespace MobCATOrNot.Droid
{
    public class CustomVisionService : ICustomVisionService
    {
        private List<string> _loadedLabels;
        private TensorFlowInferenceInterface inferenceInterface;
        string unZipLocation = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/unzip/";

        public Task LoadModel()
        {
            if (_loadedLabels != null)
                return Task.FromResult(true);

            return Task.Run(() =>
            {
                //loading model and labels
                var assets = Android.App.Application.Context.Assets;
                inferenceInterface = new TensorFlowInferenceInterface(assets, "model.pb");

                using (var sr = new StreamReader(assets.Open("labels.txt")))
                {
                    var content = sr.ReadToEnd();
                    _loadedLabels = content.Split('\n').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
                }
            });
        }

        public Task LoadModelFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));
            return Task.Run(async () =>
            {
                // HACK: unzip model in azure indead of device and get the stream
                url = $"{Constants.AzureFunctionsUrl}/MLExport?platform=tensorflow&format=binary&code={Constants.AzureFunctionsCode}";

                // TODO: we still need labels

                var client = new HttpClient();
                var rawModelBytes = await client.GetByteArrayAsync("https://mobcatnotmobcat.azurewebsites.net/api/MLExport?platform=tensorflow&format=binary&code=rLzmW4glo3/KUahNnEHlI/fngVrlzQJ/9y4YIIHS6yHAtNKNHiCm0w==");
                Stream zipModelStream = new MemoryStream(rawModelBytes);
                inferenceInterface = new TensorFlowInferenceInterface(zipModelStream);

                var assets = Android.App.Application.Context.Assets;
                using (var sr = new StreamReader(assets.Open("labels2.txt")))
                {
                    var content = sr.ReadToEnd();
                    _loadedLabels = content.Split('\n').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
                }
            });

        }

        void DirChecker(String dir)
        {
            var file = new Java.IO.File(unZipLocation + dir);

            if (!file.IsDirectory)
            {
                file.Mkdirs();
            }
        }

        static readonly int InputSize = 227;
        static readonly string InputName = "Placeholder";
        static readonly string OutputName = "loss";

        public async Task<PredictionsResult> GetPredictions(PredictionsRequest request)
        {
            if (request?.Image == null)
                return null;

            await LoadModel();

            var outputNames = new[] { OutputName };
            var floatValues = GetBitmapPixels(request.Image);
            var outputs = new float[_loadedLabels.Count];

            inferenceInterface.Feed(InputName, floatValues, 1, InputSize, InputSize, 3);
            inferenceInterface.Run(outputNames);
            inferenceInterface.Fetch(OutputName, outputs);

            var results = new List<Prediction>();
            for (var i = 0; i < outputs.Length; ++i)
            {
                results.Add(new Prediction
                {
                    Label = _loadedLabels[i],
                    Probability = outputs[i]
                });
            }

            return new PredictionsResult
            {
                Predictions = results,
            };
        }

        public Task StartModelTraining()
        {
            throw new NotImplementedException();
        }

        public Task SubmitImages(SubmitImagesRequest request)
        {
            throw new NotImplementedException();
        }

        private float[] GetBitmapPixels(Stream image)
        {
            image.Seek(0, SeekOrigin.Begin);
            Bitmap bitmap = BitmapFactory.DecodeStream(image);
            var floatValues = new float[227 * 227 * 3];

            using (var scaledBitmap = Bitmap.CreateScaledBitmap(bitmap, 227, 227, false))
            {
                using (var resizedBitmap = scaledBitmap.Copy(Bitmap.Config.Argb8888, false))
                {
                    var intValues = new int[227 * 227];
                    resizedBitmap.GetPixels(intValues, 0, resizedBitmap.Width, 0, 0, resizedBitmap.Width, resizedBitmap.Height);

                    for (int i = 0; i < intValues.Length; ++i)
                    {
                        var val = intValues[i];

                        floatValues[i * 3 + 0] = ((val & 0xFF) - 104);
                        floatValues[i * 3 + 1] = (((val >> 8) & 0xFF) - 117);
                        floatValues[i * 3 + 2] = (((val >> 16) & 0xFF) - 123);
                    }

                    resizedBitmap.Recycle();
                }

                scaledBitmap.Recycle();
            }

            return floatValues;
        }
    }
}
