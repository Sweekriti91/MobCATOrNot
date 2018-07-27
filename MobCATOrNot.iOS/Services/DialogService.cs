using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MobCATOrNot.Services;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(MobCATOrNot.iOS.Services.DialogService))]

namespace MobCATOrNot.iOS.Services
{
    public class DialogService : IDialogService
    {
        public Task<string> DisplayActionSheet(string title, string cancel, params string[] items)
        {
            var topView = UIApplication.SharedApplication.KeyWindow?.RootViewController?.View;
            if (topView == null || items == null || items.Length == 0) 
                return Task.FromResult(cancel);
            
            var actionSheet = new UIActionSheet(title);
            var buttons = new List<string>();
            if (!string.IsNullOrWhiteSpace(cancel))
            {
                actionSheet.AddButton(cancel);
                actionSheet.CancelButtonIndex = 0;
                buttons.Add(cancel);
            }

            foreach (var item in items)
            {
                actionSheet.AddButton(item);
                buttons.Add(item);
            }
          
            var tcs = new TaskCompletionSource<string>(cancel);

            actionSheet.Dismissed += (s, e) =>
            {
                var selectedButton = buttons[(int)e.ButtonIndex];
                tcs?.TrySetResult(selectedButton);
            };

            actionSheet.ShowInView(topView);

            return tcs.Task;
        }
    }
}
