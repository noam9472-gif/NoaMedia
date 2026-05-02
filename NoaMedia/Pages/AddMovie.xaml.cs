using ApiInterface;
using Microsoft.Win32;
using Model;
using System;
using System.IO;
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
        private string picPath = "";
        private User currentUser;

        public AddMovie(User user)
        {
            InitializeComponent();
            this.currentUser = user;
        }

        private void BtnUploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Title = "Select Movie Poster";
            openFile.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";

            if (openFile.ShowDialog() == true)
            {
                imgPreview.Source = new BitmapImage(new Uri(openFile.FileName));
                picPath = openFile.FileName;
            }
        }

        private async void SaveMovie_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(MovieNameTextBox.Text) || string.IsNullOrEmpty(picPath) || string.IsNullOrEmpty(MovieUrlTextBox.Text))
                {
                    MessageBox.Show("נא להזין שם, כתובת סרט ולהעלות תמונה.");
                    return;
                }

                if (!int.TryParse(DurationTextBox.Text, out int duration))
                {
                    MessageBox.Show("נא להזין אורך סרט תקין.");
                    return;
                }

                GenreList allGenres = await api.GetAllGenres();
                string genreName = GenreTextBox.Text;
                Genre selectedGenre = allGenres.FirstOrDefault(g => g.GenreDescription.Equals(genreName, StringComparison.OrdinalIgnoreCase));

                if (selectedGenre == null)
                {
                    MessageBox.Show("הז'אנר לא נמצא.");
                    return;
                }

                // יצירת האובייקט עם התקציר מהתיבה החדשה
                Video newVideo = new Video
                {
                    VideoName = MovieNameTextBox.Text,
                    LengthInMinutes = duration,
                    Genre = selectedGenre,
                    VideoPic = picPath,
                    VideoUploadedDate = DateTime.Now,
                    // כאן אנחנו מכניסים את התקציר מה-TextBox
                    VideoDescription = DescriptionTextBox.Text,
                    VideoAddress = MovieUrlTextBox.Text,
                    WhoUploadedTheVideo = currentUser
                };

                int success = await api.InsertVideo(newVideo);

                if (success == 1)
                {
                    MessageBox.Show("הסרט פורסם בהצלחה!");
                    this.NavigationService.GoBack();
                }
                else
                {
                    MessageBox.Show("שגיאה בשמירה בשרת.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e) => this.NavigationService.GoBack();
    }
}