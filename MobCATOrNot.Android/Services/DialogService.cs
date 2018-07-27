using System;
using System.Threading.Tasks;
using Android.App;
using Android.Views;
using Android.Widget;
using MobCATOrNot.Services;

[assembly: Xamarin.Forms.Dependency(typeof(MobCATOrNot.Droid.DialogService))]

namespace MobCATOrNot.Droid
{
    public class DialogService : IDialogService
    {
       

        public Task<string> DisplayActionSheet(string title, string cancel, params string[] items)
        {
            var currentContext = MainActivity.Instance;
            if (currentContext == null || items == null || items.Length == 0)
                return Task.FromResult(cancel);

            var tcs = new TaskCompletionSource<string>(cancel);
            var builder = new AlertDialog.Builder(currentContext);
            builder.SetTitle(title);
            var listView = new ListView(currentContext);
            listView.SetPadding(20, 20, 20, 20);
            listView.Adapter = new ActionSheetAdapter(currentContext, items);
            builder.SetView(listView);
            builder.SetNegativeButton(cancel, (s, e) => tcs?.TrySetResult(cancel));
            var actionSheet = builder.Create();
            actionSheet.CancelEvent += (s, e) => tcs?.TrySetResult(cancel);
            actionSheet.DismissEvent += (s, e) => tcs?.TrySetResult(cancel);
            listView.ItemClick += (s, e) =>
            {
                tcs?.TrySetResult(items[e.Position]);
                actionSheet.Dismiss();
            };

            actionSheet.Show();
            return tcs.Task;
        }

        public class ActionSheetAdapter : BaseAdapter<string>
        {
            private Activity _context;
            private string[] _items;

            public ActionSheetAdapter(Activity context, string[] items)
            {
                _context = context;
                _items = items;
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                if (convertView == null)
                    convertView = _context.LayoutInflater.Inflate(Resource.Layout.ActionListItem, null);

                convertView.FindViewById<TextView>(Resource.Id.ActionListItemText)
                           .SetText(this[position], TextView.BufferType.Normal);

                return convertView;
            }

            public override int Count => _items.Length;

            public override string this[int position] => _items[position];
        }
    }
}
