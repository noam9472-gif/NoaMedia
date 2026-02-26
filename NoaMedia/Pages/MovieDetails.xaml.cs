using ApiInterface;
using Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace NoaMedia.Pages
{
    public partial class MovieDetails : Page
    {
        private InterfaceAPI api = new InterfaceAPI();
        private Video currentVideo;

        public MovieDetails(Video selectedVideo)
        {
            InitializeComponent();
            currentVideo = selectedVideo; // שמירת הסרט שנבחר
            LoadMovieDetails(selectedVideo);
        }

        private async void LoadMovieDetails(Video v)
        {
            if (v == null) return;

            MovieTitle.Text = v.VideoName;
            MovieGenre.Text = v.Genre?.GenreDescription ?? "General";
            MovieDuration.Text = v.LengthInMinutes > 0 ? $"{v.LengthInMinutes} min" : "";
            string description = !string.IsNullOrWhiteSpace(v.VideoDescription) ? v.VideoDescription : "No description available for this movie.";
            MovieDesc.Text = description;
            FullDescriptionText.Text = description;

            // מי העלה ומתי
            if (WhoUploadedName != null)
            {
                WhoUploadedName.Text = v.WhoUploadedTheVideo?.UserName ?? "Admin";
            }

            if (v.VideoUploadedDate != DateTime.MinValue)
            {
                ReleaseYear.Text = v.VideoUploadedDate.Year.ToString(); // בדרך כלל מציגים רק שנה בפרטי סרט
            }

            try
            {
                if (!string.IsNullOrEmpty(v.VideoPic) && !v.VideoPic.Contains("found"))
                {
                    BackgroundImage.Source = Base64ToImage(v.VideoPic);
                }
                else
                {
                    string base64 = await api.GetVideoPicByte64(v.Id);
                    if (!string.IsNullOrEmpty(base64))
                    {
                        BackgroundImage.Source = Base64ToImage(base64);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading image: " + ex.Message);
            }

            var myApp = Application.Current as App;
            if (myApp?.LoggedInUser != null)
            {
                // בדיקה מול ה-API האם המשתמש עשה לייק
                bool hasLiked = await api.CheckIfUserLikedVideo(myApp.LoggedInUser.Id, v.Id);

                // שמירת הסטטוס בתוך ה-Tag של הכפתור
                LikeButton.Tag = hasLiked;

                // עדכון הצבע בהתאם
                LikeIcon.Foreground = hasLiked ?
                    new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red) :
                    new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            }
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
                    // 1. הסרה מהמסד דרך ה-API
                    await api.RemoveLike(myApp.LoggedInUser.Id, currentVideo.Id);

                    // 2. עדכון ויזואלי
                    LikeIcon.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                    LikeButton.Tag = false;
                }
                else
                {
                    // 1. הוספה למסד דרך ה-API
                    // וודא שב-InterfaceAPI הפונקציה AddLike קוראת ל-Insert של ה-Service ב-Basis
                    MyLikes like = new MyLikes
                    {
                        Id = 0, // לפעמים השרת חייב לראות את השדה הזה
                        UserId = myApp.LoggedInUser,
                        VideoId = currentVideo
                    };
                    bool success = await api.AddLike(like);

                    if (success)
                    {
                        LikeIcon.Foreground = new SolidColorBrush(Colors.Red);
                        LikeButton.Tag = true;
                    }
                    else
                    {
                        MessageBox.Show("השרת לא אישר את הלייק. בדוק את החיבור למסד הנתונים.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בשמירת הלייק: " + ex.Message);
            }
        }


        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (currentVideo == null || string.IsNullOrWhiteSpace(currentVideo.VideoAddress))
                {
                    MessageBox.Show("Error: Video address is missing in the database.", "Missing Data");
                    return;
                }

                string address = currentVideo.VideoAddress;

                if (!address.Contains("://") && !System.IO.Path.IsPathRooted(address))
                {
                    address = "http://" + address;
                }

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

        private void MoreInfoButton_Click(object sender, RoutedEventArgs e)
        {

            double scrollTo = MainScrollViewer.ScrollableHeight;

            // יצירת האנימציה - נמשכת 0.8 שניות עם אפקט האטה בסוף (EaseOut)
            DoubleAnimation scrollAnimation = new DoubleAnimation
            {
                From = MainScrollViewer.VerticalOffset,
                To = scrollTo,
                Duration = new Duration(TimeSpan.FromSeconds(0.8)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            // הפעלת האנימציה 
            this.BeginAnimation(ScrollOffsetProperty, scrollAnimation);
        }

        // ---  שמאפשרת לאנימציה לעבוד ScrollViewer ---
        public static readonly DependencyProperty ScrollOffsetProperty = DependencyProperty.Register("ScrollOffset", typeof(double), typeof(MovieDetails),
            new PropertyMetadata(0.0, OnScrollOffsetChanged));

        private static void OnScrollOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var page = d as MovieDetails;
            page?.MainScrollViewer.ScrollToVerticalOffset((double)e.NewValue);
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
                    return image;
                }
            }
            catch { return null; }
        }
    }
}