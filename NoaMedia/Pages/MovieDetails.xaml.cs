using System;
using System.Windows;
using System.Windows.Controls;
using Model;
using ApiInterface;

namespace NoaMedia.Pages
{
    public partial class MovieDetails : Page
    {
        private InterfaceAPI api = new InterfaceAPI();
        private Video currentVideo; // משתנה גלובלי לדף

        public MovieDetails(Video selectedVideo)
        {
            InitializeComponent();
            currentVideo = selectedVideo; // שמירת הסרט שנבחר
            LoadMovieDetails(selectedVideo);
        }

        private async void LoadMovieDetails(Video v)
        {
            MovieTitle.Text = v.VideoName;
            MovieGenre.Text = v.Genre.GenreDescription;
            MovieDuration.Text = v.LengthInMinutes + " min";
            // MovieDesc.Text = v.Description; // אם קיים במודל

            try
            {
                string base64 = await api.GetVideoPicByte64(v.Id);
                if (!string.IsNullOrEmpty(base64))
                {
                    BackgroundImage.Source = ByteImageConverter.ByteToImage(Convert.FromBase64String(base64));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Image error: " + ex.Message);
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. בדיקה אם האובייקט או הכתובת קיימים
                if (currentVideo == null || string.IsNullOrWhiteSpace(currentVideo.VideoAddress))
                {
                    MessageBox.Show("Error: Video address is missing in the database.", "Missing Data");
                    return;
                }

                string address = currentVideo.VideoAddress;

                // 2. בדיקה אם הכתובת היא נתיב מקומי (למשל C:\Movies\vid.mp4) או כתובת אינטרנט
                // אם זו כתובת אינטרנט וחסר הפרוטוקול, נוסיף אותו
                if (!address.Contains("://") && !System.IO.Path.IsPathRooted(address))
                {
                    address = "http://" + address;
                }

                // 3. ניסיון יצירת ה-Uri בבטחה
                Uri videoUri;
                if (Uri.TryCreate(address, UriKind.RelativeOrAbsolute, out videoUri))
                {
                    VideoLayer.Visibility = Visibility.Visible;
                    InlinePlayer.Source = videoUri;
                    InlinePlayer.Play();
                }
                else
                {
                    MessageBox.Show("Error: Invalid URL format.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Critical Error: " + ex.Message);
            }
        }
        private void CloseVideo_Click(object sender, RoutedEventArgs e)
        {
            InlinePlayer.Stop();
            VideoLayer.Visibility = Visibility.Collapsed;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }
    }
}