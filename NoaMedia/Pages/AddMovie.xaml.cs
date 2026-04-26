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
                string name = MovieNameTextBox.Text;
                string genreName = GenreTextBox.Text;

                if (!int.TryParse(DurationTextBox.Text, out int duration) ||
                    !int.TryParse(AgeRatingTextBox.Text, out int ageValue))
                {
                    MessageBox.Show("Please enter valid numbers.");
                    return;
                }

                GenreList allGenres = await api.GetAllGenres();
                Genre selectedGenre = allGenres.FirstOrDefault(g => g.GenreDescription.Equals(genreName, StringComparison.OrdinalIgnoreCase));

                if (selectedGenre == null)
                {
                    MessageBox.Show("Genre not found.");
                    return;
                }
                Video newVideo = new Video();
                newVideo.VideoName = name;
                newVideo.LengthInMinutes = duration;
                newVideo.Genre = selectedGenre;

                newVideo.VideoPic = base64Image;

                newVideo.VideoUploadedDate = DateTime.Now;
                newVideo.VideoDescription = "No description";
                newVideo.VideoAddress = "local";

               
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