using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MobCATOrNot.Models;
using Xamarin.Forms;

namespace MobCATOrNot.ViewModels
{
    public class AddNewMobCatViewModel : ViewModelBase
    {
        private readonly ObservableCollection<Stream> _imagesToUpload = new ObservableCollection<Stream>();
        public ObservableCollection<Stream> ImagesToUpload => _imagesToUpload;

        public bool AnyPhotoAvailable => ImagesToUpload.Count > 0;
      
        private bool _mobcatPhotos = true;
        public bool MobCatPhotos
        {
            get { return _mobcatPhotos; }
            set
            {
                if (UpdateAndRaise(ref _mobcatPhotos, value))
                {
                    Raise(nameof(MobCatOrNotText));
                }
            }
        }

        public string MobCatOrNotText => MobCatPhotos ? Constants.CustomVisionMobCatTag : Constants.CustomVisionNotMobCatTag;

        private string _personTag;
        public string PersonTag 
        {
            get { return _personTag; }
            set { UpdateAndRaise(ref _personTag, value); }
        }

        public Command AddPhotos { get; }
        public Command UploadPhotos { get; }
        public Command TrainIteration { get; }
        public Command CheckIteration { get; }
        public Command RefreshImagesToUpload { get; set; }

        public AddNewMobCatViewModel()
        {
            AddPhotos = new Command(async () => await OnAddPhotos());
            UploadPhotos = new Command(async () => await OnUploadPhotos());
            TrainIteration = new Command(async () => await OnTrainIteration());
            CheckIteration = new Command(async () => await OnCheckIteration());

            //#if DEBUG
            //for (int i = 0; i < 3; i++)
            //{
            //    var fakeImageData = new byte[] { 1, 2, 3 };
            //    var fakeImageStream = new MemoryStream(fakeImageData);
            //    _imagesToUpload.Add(fakeImageStream);
            //}
            //#endif

            PersonTag = Device.RuntimePlatform == Device.iOS ? "guest1" : "guest2";
        }

        private async Task OnAddPhotos()
        {
            try
            {
                IsBusy = true;
                var selectedOption = await DialogService.Value.DisplayActionSheet("Adding new MobCAT'er?", "Nevermind...", Constants.OptionPickFromLibrary, Constants.OptionTakePhoto);
                var inputImages = new List<Stream>();
                if (selectedOption == Constants.OptionPickFromLibrary)
                {
                    var pickedImages = await ImagePickerService.Value.PickImage();
                    if (pickedImages != null && pickedImages.Count > 0)
                        inputImages.AddRange(pickedImages);
                }
                else if (selectedOption == Constants.OptionTakePhoto)
                {
                    var takenImage = await ImagePickerService.Value.TakePhoto();
                    if (takenImage != null)
                        inputImages.Add(takenImage);
                }

                if (inputImages.Count > 0)
                {
                    foreach (var inputImage in inputImages)
                    {
                        // copy as managed stream
                        var imageCopy = new MemoryStream();
                        await inputImage.CopyToAsync(imageCopy);
                        imageCopy.Seek(0, SeekOrigin.Begin);
                        _imagesToUpload.Add(imageCopy);
                    }
                }

                RefreshImagesToUpload?.Execute(null);
                Raise(nameof(AnyPhotoAvailable));
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

        private async Task OnUploadPhotos()
        {
            if (_imagesToUpload.Count == 0)
            {
                StatusText = "Please add some images to upload first using the 'Photos' button";
                return;
            }

            try
            {
                var tagName = MobCatPhotos ? Constants.CustomVisionMobCatTag : Constants.CustomVisionNotMobCatTag;
                IsBusy = true;
                StatusText = $"Uploading images as [{tagName}]...";
                var tagIds = new List<string>();
                var tags = await CustomVisionAPIService.Value.GetTags();
                var tag = tags?.FirstOrDefault(t => t.name == tagName);
                if (tag == null)
                {
                    if (_imagesToUpload.Count < 5)
                        throw new InvalidOperationException("Pick at least 5 images for new tag creation");
                    
                    StatusText = $"Creating [{tagName}] tag...";
                    tag = await CustomVisionAPIService.Value.CreateTag(tagName);
                }
                tagIds.Add(tag.id);

                if (!string.IsNullOrWhiteSpace(PersonTag))
                {
                    var personTag = tags?.FirstOrDefault(t => t.name == PersonTag);
                    if (personTag == null)
                    {
                        if (_imagesToUpload.Count < 5)
                            throw new InvalidOperationException("Pick at least 5 images for new tag creation");
                   
                        StatusText = $"Creating person tag [{PersonTag}]...";
                        personTag = await CustomVisionAPIService.Value.CreateTag(PersonTag);
                    }
                    tagIds.Add(personTag.id);
                }

                var imageToUpload = _imagesToUpload.ToList();
                var uploadImageResult = await CustomVisionAPIService.Value.UploadImages(imageToUpload, tagIds);
                var uploadSuccess = uploadImageResult?.Where(i => i.status == CustomVisionUploadImageStatus.OK.ToString()).Count();
                StatusText = $"Uploaded {uploadSuccess} out of {imageToUpload.Count} images as [{tagName}]!";

                _imagesToUpload.Clear();
                RefreshImagesToUpload?.Execute(null);
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

        private async Task OnTrainIteration()
        {
            try
            {
                IsBusy = true;
                StatusText = "Request training...";
                var newIteration = await CustomVisionAPIService.Value.TriggerIterationTraining();

                StatusText = "Training has been started!";

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

        private async Task OnCheckIteration()
        {
            try
            {
                IsBusy = true;
                StatusText = "Checking training...";
                var iterations = await CustomVisionAPIService.Value.GetIterations();
                var theMostRecent = iterations?.OrderByDescending(i => i.created).FirstOrDefault();
                StatusText = $"Training created {theMostRecent.created} is in status {theMostRecent.status}";
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
    }
}

