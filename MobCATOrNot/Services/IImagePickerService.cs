using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

namespace MobCATOrNot.Services
{
    public interface IImagePickerService
    {
        Task<List<Stream>> PickImage();
        Task<Stream> TakePhoto();
    }
}
