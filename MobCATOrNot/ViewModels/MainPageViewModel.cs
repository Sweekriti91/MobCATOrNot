using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MobCATOrNot.Models;
using MobCATOrNot.Services;
using Xamarin.Forms;
using System.Collections.Generic;

namespace MobCATOrNot.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private ImageSource _selectedImage;
        public ImageSource SelectedImage
        {
            get { return _selectedImage; }
            set { UpdateAndRaise(ref _selectedImage, value); }
        }

        private MobCatOrNotStatus? _mobCatStatus;
        public MobCatOrNotStatus? MobCatStatus
        {
            get { return _mobCatStatus; }
            set
            {
                if (UpdateAndRaise(ref _mobCatStatus, value))
                {
                    System.Diagnostics.Debug.WriteLine($"MobCatStatus: {MobCatStatus}");
                    Raise(nameof(MobCatStatusImage));
                    Raise(nameof(AnyResultAvailable));
                }
            }
        }

        public string MobCatStatusImage => MobCatStatus?.ToString();


        private List<Prediction> _predictionTags = new List<Prediction>();
        public List<Prediction> PredictionTags
        {
            get { return _predictionTags; }
            set
            {
                if (UpdateAndRaise(ref _predictionTags, value))
                {
                    Raise(nameof(PredictionTagsText));
                }
            }
        }

        public bool AnyResultAvailable => MobCatStatus != null;

        public string PredictionTagsText
        {
            get 
            {
                if (_predictionTags == null || _predictionTags.Count == 0)
                    return null;

                var listTags = _predictionTags
                    .Where(p => p.Probability >= Constants.ProbabilityThreshold)
                    .Select(p => $"#{p.Label} ({(p.Probability * 100):0}%)")
                    .ToList();
                
                var result = string.Join(",", listTags);
                return result;
            }
        }

        public Command CheckMobCAT { get; }
        public Command AddNewMobCAT { get; set; }
        public Command UpdateModel { get; set; }

        public MainPageViewModel()
        {
            CheckMobCAT = new Command(async () => await OnCheckMobCAT());
            UpdateModel = new Command(async () => await OnUpdateModel());
        }

        private async Task OnCheckMobCAT()
        {
            try
            {
                IsBusy = true;
                MobCatStatus = null;
                StatusText = null;
                PredictionTags = null;

                var selectedOption = await DialogService.Value.DisplayActionSheet("Check MobCat or Not", "Nevermind...", Constants.OptionPickFromLibrary, Constants.OptionTakePhoto);
                Stream inputImage = null;
                if (selectedOption == Constants.OptionPickFromLibrary)
                {
                    var pickedImages = await ImagePickerService.Value.PickImage();
                    inputImage = pickedImages?.FirstOrDefault();
                }
                else if (selectedOption == Constants.OptionTakePhoto)
                {
                    inputImage = await ImagePickerService.Value.TakePhoto();
                }

                if (inputImage == null)
                {
                    StatusText = null;
                    return;
                }

                await UpdateSelectedImage(inputImage);
                await UpdatePredictionsForImage(inputImage);
            }
            catch (Exception ex)
            {
                StatusText = $"Failed. {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task UpdateSelectedImage(Stream inputImage)
        {
            // copy is required to work in parallel on prediction and image display

            var imageCopy = new MemoryStream();
            await inputImage.CopyToAsync(imageCopy);
            if(Device.RuntimePlatform == Device.iOS)
            {
                inputImage.Seek(0, SeekOrigin.Begin);
            }

            imageCopy.Seek(0, SeekOrigin.Begin);
            SelectedImage = ImageSource.FromStream(() => imageCopy);
        }

        private async Task UpdatePredictionsForImage(Stream inputImage)
        {
            var predictionsRequest = new PredictionsRequest { Image = inputImage };
            var predictions = await CustomVisionService.Value.GetPredictions(predictionsRequest);
            foreach (var prediction in predictions?.Predictions)
                System.Diagnostics.Debug.WriteLine($"{prediction.Label}: {(prediction.Probability * 100):0.00}%\n");

            var predictionTags = predictions?
                .Predictions?
                .Where(p => p.Probability >= Constants.ProbabilityThreshold)
                .OrderByDescending(p => p.Probability)
                .ToList();

            var status = MobCatOrNotStatus.idk;
            if (predictionTags != null)
            {
                var anyPersonTag = predictionTags.FirstOrDefault(p => p.Label != Constants.CustomVisionMobCatTag && p.Label != Constants.CustomVisionNotMobCatTag);
                var anyMobcatTag = predictionTags.FirstOrDefault(p => p.Label == Constants.CustomVisionMobCatTag);
                var anyNotMobcatTag = predictionTags.FirstOrDefault(p => p.Label == Constants.CustomVisionNotMobCatTag);
                if (anyPersonTag != null)
                    status = MobCatOrNotStatus.yes;
                else if (anyMobcatTag != null && anyMobcatTag.Probability > (anyNotMobcatTag?.Probability ?? 0))
                    status = MobCatOrNotStatus.idk;
                else if (anyNotMobcatTag != null)
                    status = MobCatOrNotStatus.no;
            }
            
            MobCatStatus = status;
            PredictionTags = predictions?.Predictions;
        }

        private async Task OnUpdateModel()
        {
            try
            {
                IsBusy = true;
                MobCatStatus = null;
                StatusText = "Updating model...";

                var iterations = await CustomVisionAPIService.Value.GetIterations();
                var theMostRecent = iterations?.OrderByDescending(i => i.created)
                                               .Where(i => i.exportable && i.status == CustomVisionIterationStatus.Completed.ToString())
                                               .FirstOrDefault();
                if (theMostRecent == null)
                    throw new ArgumentException($"Unable to find the most recent completed iteration");
                
                CustomVisionIterationExport export;

                if(Device.RuntimePlatform == Device.iOS)
                    export = await CustomVisionAPIService.Value.GetIterationModelExport(theMostRecent.id, CustomVisionModelType.CoreML);
                else
                    export = await CustomVisionAPIService.Value.GetIterationModelExport(theMostRecent.id, CustomVisionModelType.TensorFlow);
                
                if (export == null || export.status != CustomVisionIterationExportStatus.Done.ToString() || string.IsNullOrWhiteSpace(export.downloadUri))
                    throw new ArgumentException($"Iteration {theMostRecent.id} is not available for export");

                await CustomVisionService.Value.LoadModelFromUrl(export.downloadUri);

                StatusText = "The AI model has been updated";
            }
            catch(Exception ex)
            {
                StatusText = $"Failed. {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}