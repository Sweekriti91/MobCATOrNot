using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CoreGraphics;
using CoreML;
using CoreVideo;
using Foundation;
using MobCATOrNot.Models;
using MobCATOrNot.Services;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(MobCATOrNot.iOS.Services.CustomVisionService))]

namespace MobCATOrNot.iOS.Services
{
    public class CustomVisionService : ICustomVisionService
    {
        private MLModel _loadedModel;

        public Task LoadModel()
        {
            if (_loadedModel != null)
                return Task.FromResult(true);

            return Task.Run(() =>
            {
                var bundle = NSBundle.MainBundle;
                var rawModelUrl = bundle.GetUrlForResource("MobCatOrNot", "mlmodel");
                _loadedModel = LoadModelInternal(rawModelUrl);
            });
        }

        public Task LoadModelFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));

            return Task.Run(async () =>
            {
                var documents =  Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var client = new HttpClient();
                var rawModelBytes = await client.GetByteArrayAsync(url);
                var filePath = Path.Combine(documents, "MobCATOrNot.mlmodel");
                File.WriteAllBytes(filePath, rawModelBytes);
                var rawModelUrl = new NSUrl(filePath); 
                _loadedModel = LoadModelInternal(rawModelUrl);
            });
        }

        public async Task<PredictionsResult> GetPredictions(PredictionsRequest request)
        {
            if (request?.Image == null)
                return null;

            await LoadModel();

            var imageData = NSData.FromStream(request.Image);
            var input = UIImage.LoadFromData(imageData);
            var output = Classify(_loadedModel, input);

            return new PredictionsResult
            {
                Predictions = output,
            };
        }

        public Task SubmitImages(SubmitImagesRequest request)
        {
            throw new NotImplementedException();
        }

        public Task StartModelTraining()
        {
            throw new NotImplementedException();
        }

        private MLModel LoadModelInternal(NSUrl rawModelUrl)
        {
            var compiledModelUrl = MLModel.CompileModel(rawModelUrl, out NSError compileError);
            if (compileError != null)
            {
                System.Diagnostics.Debug.WriteLine(compileError);
                throw new InvalidOperationException(compileError.Description);
            }

            var loadedModel = MLModel.Create(compiledModelUrl, out NSError createError);
            if (createError != null)
            {
                System.Diagnostics.Debug.WriteLine(createError);
                throw new InvalidOperationException(createError.Description);
            }

            return loadedModel;
        }

        private List<Prediction> Classify(MLModel model, UIImage source)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var scaled = source.Scale(new CGSize(227, 227));
            var pixelBuffer = ToCVPixelBuffer(scaled);
            var imageValue = MLFeatureValue.Create(pixelBuffer);

            var inputs = new NSDictionary<NSString, NSObject>(new NSString("data"), imageValue);

            NSError error, error2;
            var inputFp = new MLDictionaryFeatureProvider(inputs, out error);
            if (error != null)
            {
                System.Diagnostics.Debug.WriteLine(error);
                throw new InvalidOperationException(error.Description);
            }
            var outFeatures = model.GetPrediction(inputFp, out error2);
            if (error2 != null)
            {
                System.Diagnostics.Debug.WriteLine(error2);
                throw new InvalidOperationException(error2.Description);
            }

            var predictionsDictionary = outFeatures.GetFeatureValue("loss").DictionaryValue;
            var byProbability = new List<Tuple<double, string>>();
            foreach (var key in predictionsDictionary.Keys)
            {
                var description = (string)(NSString)key;
                var prob = (double)predictionsDictionary[key];
                byProbability.Add(new Tuple<double, string>(prob, description));
            }

            byProbability.Sort((t1, t2) => t1.Item1.CompareTo(t2.Item1) * -1);

            var result =   byProbability
                .Select(p => new Prediction { Label = p.Item2, Probability = p.Item1 })
                .ToList();

            return result;
        }

        private CVPixelBuffer ToCVPixelBuffer(UIImage self)
        {
            var attrs = new CVPixelBufferAttributes();
            attrs.CGImageCompatibility = true;
            attrs.CGBitmapContextCompatibility = true;

            var cgImg = self.CGImage;

            var pb = new CVPixelBuffer(cgImg.Width, cgImg.Height, CVPixelFormatType.CV32ARGB, attrs);
            pb.Lock(CVPixelBufferLock.None);
            var pData = pb.BaseAddress;
            var colorSpace = CGColorSpace.CreateDeviceRGB();
            var ctxt = new CGBitmapContext(pData, cgImg.Width, cgImg.Height, 8, pb.BytesPerRow, colorSpace, CGImageAlphaInfo.NoneSkipFirst);
            ctxt.TranslateCTM(0, cgImg.Height);
            ctxt.ScaleCTM(1.0f, -1.0f);
            UIGraphics.PushContext(ctxt);
            self.Draw(new CGRect(0, 0, cgImg.Width, cgImg.Height));
            UIGraphics.PopContext();
            pb.Unlock(CVPixelBufferLock.None);

            return pb;
        }
    }
}
