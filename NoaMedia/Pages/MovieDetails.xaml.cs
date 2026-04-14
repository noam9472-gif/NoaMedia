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

        public MovieDetails(Video selectedVideo)
        {
            InitializeComponent();
            currentVideo = selectedVideo;
            LoadMovieDetails(selectedVideo);
            CheckPremiumForMyList();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        private async void LoadMovieDetails(Video v)
        {
            if (v == null) return;

            // עדכון טקסטים
            MovieTitle.Text = v.VideoName?.ToUpper() ?? "UNKNOWN TITLE";
            MovieGenre.Text = v.Genre?.GenreDescription.ToUpper() ?? "GENERAL";
            MovieDuration.Text = v.LengthInMinutes > 0 ? $"{v.LengthInMinutes} MIN" : "";

            if (v.VideoUploadedDate != DateTime.MinValue)
                ReleaseYear.Text = v.VideoUploadedDate.Year.ToString();

            MovieDesc.Text = v.VideoDescription ?? "";
            FullDescriptionText.Text = v.VideoDescription ?? "";
            WhoUploadedName.Text = v.WhoUploadedTheVideo?.UserName ?? "Admin";

            // טעינת תמונה:רקע ופוסטר
            try
            {
                BitmapImage movieImg = null;
                if (!string.IsNullOrEmpty(v.VideoPic) && !v.VideoPic.Contains("found"))
                {
                    movieImg = Base64ToImage(v.VideoPic);
                }
                else
                {
                    string base64 = await api.GetVideoPicByte64(v.Id);
                    if (!string.IsNullOrEmpty(base64))
                        movieImg = Base64ToImage(base64);
                }

                if (movieImg != null)
                {
                    BackgroundImage.Source = movieImg;
                    MainPosterImage.Source = movieImg;
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Error: " + ex.Message); }

            // טעינת ביקורות
            try
            {
                lstMovieReviews.ItemsSource = null;
                var reviews = await api.GetReviewsByVideoId(v.Id);
                lstMovieReviews.ItemsSource = reviews;
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Error loading reviews: " + ex.Message); }

            // בדיקת לייק
            var myApp = Application.Current as App;
            if (myApp?.LoggedInUser != null)
            {
                bool hasLiked = await api.CheckIfUserLikedVideo(myApp.LoggedInUser.Id, v.Id);
                LikeButton.Tag = hasLiked;
                LikeIcon.Foreground = hasLiked ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.White);
            }


            // בדיקה האם הסרט כבר נמצא ב-WatchList (הרשימה שלי)
            if (myApp?.LoggedInUser != null)
            {
                // הערה: וודא שיש לך מתודה כזו ב-API, בדומה ל-CheckIfUserLikedVideo
                bool isInWatchList = await api.CheckIfUserInWatchList(myApp.LoggedInUser.Id, v.Id);

                // שמירת הסטטוס ב-Tag של הכפתור (כמו בלייק)
                MyListButton.Tag = isInWatchList;

                // עדכון האייקון: אם נמצא ברשימה נציג V, אם לא נציג + (או מה שבחרת ב-XAML)
                MyListIcon.Text = isInWatchList ? "✓" : "+";
                MyListIcon.Foreground = isInWatchList ? new SolidColorBrush(Colors.Gold) : new SolidColorBrush(Colors.White);
            }


        }

        private void CheckPremiumForMyList()
        {
            var currentUser = (Application.Current as App).LoggedInUser;
            if (currentUser != null && currentUser.IsAdmin)
            {
                MyListButton.Visibility = Visibility.Visible;
            }
        }

        private async void MyListButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentUser = (Application.Current as App).LoggedInUser;
                if (currentUser == null || currentVideo == null) return;

                bool alreadyInList = (MyListButton.Tag as bool?) ?? false;

                if (alreadyInList)
                {
                    bool removed = await api.DeleteMyWatchList(currentUser.Id, currentVideo.Id);
                    if (removed)
                    {
                        // חזרה למצב רגיל: פלוס לבן ורקע שקוף
                        MyListIcon.Text = "+";
                        MyListIcon.Foreground = Brushes.White;
                        MyListButton.Background = new SolidColorBrush(Color.FromArgb(51, 255, 255, 255)); // #33FFFFFF
                        MyListButton.Tag = false;
                    }
                }
                else
                {
                    var watch = new MyWatchList { UserId = currentUser, VideoId = currentVideo };
                    int result = await api.InsertMyWatchList(watch);

                    if (result > 0)
                    {
                        // מצב מסומן: וי (V) בצבע זהב/אדום ורקע קצת יותר כהה
                        MyListIcon.Text = "✓";
                        MyListIcon.Foreground = Brushes.Gold;
                        MyListButton.Background = new SolidColorBrush(Color.FromArgb(80, 255, 215, 0)); // זהב שקוף מעט
                        MyListButton.Tag = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

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

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentVideo == null || string.IsNullOrWhiteSpace(currentVideo.VideoAddress)) return;

            // --- רישום היסטוריית צפייה ---
            var myApp = Application.Current as App;
            if (myApp?.LoggedInUser != null)
            {
                try
                {
                    MyHistory historyEntry = new MyHistory
                    {
                        UserId = myApp.LoggedInUser,
                        VideoId = currentVideo
                    };
                    await api.InsertMyHistory(historyEntry);
                }
                catch (Exception ex)
                {
                    // אנחנו לא רוצים להפסיק את הסרט אם הרישום נכשל, רק מדפיסים לדיבאג
                    System.Diagnostics.Debug.WriteLine("Failed to record history: " + ex.Message);
                }
            }
            // -----------------------------

            VideoLayer.Visibility = Visibility.Visible;
            string movieFileName = currentVideo.VideoAddress;
            string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Movies", movieFileName);

            InlinePlayer.Source = new Uri(fullPath, UriKind.Absolute);
            InlinePlayer.Play();
            timer.Start();
        }

        private void CloseVideo_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            InlinePlayer.Stop();
            VideoLayer.Visibility = Visibility.Collapsed;
        }

        private void InlinePlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (InlinePlayer.NaturalDuration.HasTimeSpan)
            {
                TimeSpan ts = InlinePlayer.NaturalDuration.TimeSpan;
                TimelineSlider.Maximum = ts.TotalSeconds;
                TotalTime.Text = ts.TotalHours >= 1 ? ts.ToString(@"hh\:mm\:ss") : ts.ToString(@"mm\:ss");
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (InlinePlayer.NaturalDuration.HasTimeSpan)
            {
                TimelineSlider.Value = InlinePlayer.Position.TotalSeconds;
                TimeStatus.Text = InlinePlayer.Position.TotalHours >= 1 ?
                                  InlinePlayer.Position.ToString(@"hh\:mm\:ss") :
                                  InlinePlayer.Position.ToString(@"mm\:ss");
            }
        }

        private void TimelineSlider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            InlinePlayer.Position = TimeSpan.FromSeconds(TimelineSlider.Value);
        }

        private void Forward_Click(object sender, RoutedEventArgs e) => InlinePlayer.Position += TimeSpan.FromSeconds(10);
        private void Rewind_Click(object sender, RoutedEventArgs e) => InlinePlayer.Position -= TimeSpan.FromSeconds(10);
        private void InlinePlayer_BufferingStarted(object sender, RoutedEventArgs e) => LoadingStatus.Visibility = Visibility.Visible;
        private void InlinePlayer_BufferingEnded(object sender, RoutedEventArgs e) => LoadingStatus.Visibility = Visibility.Collapsed;
        private void Back_Click(object sender, RoutedEventArgs e) => this.NavigationService.GoBack();

        public static readonly DependencyProperty ScrollOffsetProperty =
            DependencyProperty.Register("ScrollOffset", typeof(double), typeof(MovieDetails), new PropertyMetadata(0.0, OnScrollOffsetChanged));

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

        private void ReviewUser_Click(object sender, RoutedEventArgs e) { }
    }
}