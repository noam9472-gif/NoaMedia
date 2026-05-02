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

        private void PopulateVideoDetails() // מילוי פרטי הסרט בעמוד
        {
            txtTitle.Text = currentVideo.VideoName;
            txtGenre.Text = currentVideo.Genre?.GenreDescription ?? "General"; // אם אין ז'אנר, מציג ברירת מחדל
            txtYear.Text = currentVideo.VideoUploadedDate.Year.ToString(); // מציג רק את השנה
            txtDuration.Text = $"{currentVideo.LengthInMinutes} min"; // מציג את משך הסרט בדקות
            txtDescription.Text = currentVideo.VideoDescription ?? "No description available."; // אם אין תיאור, מציג ברירת מחדל
            btnUploader.Content = currentVideo.WhoUploadedTheVideo?.UserName ?? "Unknown"; // אם אין מידע על המעלה, מציג ברירת מחדל

            if (!string.IsNullOrEmpty(currentVideo.VideoPic)) // אם יש תמונה, מציג אותה
            {
                imgPoster.Source = Base64ToImage(currentVideo.VideoPic); // המרה
            }
        }

        private async void LoadReviews() // טעינת הביקורות לסרט הנוכחי
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

        private void ReviewUser_Click(object sender, RoutedEventArgs e) // לחיצה על שם המשתמש שכתב את הביקורת
        {
            var btn = sender as Button;
            if (btn?.DataContext is VideoReview review && review.WhoUpdatedTheReview != null)
            {
                NavigationService.Navigate(new UserDetailsPage(review.WhoUpdatedTheReview));
            }
        }

        private void btnUploader_Click(object sender, RoutedEventArgs e) // לחיצה על שם המעלה של הסרט
        {
            if (currentVideo.WhoUploadedTheVideo != null)
                NavigationService.Navigate(new UserDetailsPage(currentVideo.WhoUploadedTheVideo));
        }

        private BitmapImage Base64ToImage(string base64String) 
        {
            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64String); // המרה מבסיס64 לבייטים
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(imageBytes)) 
                {
                    BitmapImage image = new BitmapImage();  
                    image.BeginInit(); // התחלת אתחול התמונה
                    image.StreamSource = ms; 
                    image.CacheOption = BitmapCacheOption.OnLoad; // טעינת התמונה כולה לזיכרון כדי לאפשר סגירת הזרם
                    image.EndInit(); // סיום אתחול התמונה
                    image.Freeze(); 
                    return image; // החזרת התמונה כ- BitmapImage
                }
            }
            catch { return null; }
        }
    }
}