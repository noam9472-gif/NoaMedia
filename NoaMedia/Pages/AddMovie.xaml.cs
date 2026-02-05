using ApiInterface;
using Model;
using NoaMedia.Pages; // וודא שה-Namespace נכון
using NoamediaLogin; // היכן שנמצא ה-InterfaceAPI שלך
using System;
using System.Windows;
using System.Windows.Controls;

namespace NoaMedia.Pages
{
    public partial class AddMovie : Page
    {
        // יצירת מופע של ה-API כדי שנוכל לתקשר עם השרת
        InterfaceAPI api = new InterfaceAPI();

        public AddMovie()
        {
            InitializeComponent();
        }

        private async void SaveMovie_Click(object sender, RoutedEventArgs e, AgeOfVideos ageLimit, AgeOfVideos ageLimit)
        {
            try
            {
                // 1. איסוף הנתונים מהשדות ב-XAML
                string name = MovieNameTextBox.Text;
                string genreName = GenreTextBox.Text;
                string posterUrl = PosterUrlTextBox.Text;

                // המרת אורך סרט וגיל למספרים (בדיקה שהקלט תקין)
                if (!int.TryParse(DurationTextBox.Text, out int duration) ||
                    !int.TryParse(AgeRatingTextBox.Text, out int ageLimit))
                {
                    MessageBox.Show("Please enter valid numbers for Duration and Age Rating.");
                    return;
                }

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(genreName))
                {
                    MessageBox.Show("Please fill in all required fields.");
                    return;
                }

                // 2. יצירת אובייקט וידאו חדש (בהתאם למחלקה Video אצלך)
                // הערה: נצטרך למצוא את ה-ID של הז'אנר או ליצור אובייקט ז'אנר
                Video newVideo = new Video
                {
                    VideoName = name,
                    LengthInMinutes = duration,
                    AgeOfVideo = ageLimit,
                    // כאן בדרך כלל ה-API מצפה לז'אנר מלא או ל-ID
                    Genre = new Genre { GenreDescription = genreName }
                };

                // 3. שליחה לשרת דרך ה-API
                // הערה: וודא ששם הפעולה ב-API שלך הוא אכן InsertVideo
              bool success = await api.InsertVideo(newVideo);

                if (success)
                {
                    MessageBox.Show("Movie saved successfully to the database!");

                    // 4. חזרה לדף הבית כדי לראות את הסרט החדש
                    this.NavigationService.Navigate(new Home());
                }
                else
                {
                    MessageBox.Show("Failed to save movie. Please try again.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }
    }
}