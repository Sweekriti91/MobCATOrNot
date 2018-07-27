using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using MobCATOrNot.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using System.Threading.Tasks;
using System.IO;

namespace MobCATOrNot
{
    public partial class AddNewMobCat : ContentPage
    {
        public AddNewMobCatViewModel TypedBindingContext => (AddNewMobCatViewModel)BindingContext;

        public AddNewMobCat()
        {
            InitializeComponent();
            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);
            BindingContext = new AddNewMobCatViewModel() { RefreshImagesToUpload = new Command(async () => await SyncFlexLayout()) };
            SyncFlexLayout();
        }

        private async Task SyncFlexLayout()
        {
            // TODO: add new items only and remove obsoleted
            flexLayout.Children.Clear();

            foreach (var imageToUpload in TypedBindingContext.ImagesToUpload)
            {
                // TODO: optimize loading just once
                var imageCopy = new MemoryStream();
                await imageToUpload.CopyToAsync(imageCopy);
                imageCopy.Seek(0, SeekOrigin.Begin);

                imageToUpload.Seek(0, System.IO.SeekOrigin.Begin);
                var image = new Image();
                image.Aspect = Aspect.AspectFill;
                image.BackgroundColor = Color.AliceBlue;
                //image.Source = ImageSource.FromFile("testme.png");
                image.Source = ImageSource.FromStream(() => imageCopy);
                image.WidthRequest = 140;
                image.HeightRequest = 140;
                image.Margin = new Thickness(10, 10, 0, 0);
                flexLayout.Children.Add(image);
            }
        }
    }
}
