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
using System.Windows.Input;
using System.Windows.Threading;

namespace NoaMedia.Pages
{
    public partial class MovieDetails : Page
    {
        private IInterfaceAPI api = new InterfaceAPI();
        private Video currentVideo;
        private DispatcherTimer timer;

        // Dependency Property המאפשר להפעיל אנימציה על גלילה אנכית
        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.Register("VerticalOffset", typeof(double), typeof(MovieDetails),
            new PropertyMetadata(0.0, OnVerticalOffsetChanged));

        public double VerticalOffset
        {
            get => (double)GetValue(VerticalOffsetProperty);
            set => SetValue(VerticalOffsetProperty, value);
        }

        private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MovieDetails page && page.MainScrollViewer != null)
            {
                page.MainScrollViewer.ScrollToVerticalOffset((double)e.NewValue);
            }
        }

        public MovieDetails(Video selectedVideo)
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            currentVideo = selectedVideo;
            LoadMovieDetails(selectedVideo);
        }

        private async void LoadMovieDetails(Video v)
        {
            if (v == null) return;

            MovieTitle.Text = v.VideoName?.ToUpper() ?? "UNKNOWN TITLE";
            MovieGenre.Text = v.Genre?.GenreDescription.ToUpper() ?? "GENERAL";
            MovieDuration.Text = v.LengthInMinutes > 0 ? $"{v.LengthInMinutes} MIN" : "";

            if (v.VideoUploadedDate != DateTime.MinValue)
                ReleaseYear.Text = v.VideoUploadedDate.Year.ToString();

            MovieDesc.Text = v.VideoDescription ?? "";
            FullDescriptionText.Text = v.VideoDescription ?? "";
            WhoUploadedName.Text = v.WhoUploadedTheVideo?.Name ?? "Admin";

            try
            {
                BitmapImage movieImg = null;
                if (!string.IsNullOrEmpty(v.VideoPic) && !v.VideoPic.Contains("found"))
                    movieImg = Base64ToImage(v.VideoPic);
                else
                {
                    string base64 = await api.GetVideoPicByte64(v.Id);
                    if (!string.IsNullOrEmpty(base64)) movieImg = Base64ToImage(base64);
                }

                if (movieImg != null)
                {
                    BackgroundImage.Source = movieImg;
                    MainPosterImage.Source = movieImg;
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Error: " + ex.Message); }

            RefreshReviews();

            var myApp = Application.Current as App;
            if (myApp?.LoggedInUser != null)
            {
                bool hasLiked = await api.CheckIfUserLikedVideo(myApp.LoggedInUser.Id, v.Id);
                LikeButton.Tag = hasLiked;
                LikeIcon.Foreground = hasLiked ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.White);

                bool isInWatchList = await api.CheckIfUserInWatchList(myApp.LoggedInUser.Id, v.Id);
                MyListButton.Tag = isInWatchList;
                MyListIcon.Text = isInWatchList ? "✓" : "+";
                MyListIcon.Foreground = isInWatchList ? new SolidColorBrush(Colors.Gold) : new SolidColorBrush(Colors.White);

                MyListButton.Visibility = Visibility.Visible;
            }
        }

        private async void RefreshReviews()
        {
            if (currentVideo == null || lstMovieReviews == null) return;
            try
            {
                var reviews = await api.GetReviewsByVideoId(currentVideo.Id);
                lstMovieReviews.ItemsSource = reviews;
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Review Load Error: " + ex.Message); }
        }

        private async void AddReviewButton_Click(object sender, RoutedEventArgs e)
        {
            var myApp = Application.Current as App;
            if (myApp?.LoggedInUser == null)
            {
                MessageBox.Show("עליך להתחבר כדי להוסיף ביקורת.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNewReview.Text))
            {
                MessageBox.Show("לא ניתן להוסיף ביקורת ריקה.");
                return;
            }

            try
            {
                VideoReview newReview = new VideoReview
                {
                    WhoUpdatedTheReview = myApp.LoggedInUser,
                    WhichVideoDidTheUserReview = currentVideo,
                    ReviewDate = DateTime.Now,
                    ReviewDescription = txtNewReview.Text
                };

                int result = await api.InsertVideoReview(newReview);
                if (result > 0)
                {
                    txtNewReview.Text = "";
                    RefreshReviews();
                    MessageBox.Show("הביקורת נוספה בהצלחה!");
                }
                else { MessageBox.Show("שגיאה בשמירת הביקורת."); }
            }
            catch (Exception ex) { MessageBox.Show("שגיאה: " + ex.Message); }
        }

        private async void DeleteReviewBtn_Click(object sender, RoutedEventArgs e)
        {
            // 1. שליפת כפתור המחיקה והנתונים שלו
            var button = sender as Button;
            var review = button?.DataContext as VideoReview;
            var myApp = Application.Current as App;

            // 2. בדיקות בטיחות בסיסיות
            if (review == null || myApp?.LoggedInUser == null) return;

            // 3. הווידוא הקריטי: האם המשתמש המחובר הוא אכן כותב הביקורת?
            if (review.WhoUpdatedTheReview.Id != myApp.LoggedInUser.Id)
            {
                MessageBox.Show("אינך מורשה למחוק ביקורת שאינה שלך.", "שגיאת הרשאה", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            // 4. אישור מהמשתמש לפני המחיקה הסופית
            var result = MessageBox.Show("האם אתה בטוח שברצונך למחוק את הביקורת לצמיתות?", "אישור מחיקה", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // 5. קריאה ל-API (לפי המתודה הקיימת אצלך שמחזירה Task<int>)
                    int affectedRows = await api.DeleteVideoReview(review.Id);

                    if (affectedRows > 0)
                    {
                        // 6. רענון הרשימה כדי שהביקורת תיעלם מהמסך
                        RefreshReviews();
                    }
                    else
                    {
                        MessageBox.Show("המחיקה נכשלה. ייתכן שהביקורת כבר לא קיימת במערכת.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("שגיאה בחיבור לשרת: " + ex.Message);
                }
            }
        }

        private async void MyListButton_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = (Application.Current as App).LoggedInUser;
            if (currentUser == null || currentVideo == null) return;
            bool alreadyInList = (MyListButton.Tag as bool?) ?? false;

            if (alreadyInList)
            {
                if (await api.DeleteMyWatchList(currentUser.Id, currentVideo.Id))
                {
                    MyListIcon.Text = "+"; MyListIcon.Foreground = Brushes.White;
                    MyListButton.Tag = false;
                }
            }
            else
            {
                var watch = new MyWatchList { UserId = currentUser, VideoId = currentVideo };
                if (await api.InsertMyWatchList(watch) > 0)
                {
                    MyListIcon.Text = "✓"; MyListIcon.Foreground = Brushes.Gold;
                    MyListButton.Tag = true;
                }
            }
        }

        private void MoreInfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainScrollViewer == null) return;

            double scrollTo = MainScrollViewer.ScrollableHeight;
            DoubleAnimation scrollAnimation = new DoubleAnimation
            {
                From = MainScrollViewer.VerticalOffset,
                To = scrollTo,
                Duration = new Duration(TimeSpan.FromSeconds(0.8)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            // הפעלת האנימציה דרך ה-Dependency Property שיצרנו
            this.BeginAnimation(VerticalOffsetProperty, scrollAnimation);
        }

        private async void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            var myApp = Application.Current as App;
            if (myApp?.LoggedInUser == null || currentVideo == null) return;
            bool isLiked = (LikeButton.Tag as bool?) ?? false;

            if (isLiked)
            {
                await api.RemoveLike(myApp.LoggedInUser.Id, currentVideo.Id);
                LikeIcon.Foreground = Brushes.White; LikeButton.Tag = false;
            }
            else
            {
                if (await api.AddLike(new MyLikes { UserId = myApp.LoggedInUser, VideoId = currentVideo }))
                {
                    LikeIcon.Foreground = Brushes.Red; LikeButton.Tag = true;
                }
            }
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentVideo == null || string.IsNullOrWhiteSpace(currentVideo.VideoAddress)) return;
            var myApp = Application.Current as App;
            if (myApp?.LoggedInUser != null) await api.InsertMyHistory(new MyHistory { UserId = myApp.LoggedInUser, VideoId = currentVideo });

            VideoLayer.Visibility = Visibility.Visible;
            InlinePlayer.Source = new Uri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Movies", currentVideo.VideoAddress), UriKind.Absolute);
            InlinePlayer.Play();
            timer.Start();
        }

        private void CloseVideo_Click(object sender, RoutedEventArgs e) { timer.Stop(); InlinePlayer.Stop(); VideoLayer.Visibility = Visibility.Collapsed; }
        private void InlinePlayer_MediaOpened(object sender, RoutedEventArgs e) { if (InlinePlayer.NaturalDuration.HasTimeSpan) { TimelineSlider.Maximum = InlinePlayer.NaturalDuration.TimeSpan.TotalSeconds; TotalTime.Text = InlinePlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"); } }
        private void Timer_Tick(object sender, EventArgs e) { if (InlinePlayer.NaturalDuration.HasTimeSpan) { TimelineSlider.Value = InlinePlayer.Position.TotalSeconds; TimeStatus.Text = InlinePlayer.Position.ToString(@"mm\:ss"); } }
        private void TimelineSlider_MouseUp(object sender, MouseButtonEventArgs e) { InlinePlayer.Position = TimeSpan.FromSeconds(TimelineSlider.Value); }
        private void Forward_Click(object sender, RoutedEventArgs e) => InlinePlayer.Position += TimeSpan.FromSeconds(10);
        private void Rewind_Click(object sender, RoutedEventArgs e) => InlinePlayer.Position -= TimeSpan.FromSeconds(10);
        private void InlinePlayer_BufferingStarted(object sender, RoutedEventArgs e) => LoadingStatus.Visibility = Visibility.Visible;
        private void InlinePlayer_BufferingEnded(object sender, RoutedEventArgs e) => LoadingStatus.Visibility = Visibility.Collapsed;
        private void Back_Click(object sender, RoutedEventArgs e) => this.NavigationService.GoBack();

        public BitmapImage Base64ToImage(string base64String)
        {
            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64String);
                using (var ms = new System.IO.MemoryStream(imageBytes))
                {
                    var image = new BitmapImage();
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