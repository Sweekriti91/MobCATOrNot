using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Provider;
using MobCATOrNot.Services;
using Xamarin.Forms;
using Java.IO;

[assembly: Dependency(typeof(MobCATOrNot.Droid.ImagePickerService))]

namespace MobCATOrNot.Droid
{
    public class ImagePickerService : IImagePickerService
    {
        readonly string _defaultPictureSaveDirectory = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/Pictures/";

        public ImagePickerService()
        {
        }

        Java.IO.File _currentFile;

        public Task<Stream> TakePhoto()
        {
            // Define the Intent for opening Camera
            Intent intent = new Intent();
            intent.SetAction(MediaStore.ActionImageCapture);
            var path = Path.Combine(_defaultPictureSaveDirectory, "image.jpg");
            _currentFile = new Java.IO.File(path);
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(_currentFile));

            // Save the TaskCompletionSource object as a MainActivity property
            MainActivity.Instance.CameraImageTaskCompletionSource = new TaskCompletionSource<Stream>();

            // Start the camera activity (resumes in MainActivity.cs)
            MainActivity.Instance.StartActivityForResult(intent, 
                MainActivity.CameraImageId);

            //Return Task Object
            return MainActivity.Instance.CameraImageTaskCompletionSource.Task;
        }

        public Task<List<Stream>> PickImage()
        {
            // Define the Intent for getting images
            Intent intent = new Intent();
            intent.SetType("image/*");
            intent.PutExtra(Intent.ExtraAllowMultiple, true);
            intent.SetAction(Intent.ActionGetContent);

            // Save the TaskCompletionSource object as a MainActivity property
            MainActivity.Instance.PickImageTaskCompletionSource = new TaskCompletionSource<List<Stream>>();

            // Start the picture-picker activity (resumes in MainActivity.cs)
            MainActivity.Instance.StartActivityForResult(
                Intent.CreateChooser(intent, "Select Picture"),
                MainActivity.PickImageId);

            // Return Task object
            return MainActivity.Instance.PickImageTaskCompletionSource.Task;

        }
    }
}
