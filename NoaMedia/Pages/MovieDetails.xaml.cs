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
using System.Windows.Input; // נוסף עבור MouseButtonEventArgs
using System.Windows.Threading; // נוסף עבור DispatcherTimer

namespace NoaMedia.Pages
{
    public partial class MovieDetails : Page
    {
        private IInterfaceAPI api = new InterfaceAPI();
        private Video currentVideo;
        private DispatcherTimer timer; // טיימר לעדכון ה-Slider בזמן אמת

        public MovieDetails(Video selectedVideo)
        {
            InitializeComponent();
            currentVideo = selectedVideo;
            LoadMovieDetails(selectedVideo);

            // אתחול הטיימר לעדכון פס ההתקדמות
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
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

        private void AddReviewButton_Click(object sender, RoutedEventArgs e)// הוספת ביקורת
        {
            MessageBox.Show("This feature will allow you to add a review soon!", "Coming Soon");
        }

        private async void LikeButton_Click(object sender, RoutedEventArgs e)// הוספת לייק או הסרתו
        {
            var myApp = Application.Current as App;// בדיקה אם המשתמש מחובר ואם יש סרטון נבחר
            if (myApp?.LoggedInUser == null || currentVideo == null) return;// בדיקה אם המשתמש כבר שם לייק על הסרטון
            bool isLiked = (LikeButton.Tag as bool?) ?? false;// ניסיון להוסיף או להסיר לייק בהתאם למצב הנוכחי

            try
            {
                if (isLiked)// אם כבר יש לייק, הפעולה מסירה אותו
                {
                    await api.RemoveLike(myApp.LoggedInUser.Id, currentVideo.Id);// הסרת הלייק מהאקסס
                    LikeIcon.Foreground = new SolidColorBrush(Colors.White);// עדכון האייקון בהתאם
                    LikeButton.Tag = false;// אם אין לייק, הפעולה מוסיפה אותו
                }
                else
                {
                    MyLikes like = new MyLikes { UserId = myApp.LoggedInUser, VideoId = currentVideo };// יצירת אובייקט לייק חדש
                    if (await api.AddLike(like))// הוספת הלייק לאקסס
                    {
                        LikeIcon.Foreground = new SolidColorBrush(Colors.Red);// עדכון האייקון בהתאם
                        LikeButton.Tag = true;// עדכון התג כדי לציין שיש לייק
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)// כפתור הפעלת הסרטון
        {
            // בדיקה אם יש סרטון ואם יש לו כתובת
            if (currentVideo == null || string.IsNullOrWhiteSpace(currentVideo.VideoAddress)) return;

            VideoLayer.Visibility = Visibility.Visible;

            // יצירת נתיב יחסי לתיקייה Movies שנמצאת לצד ה-EXE
            // אנחנו משתמשים ב-Path.Combine כדי שזה יעבוד נכון בווינדוס
            string movieFileName = currentVideo.VideoAddress; // נניח שזה "my_movie.mp4"
            string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Movies", movieFileName);

            // הגדרת המקור לנגן והפעלה
            InlinePlayer.Source = new Uri(fullPath, UriKind.Absolute);
            InlinePlayer.Play();
            timer.Start(); // הפעלת הטיימר לעדכון ה-Slider
        }
        private void CloseVideo_Click(object sender, RoutedEventArgs e)// כפתור סגירת הסרטון
        {
            timer.Stop(); // עצירת הטיימר
            InlinePlayer.Stop();
            VideoLayer.Visibility = Visibility.Collapsed;
        }

        // פונקציה שנקראת ברגע שהקובץ נפתח באמת ומזהה את האורך המדויק
        private void InlinePlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (InlinePlayer.NaturalDuration.HasTimeSpan)
            {
                TimeSpan ts = InlinePlayer.NaturalDuration.TimeSpan;
                TimelineSlider.Maximum = ts.TotalSeconds;

                // הצגת זמן בפורמט של שעות אם הסרט ארוך משעה
                TotalTime.Text = ts.TotalHours >= 1 ? ts.ToString(@"hh\:mm\:ss") : ts.ToString(@"mm\:ss");
            }
        }

        // עדכון ה-Timer כדי שיציג שעות במידת הצורך
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (InlinePlayer.NaturalDuration.HasTimeSpan)
            {
                TimelineSlider.Value = InlinePlayer.Position.TotalSeconds;

                // עדכון טקסט הזמן הנוכחי
                TimeStatus.Text = InlinePlayer.Position.TotalHours >= 1 ?
                                  InlinePlayer.Position.ToString(@"hh\:mm\:ss") :
                                  InlinePlayer.Position.ToString(@"mm\:ss");
            }
        }

        // פונקציה שמאפשרת למשתמש להריץ קדימה ואחורה
        private void TimelineSlider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // הזזת הסרט למיקום שנבחר ב-Slider
            InlinePlayer.Position = TimeSpan.FromSeconds(TimelineSlider.Value);
        }

        // כפתורים לדילוג מהיר (נוסף לבקשתך)
        private void Forward_Click(object sender, RoutedEventArgs e) => InlinePlayer.Position += TimeSpan.FromSeconds(10);
        private void Rewind_Click(object sender, RoutedEventArgs e) => InlinePlayer.Position -= TimeSpan.FromSeconds(10);

        // ניהול מצבי טעינה (Buffering)
        private void InlinePlayer_BufferingStarted(object sender, RoutedEventArgs e) => LoadingStatus.Visibility = Visibility.Visible;
        private void InlinePlayer_BufferingEnded(object sender, RoutedEventArgs e) => LoadingStatus.Visibility = Visibility.Collapsed;

        private void Back_Click(object sender, RoutedEventArgs e) => this.NavigationService.GoBack();// כפתור חזרה

        private void MoreInfoButton_Click(object sender, RoutedEventArgs e)// כפתור הצגת תיאור סרט מלא
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

        public static readonly DependencyProperty ScrollOffsetProperty = DependencyProperty.Register("ScrollOffset", typeof(double), typeof(MovieDetails), new PropertyMetadata(0.0, OnScrollOffsetChanged)); // יצירת הפונקציה שתגרום לאנימציה של גלילה חלקה

        private static void OnScrollOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)// פונקציה שמעדכנת את מיקום הגלילה בהתאם לערך האנימציה
        {
            (d as MovieDetails)?.MainScrollViewer.ScrollToVerticalOffset((double)e.NewValue);
        }

        public BitmapImage Base64ToImage(string base64String)// פנוקציה להמרת סטרינג לבייס64 לתמונה- ההרחבה
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

        // הערה: אל תשכח להוסיף את הפונקציה הזו אם יש כפתור למשתמש בביקורות
        private void ReviewUser_Click(object sender, RoutedEventArgs e) { /* לוגיקה למעבר לפרופיל משתמש */ }
    }
}