using ApiInterface;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace NoaMedia.Pages
{
    public partial class VideoDetailsPage : Page
    {
        private readonly InterfaceAPI api = new InterfaceAPI();
        private Video currentVideo;

        public VideoDetailsPage(Video video)
        {
            InitializeComponent();
            currentVideo = video;
            PopulateVideoDetails();
            LoadReviews();
        }

        private void PopulateVideoDetails()
        {
            txtTitle.Text = currentVideo.VideoName;
            txtGenre.Text = currentVideo.Genre?.GenreDescription ?? "General";
            txtYear.Text = currentVideo.VideoUploadedDate.Year.ToString();
            txtDuration.Text = $"{currentVideo.LengthInMinutes} min";
            txtDescription.Text = currentVideo.VideoDescription ?? "No description available.";
            btnUploader.Content = currentVideo.WhoUploadedTheVideo?.UserName ?? "Unknown";

            if (!string.IsNullOrEmpty(currentVideo.VideoPic))
            {
                imgPoster.Source = Base64ToImage(currentVideo.VideoPic);
            }
        }

        private async void LoadReviews()
        {
            try
            {
                var allReviews = await api.GetAllVideoReviews();
                if (allReviews != null)
                {
                    // סינון ביקורות ששייכות לסרט הנוכחי בלבד
                    var movieReviews = allReviews
                        .Where(r => r.WhichVideoDidTheUserReview != null &&
                                    r.WhichVideoDidTheUserReview.Id == currentVideo.Id)
                        .ToList();

                    lstMovieReviews.ItemsSource = movieReviews;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading reviews: " + ex.Message);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private void ReviewUser_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            // תיקון: שימוש ב-WhoUpdatedTheReview כדי להתאים ל-Model
            if (btn?.DataContext is VideoReview review && review.WhoUpdatedTheReview != null)
            {
                NavigationService.Navigate(new UserDetailsPage(review.WhoUpdatedTheReview));
            }
        }

        private void btnUploader_Click(object sender, RoutedEventArgs e)
        {
            if (currentVideo.WhoUploadedTheVideo != null)
                NavigationService.Navigate(new UserDetailsPage(currentVideo.WhoUploadedTheVideo));
        }

        private BitmapImage Base64ToImage(string base64String)
        {
            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64String);
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(imageBytes))
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = ms;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    image.Freeze(); // חשוב לביצועים ושימוש ב-UI
                    return image;
                }
            }
            catch { return null; }
        }
    }
}