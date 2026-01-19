using Model; // המחלקה המקורית שלך
using System.Windows.Media; // כאן זה יעבוד!

namespace NoaMedia // השם של פרויקט ה-WPF
{
    // המחלקה הזו יורשת הכל מ-Video ומוסיפה רק את שדה התמונה
    public class VideoDisplay : Video
    {
        public ImageSource MovieImageSource { get; set; }
    }
}