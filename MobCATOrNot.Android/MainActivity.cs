using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content;
using System.Collections.Generic;
using Android.Database;
using Android.Provider;
using Android.Graphics;
using Android.Media;
using System.IO;
using System.Threading.Tasks;
using Android;

namespace MobCATOrNot.Droid
{
    [Activity(Label = "MobCATOrNot", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        internal static MainActivity Instance { get; private set; }

        // Field, property, and method for Picture Picker
        public static readonly int PickImageId = 1000;
        public static readonly int CameraImageId = 500;
        public static readonly string filePath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/Pictures/" + "image.jpg";

        public TaskCompletionSource<List<System.IO.Stream>> PickImageTaskCompletionSource { set; get; }
        public TaskCompletionSource<System.IO.Stream> CameraImageTaskCompletionSource { set; get; }

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder();
            StrictMode.SetVmPolicy(builder.Build());
            base.OnCreate(bundle);

            CheckAppPermissions();

            Instance = this;
            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }

        private void CheckAppPermissions()
        {
            if ((int)Build.VERSION.SdkInt < 23)
            {
                return;
            }
            else
            {
                if (PackageManager.CheckPermission(Manifest.Permission.ReadExternalStorage, PackageName) != Permission.Granted
                    && PackageManager.CheckPermission(Manifest.Permission.WriteExternalStorage, PackageName) != Permission.Granted
                    && PackageManager.CheckPermission(Manifest.Permission.Camera, PackageName) != Permission.Granted)
                {
                    var permissions = new string[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage , Manifest.Permission.Camera};
                    RequestPermissions(permissions, requestCode: 1);
                }
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);

            if (requestCode == PickImageId)
            {
                if ((resultCode == Result.Ok) && (intent != null))
                {
                    
                    var inputUris = new List<Android.Net.Uri>();
                    if (intent.Data != null)
                        inputUris.Add(intent.Data);
                    
                    if (intent.ClipData != null && intent.ClipData.ItemCount > 0)
                    {
                        for (int i = 0; i < intent.ClipData.ItemCount; i++)
                            inputUris.Add(intent.ClipData.GetItemAt(i).Uri);
                    }

                    var result = new List<System.IO.Stream>();
                    foreach (var inputUri in inputUris)
                    {
                        System.IO.Stream stream = ContentResolver.OpenInputStream(inputUri);
                        var bmp = BitmapFactory.DecodeStream(stream);
                        MemoryStream ms = new MemoryStream();
                        bmp.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);
                        ms.Seek(0L, SeekOrigin.Begin);
                        result.Add(ms);
                    }

                    // Set the Stream as the completion of the Task
                    PickImageTaskCompletionSource.SetResult(result);
                }
                else
                {
                    PickImageTaskCompletionSource.SetResult(null);
                }
            }
            else
            {
                if ((resultCode == Result.Ok) && (intent != null))
                {
                    var bmp = BitmapFactory.DecodeFile(filePath);
                    MemoryStream ms = new MemoryStream();
                    bmp.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);
                    ms.Seek(0L, SeekOrigin.Begin);
                    // Set the Stream as the completion of the Task
                    CameraImageTaskCompletionSource.SetResult(ms);
                }
                else
                {
                    CameraImageTaskCompletionSource.SetResult(null);
                }
            }
        }
    }
}

