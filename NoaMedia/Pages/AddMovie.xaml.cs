using ApiInterface;
using Microsoft.Win32; // נוסף עבור בחירת קובץ
using Model;
using System;
using System.IO;       // נוסף עבור קריאת הקובץ
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace NoaMedia.Pages
{
    public partial class AddMovie : Page
    {
        InterfaceAPI api = new InterfaceAPI();
        // משתנה שישמור את התמונה שהמשתמש בחר
        private string base64Image = "";

        public AddMovie()
        {
            InitializeComponent();
        }

        // פונקציה חדשה: בחירת תמונה מהמחשב והמרתה לסטרינג
        private void BtnUploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";

            if (openFile.ShowDialog() == true)
            {
                // המרה ל-Base64 עבור מסד הנתונים
                byte[] imageBytes = File.ReadAllBytes(openFile.FileName);
                base64Image = Convert.ToBase64String(imageBytes);

                // הצגת התמונה על המסך בשביל המשתמש
                imgPreview.Source = new BitmapImage(new Uri(openFile.FileName));
            }
        }

        private async void SaveMovie_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. איסוף נתונים
                string name = MovieNameTextBox.Text;
                string genreName = GenreTextBox.Text;

                if (!int.TryParse(DurationTextBox.Text, out int duration) ||
                    !int.TryParse(AgeRatingTextBox.Text, out int ageValue))
                {
                    MessageBox.Show("Please enter valid numbers.");
                    return;
                }

                // 2. מציאת ה-Genre
                GenreList allGenres = await api.GetAllGenres();
                Genre selectedGenre = allGenres.FirstOrDefault(g => g.GenreDescription.Equals(genreName, StringComparison.OrdinalIgnoreCase));

                if (selectedGenre == null)
                {
                    MessageBox.Show("Genre not found.");
                    return;
                }

                // 3. יצירת אובייקט הסרט כולל התמונה!
                Video newVideo = new Video();
                newVideo.VideoName = name;
                newVideo.LengthInMinutes = duration;
                newVideo.Genre = selectedGenre;
                newVideo.AgeOfVideo = (AgeOfVideos)Enum.ToObject(typeof(AgeOfVideos), ageValue);

                // כאן אנחנו מכניסים את התמונה ששמרנו קודם
                newVideo.VideoPic = base64Image;

                // הוספת ערכי ברירת מחדל לשדות חובה במסד הנתונים (אם חסר)
                newVideo.VideoUploadedDate = DateTime.Now;
                newVideo.VideoDescription = "No description";
                newVideo.VideoAddress = "local";

               
                // הוסף את השורה הזו כדי לראות בחלון ה-Output של VS אם הסטרינג אכן קיים
                System.Diagnostics.Debug.WriteLine("Image length: " + (newVideo.VideoPic?.Length ?? 0));

                await api.InsertVideo(newVideo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e) => this.NavigationService.GoBack();
        private void MovieNameTextBox_TextChanged(object sender, TextChangedEventArgs e) { }
    }
}