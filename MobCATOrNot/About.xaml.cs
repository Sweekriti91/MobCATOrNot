using System;
using System.Collections.Generic;
using MobCATOrNot.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace MobCATOrNot
{
    public partial class About : ContentPage
    {
        public AboutViewModel TypedBindingContext => (AboutViewModel)BindingContext;

        public About()
        {
            InitializeComponent();
            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);
            BindingContext = new AboutViewModel();
        }

        private void Handle_ItemSelected(object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            AboutItems.SelectedItem = null;
        }

        private void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            TypedBindingContext.OpenItem.Execute(e.Item);
        }
    }
}
