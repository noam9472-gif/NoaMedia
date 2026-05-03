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
        // אירוע לחצן העלאת תמונה שמאפשר למשתמש לבחור תמונה מהמחשב שלו ולהציג אותה בתצוגה מקדימה
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
                    MessageBox.Show("Enter all required fields."); // הודעה למשתמש אם הוא לא הזין את כל השדות ה
                    return;
                }

                if (!int.TryParse(DurationTextBox.Text, out int duration))
                {
                    MessageBox.Show("Enter a valid movie duration."); // הודעה למשתמש אם הוא לא הזין אורך סרט תקין
                    return;
                }

                GenreList allGenres = await api.GetAllGenres();
                string genreName = GenreTextBox.Text;
                Genre selectedGenre = allGenres.FirstOrDefault(g => g.GenreDescription.Equals(genreName, StringComparison.OrdinalIgnoreCase));

                if (selectedGenre == null)
                {
                    MessageBox.Show("Genre not found."); // הודעה למשתמש אם הז'אנר לא נמצא
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
                    VideoDescription = DescriptionTextBox.Text,
                    VideoAddress = MovieUrlTextBox.Text,
                    WhoUploadedTheVideo = currentUser
                };

                int success = await api.InsertVideo(newVideo);

                if (success == 1)
                {
                    MessageBox.Show("The movie was published successfully!"); // הודעה למשתמש אם הסרט פורסם בהצלחה
                    this.NavigationService.GoBack();
                }
                else
                {
                    MessageBox.Show("Error saving the movie on the server."); // הודעה למשתמש אם יש שגיאה בשמירה בשרת
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message); // הודעה למשתמש אם יש שגיאה כללית
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e) => this.NavigationService.GoBack();
    }
}