using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using MobCATOrNot.ViewModels;

namespace MobCATOrNot
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);
            BindingContext = new MainPageViewModel
            {
                AddNewMobCAT = new Command(() => Navigation.PushAsync(new AddNewMobCat()))
            };
        }
    }
}
