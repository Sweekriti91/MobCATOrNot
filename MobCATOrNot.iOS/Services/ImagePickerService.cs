using System;
using System.IO;
using System.Threading.Tasks;
using Foundation;
using MobCATOrNot.Services;
using UIKit;
using Xamarin.Forms;
using System.Collections.Generic;

[assembly: Dependency(typeof(MobCATOrNot.iOS.Services.ImagePickerService))]

namespace MobCATOrNot.iOS.Services
{
    public class ImagePickerService : IImagePickerService
    {
        public async Task<List<Stream>> PickImage()
        {
            var pickedImage = await GetImageStream(UIImagePickerControllerSourceType.PhotoLibrary);
            if (pickedImage != null)
                return new List<Stream> { pickedImage };

            return null;
        }

        public Task<Stream> TakePhoto()
        {
            return GetImageStream(UIImagePickerControllerSourceType.Camera);
        }

        private async Task<Stream> GetImageStream(UIImagePickerControllerSourceType sourceType)
        {
            if (!UIImagePickerController.IsSourceTypeAvailable(sourceType))
            {
                throw new NotSupportedException("{sourceType} source is not available");
            }

            var tcs = new TaskCompletionSource<UIImage>();
            var picker = new UIImagePickerController()
            {
                Delegate = new ImagePickerControllerDelegate(tcs),
                SourceType = sourceType
            };

            var rootController = UIApplication.SharedApplication.KeyWindow?.RootViewController;
            rootController?.PresentViewController(picker, true, null);

            var pickedImage = await tcs.Task;
            if (pickedImage == null) 
                return null;
            
            var imageData = pickedImage.AsJPEG(1);
            var imageStream = imageData.AsStream();
            return imageStream;
        }
    }

    public class ImagePickerControllerDelegate : UIImagePickerControllerDelegate
    {
        private readonly TaskCompletionSource<UIImage> _taskCompletionSource;

        public ImagePickerControllerDelegate(TaskCompletionSource<UIImage> taskCompletionSource)
        {
            _taskCompletionSource = taskCompletionSource;
        }

        public override void Canceled(UIImagePickerController picker)
        {
            picker.DismissViewController(true, null);
            _taskCompletionSource.TrySetResult(null);
        }

        public override void FinishedPickingMedia(UIImagePickerController picker, NSDictionary info)
        {
            picker.DismissViewController(true, null);
            var uiImage = info[UIImagePickerController.OriginalImage] as UIImage;
            if (uiImage == null)
            {
                _taskCompletionSource.SetResult(null);
                return;
            }

            _taskCompletionSource.TrySetResult(uiImage);
        }
    }
}