using ApiInterface;
using Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace NoaMedia.Pages
{
    public partial class VideoDetailsPage : Page
    {
        InterfaceAPI api = new InterfaceAPI();
        private Video currentVideo;

        public VideoDetailsPage(Video video)
        {
            InitializeComponent();
            currentVideo = video;

            // מילוי הנתונים בטקסטים שבעיצוב
            txtTitle.Text = video.VideoName;
            txtGenre.Text = video.Genre?.GenreDescription ?? "General";
            txtYear.Text = video.VideoUploadedDate.Year.ToString();
            txtDuration.Text = $"{video.LengthInMinutes} min";
            txtDescription.Text = video.VideoDescription ?? "No description available.";
            btnUploader.Content = video.WhoUploadedTheVideo?.UserName ?? "Unknown";

            // טעינת התמונה
            if (!string.IsNullOrEmpty(video.VideoPic))
            {
                imgPoster.Source = Base64ToImage(video.VideoPic);
            }

            LoadReviews();
        }

        // --- פונקציות שחובה להוסיף כדי לפתור את השגיאות מהצילום מסך ---

        // 1. פונקציית חזרה (מטפלת ב-Back_Click)
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        // 2. פונקציית לחיצה על כותב הביקורת (מטפלת ב-ReviewUser_Click)
        private void ReviewUser_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.DataContext is VideoReview review && review.WhoUpdatedTheReview != null)
            {
                NavigationService.Navigate(new UserDetailsPage(review.WhoUpdatedTheReview));
            }
        }

        // ---------------------------------------------------------

        private void btnUploader_Click(object sender, RoutedEventArgs e)
        {
            if (currentVideo.WhoUploadedTheVideo != null)
                NavigationService.Navigate(new UserDetailsPage(currentVideo.WhoUploadedTheVideo));
        }

        private async void LoadReviews()
        {
            try
            {
                var allReviews = await api.GetAllVideoReviews();
                if (allReviews != null)
                {
                    lstMovieReviews.ItemsSource = allReviews
                        .Where(r => r.WhichVideoDidTheUserReview != null && r.WhichVideoDidTheUserReview.Id == currentVideo.Id)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading reviews: " + ex.Message);
            }
        }

        // פונקציית עזר להצגת התמונה
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
                    return image;
                }
            }
            catch { return null; }
        }
    }
}