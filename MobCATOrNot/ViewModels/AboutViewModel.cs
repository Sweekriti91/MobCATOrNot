using System;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using MobCATOrNot.Models;

namespace MobCATOrNot.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {   
        public ObservableCollection<AboutItem> ItemsAbout { get; } = new ObservableCollection<AboutItem>()
        {
            new AboutItem { Title = "Custom Vision", Url = "https://www.customvision.ai/" },
            new AboutItem { Title = "Xamarin.Forms", Url = "https://www.xamarin.com/forms" },
            new AboutItem { Title = "Xamarin.Forms.Shell", Url = "https://github.com/xamarin/Xamarin.Forms/issues/2415" },
            new AboutItem { Title = "CoreML", Url = "https://developer.apple.com/documentation/coreml" },
            new AboutItem { Title = "TensorFlow", Url = "https://www.tensorflow.org/mobile/" },
            new AboutItem { Title = "Azure Functions", Url = "https://azure.microsoft.com/en-us/services/functions/" },
            new AboutItem { Title = "Visual Studio Team Services", Url = "https://visualstudio.microsoft.com/team-services/" },
        };

        public Command<AboutItem> OpenItem { get; } = new Command<AboutItem>(i =>
        {
            Device.OpenUri(new Uri(i.Url));
        });

        public AboutViewModel()
        {
        }
    }
}

