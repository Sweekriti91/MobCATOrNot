using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MobCATOrNot.Services;
using Xamarin.Forms;

namespace MobCATOrNot.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public Lazy<IDialogService> DialogService { get; } = new Lazy<IDialogService>(() => DependencyService.Get<IDialogService>());
        public Lazy<IImagePickerService> ImagePickerService { get; } = new Lazy<IImagePickerService>(() => DependencyService.Get<IImagePickerService>());
        public Lazy<ICustomVisionService> CustomVisionService { get; } = new Lazy<ICustomVisionService>(() => DependencyService.Get<ICustomVisionService>());
        public Lazy<ICustomVisionAPIService> CustomVisionAPIService { get; } = new Lazy<ICustomVisionAPIService>(() => DependencyService.Get<ICustomVisionAPIService>());

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { UpdateAndRaise(ref _isBusy, value); }
        }

        private string _statusText;
        public string StatusText
        {
            get { return _statusText; }
            set
            {
                UpdateAndRaise(ref _statusText, value);
                System.Diagnostics.Debug.WriteLine(StatusText);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool UpdateAndRaise<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            Raise(propertyName);

            return true;
        }

        protected void Raise([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
