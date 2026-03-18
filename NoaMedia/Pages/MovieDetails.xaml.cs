using ApiInterface;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace NoaMedia.Pages
{
    public partial class MovieDetails : Page
    {
        private IInterfaceAPI api = new InterfaceAPI();
        private Video currentVideo;

        public MovieDetails(Video selectedVideo)
        {
            InitializeComponent();
            currentVideo = selectedVideo;
            LoadMovieDetails(selectedVideo);
        }

        private async void LoadMovieDetails(Video v)
        {
            if (v == null) return;

            // עדכון פרטים בסיסיים
            MovieTitle.Text = v.VideoName;
            MovieGenre.Text = v.Genre?.GenreDescription ?? "General";
            MovieDuration.Text = v.LengthInMinutes > 0 ? $"{v.LengthInMinutes} min" : "";
            string description = !string.IsNullOrWhiteSpace(v.VideoDescription) ? v.VideoDescription : "No description available.";
            MovieDesc.Text = description;
            FullDescriptionText.Text = description;

            if (WhoUploadedName != null)
                WhoUploadedName.Text = v.WhoUploadedTheVideo?.UserName ?? "Admin";

            if (v.VideoUploadedDate != DateTime.MinValue)
                ReleaseYear.Text = v.VideoUploadedDate.ToShortDateString();

            // טעינת תמונה
            try
            {
                if (!string.IsNullOrEmpty(v.VideoPic) && !v.VideoPic.Contains("found"))
                    BackgroundImage.Source = Base64ToImage(v.VideoPic);
                else
                {
                    string base64 = await api.GetVideoPicByte64(v.Id);
                    if (!string.IsNullOrEmpty(base64))
                        BackgroundImage.Source = Base64ToImage(base64);
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Error: " + ex.Message); }

            // טעינת ביקורות - שימוש בפונקציה החדשה עם הסינון
            try
            {
                lstMovieReviews.ItemsSource = null; // איפוס הרשימה לפני טעינה
                var reviews = await api.GetReviewsByVideoId(v.Id);
                lstMovieReviews.ItemsSource = reviews;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading reviews: " + ex.Message);
            }

            // בדיקת לייק
            var myApp = Application.Current as App;
            if (myApp?.LoggedInUser != null)
            {
                bool hasLiked = await api.CheckIfUserLikedVideo(myApp.LoggedInUser.Id, v.Id);
                LikeButton.Tag = hasLiked;
                LikeIcon.Foreground = hasLiked ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.White);
            }
        }

        // פונקציה חסרה שמופיעה ב-XAML עבור לחיצה על שם משתמש בביקורת
        private void ReviewUser_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is VideoReview review)
            {
                // כאן תוכל להוסיף ניווט לפרופיל המשתמש בעתיד
                // NavigationService.Navigate(new UserProfilePage(review.WhoUpdatedTheReview));
            }
        }

        private void AddReviewButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This feature will allow you to add a review soon!", "Coming Soon");
        }

        private async void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            var myApp = Application.Current as App;
            if (myApp?.LoggedInUser == null || currentVideo == null) return;
            bool isLiked = (LikeButton.Tag as bool?) ?? false;

            try
            {
                if (isLiked)
                {
                    await api.RemoveLike(myApp.LoggedInUser.Id, currentVideo.Id);
                    LikeIcon.Foreground = new SolidColorBrush(Colors.White);
                    LikeButton.Tag = false;
                }
                else
                {
                    MyLikes like = new MyLikes { UserId = myApp.LoggedInUser, VideoId = currentVideo };
                    if (await api.AddLike(like))
                    {
                        LikeIcon.Foreground = new SolidColorBrush(Colors.Red);
                        LikeButton.Tag = true;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentVideo == null || string.IsNullOrWhiteSpace(currentVideo.VideoAddress)) return;
            VideoLayer.Visibility = Visibility.Visible;
            InlinePlayer.Source = new Uri(currentVideo.VideoAddress, UriKind.RelativeOrAbsolute);
            InlinePlayer.Play();
        }

        private void CloseVideo_Click(object sender, RoutedEventArgs e)
        {
            InlinePlayer.Stop();
            VideoLayer.Visibility = Visibility.Collapsed;
        }

        private void Back_Click(object sender, RoutedEventArgs e) => this.NavigationService.GoBack();

        private void MoreInfoButton_Click(object sender, RoutedEventArgs e)
        {
            double scrollTo = MainScrollViewer.ScrollableHeight;
            DoubleAnimation scrollAnimation = new DoubleAnimation
            {
                From = MainScrollViewer.VerticalOffset,
                To = scrollTo,
                Duration = new Duration(TimeSpan.FromSeconds(0.8)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            this.BeginAnimation(ScrollOffsetProperty, scrollAnimation);
        }

        public static readonly DependencyProperty ScrollOffsetProperty = DependencyProperty.Register("ScrollOffset", typeof(double), typeof(MovieDetails),
            new PropertyMetadata(0.0, OnScrollOffsetChanged));

        private static void OnScrollOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as MovieDetails)?.MainScrollViewer.ScrollToVerticalOffset((double)e.NewValue);
        }

        public BitmapImage Base64ToImage(string base64String)
        {
            try
            {
                if (string.IsNullOrEmpty(base64String)) return null;
                byte[] imageBytes = Convert.FromBase64String(base64String);
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(imageBytes))
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = ms;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    image.Freeze();
                    return image;
                }
            }
            catch { return null; }
        }
    }
}