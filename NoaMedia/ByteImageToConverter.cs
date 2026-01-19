using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NoaMedia // וודא שזה ה-Namespace של פרויקט ה-WPF שלך
{
    public class ByteImageConverter
    {
        public static ImageSource ByteToImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;

            BitmapImage biImg = new BitmapImage();
            using (MemoryStream ms = new MemoryStream(imageData))
            {
                biImg.BeginInit();
                biImg.StreamSource = ms;
                biImg.CacheOption = BitmapCacheOption.OnLoad; // ככה התמונה לא תיעלם מהזיכרון
                biImg.EndInit();
            }
            return biImg;
        }
    }
}