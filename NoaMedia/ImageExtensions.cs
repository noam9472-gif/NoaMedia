using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace NoaMedia 
{
    public static class ImageExtensions
    {
        public static BitmapImage ByteToImage(this byte[] array)
        {
            if (array == null || array.Length == 0) return null;

            try
            {
                var image = new BitmapImage();
                using (var ms = new MemoryStream(array))
                {
                    ms.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                }
                image.Freeze(); 
                return image;
            }
            catch { return null; }
        }
    }
}