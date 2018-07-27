using System;
using System.Threading.Tasks;

namespace MobCATOrNot.Services
{
    public interface IDialogService
    {
        Task<string> DisplayActionSheet(string title, string cancel, params string[] items);
    }
}
