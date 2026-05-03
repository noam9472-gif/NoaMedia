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

        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.Register("VerticalOffset", typeof(double), typeof(MovieDetails),
            new PropertyMetadata(0.0, OnVerticalOffsetChanged));

        public double VerticalOffset // מאפיין תלוי שמאפשר אנימציה חלקה של גלילה, עוזר לנו בכפתור עוד מידע
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
        // קונסטרקטור שמקבל את הסרט שנבחר ומטפל בטעינת הפרטים שלו
        public MovieDetails(Video selectedVideo)
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            currentVideo = selectedVideo;
            LoadMovieDetails(selectedVideo);
        }
        // פונקציה שמטפלת בטעינת פרטי הסרט והעדכון של כל האלמנטים בממשק בהתאם
        private async void LoadMovieDetails(Video v)
        {
            if (v == null) return;
            // עדכון כל האלמנטים בממשק עם הנתונים של הסרט, כולל טיפול במקרים בהם הנתונים חסרים או לא תקינים
            MovieTitle.Text = v.VideoName?.ToUpper() ?? "UNKNOWN TITLE";
            MovieGenre.Text = v.Genre?.GenreDescription.ToUpper() ?? "GENERAL";
            MovieDuration.Text = v.LengthInMinutes > 0 ? $"{v.LengthInMinutes} MIN" : "";

            if (v.VideoUploadedDate != DateTime.MinValue)
                ReleaseYear.Text = v.VideoUploadedDate.Year.ToString();

            MovieDesc.Text = v.VideoDescription ?? "";
            FullDescriptionText.Text = v.VideoDescription ?? "";
            WhoUploadedName.Text = v.WhoUploadedTheVideo?.Name ?? "Admin";

            try // ניסיון לטעון את תמונת הסרט מהנתונים הקיימים, ואם לא קיימת תמונה תקבל את התמונה מהשרת באמצעות קריאה נוספת לAPI
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
            // בדיקה אם המשתמש מחובר כדי להציג את כפתורי הלייק והרשימה האישית בהתאם
            if (myApp?.LoggedInUser != null) 
            {
                bool hasLiked = await api.CheckIfUserLikedVideo(myApp.LoggedInUser.Id, v.Id);
                LikeButton.Tag = hasLiked;
                LikeIcon.Foreground = hasLiked ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.White);
                // בדיקה אם הסרט כבר נמצא ברשימת הצפייה האישית של המשתמש כדי לעדכן את הממשק בהתאם
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
        // פונקציה שמטפלת בלחיצה על כפתור הוספת ביקורת ומבצעת את כל הבדיקות והפעולות הנדרשות
        private async void AddReviewButton_Click(object sender, RoutedEventArgs e)
        {
            var myApp = Application.Current as App;
            if (myApp?.LoggedInUser == null)
            {
                MessageBox.Show("You must be logged in to add a review."); // הודעה למשתמש אם הוא לא מחובר
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNewReview.Text))
            {
                MessageBox.Show("Cannot add an empty review."); // הודעה למשתמש אם הוא מנסה להוסיף ביקורת ריקה
                return;
            }

            try
            {
                // יצירת אובייקט ביקורת חדש עם הנתונים הדרושים
                VideoReview newReview = new VideoReview
                {
                    WhoUpdatedTheReview = myApp.LoggedInUser,
                    WhichVideoDidTheUserReview = currentVideo,
                    ReviewDate = DateTime.Now,
                    ReviewDescription = txtNewReview.Text
                };
                
                int result = await api.InsertVideoReview(newReview); // קריאה ל-API
                if (result > 0)
                {
                    txtNewReview.Text = "";
                    RefreshReviews();
                    MessageBox.Show("The review was added successfully!");
                }
                else { MessageBox.Show("Error saving the review."); }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }
        // פונקציה שמטפלת בלחיצה על כפתור המחיקה של ביקורת ומבצעת את כל הבדיקות והפעולות הנדרשות
        private async void DeleteReviewBtn_Click(object sender, RoutedEventArgs e)
        {
            //  שליפת כפתור המחיקה והנתונים שלו
            var button = sender as Button;
            var review = button?.DataContext as VideoReview;
            var myApp = Application.Current as App;

            //  בדיקות בטיחות בסיסיות
            if (review == null || myApp?.LoggedInUser == null) return;

            // בדיקה אם המשתמש הוא זה שכתב את הביקורת
            if (review.WhoUpdatedTheReview.Id != myApp.LoggedInUser.Id)
            {
                MessageBox.Show("You are not authorized to delete a review that is not yours.", "Authorization Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            //  אישור מהמשתמש לפני המחיקה הסופית
            var result = MessageBox.Show("Are you sure you want to delete this review?", "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    //  קריאה ל-API
                    int affectedRows = await api.DeleteVideoReview(review.Id);

                    if (affectedRows > 0)
                    {
                        //רענון הרשימה כדי שהביקורת תיעלם מהמסך
                        RefreshReviews();
                    }
                    else
                    {
                        MessageBox.Show("Delete failed. The review may no longer exist in the system.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error connecting to the server: " + ex.Message);
                }
            }
        }
        // פונקציה שמטפלת בלחיצה על כפתור "הוסף לרשימה שלי" ומעדכנת את הממשק בהתאם
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
            // יצירת אנימציה חלקה לגלילה אל התחתית של המידע הנוסף
            DoubleAnimation scrollAnimation = new DoubleAnimation
            {
                From = MainScrollViewer.VerticalOffset,
                To = scrollTo,
                Duration = new Duration(TimeSpan.FromSeconds(0.8)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            }; 

            // הפעלת אנימציית הגלילה כלפי שאר המידע  
            this.BeginAnimation(VerticalOffsetProperty, scrollAnimation);
        }
        // פונקציה שמטפלת בלחיצה על כפתור הלייק ומעדכנת את הממשק בהתאם, כולל בדיקה אם המשתמש כבר לייק או לא
        private async void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            var myApp = Application.Current as App;
            if (myApp?.LoggedInUser == null || currentVideo == null) return;
            bool isLiked = (LikeButton.Tag as bool?) ?? false;

            if (isLiked) // אם הסרט כבר מוסמן בלייק, נבצע את הפעולה של הסרת הלייק מהמערכת ונעדכן את הממשק בהתאם
            {
                await api.RemoveLike(myApp.LoggedInUser.Id, currentVideo.Id);
                LikeIcon.Foreground = Brushes.White; LikeButton.Tag = false;
            }
            else
            {
                // אם הסרט לא מסומן בלייק, נבצע את הפעולה של הוספת הלייק למערכת ונעדכן את הממשק בהתאם
                if (await api.AddLike(new MyLikes { UserId = myApp.LoggedInUser, VideoId = currentVideo }))
                {
                    LikeIcon.Foreground = Brushes.Red; LikeButton.Tag = true;
                }
            }
        }
        // פונקציה שמטפלת בלחיצה על כפתור הניגון ומבצעת את כל הפעולות הנדרשות כדי להציג את נגן הוידאו ולהתחיל לנגן את הסרט
        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            // בדיקה בסיסית לוודא שיש סרט נבחר ושהנתונים שלו תקינים לפני שמנסים לנגן אותו
            if (currentVideo == null || string.IsNullOrWhiteSpace(currentVideo.VideoAddress)) return;
            // הוספת הסרט להיסטוריית הצפייה של המשתמש אם הוא מחובר, כדי לשמור את ההיסטוריה שלו במערכת
            var myApp = Application.Current as App;
            if (myApp?.LoggedInUser != null) await api.InsertMyHistory(new MyHistory { UserId = myApp.LoggedInUser, VideoId = currentVideo });
            // הצגת שכבת הוידאו והגדרת המקור של נגן הוידאו לכתובת של הסרט, ואז התחלת הניגון והפעלת הטיימר לעדכון הממשק בזמן אמת
            VideoLayer.Visibility = Visibility.Visible;
            InlinePlayer.Source = new Uri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Movies", currentVideo.VideoAddress), UriKind.Absolute);
            InlinePlayer.Play();
            timer.Start();
        }
        // פונקציה שמטפלת בלחיצה על כפתור הסגירה של נגן הוידאו ומבצעת את כל הפעולות הנדרשות כדי לעצור את הניגון ולהסתיר את שכבת הוידאו
        private void CloseVideo_Click(object sender, RoutedEventArgs e) { timer.Stop(); InlinePlayer.Stop(); VideoLayer.Visibility = Visibility.Collapsed; }
        // פונקציה שמטפלת באירוע פתיחת המדיה ומעדכנת את הממשק בהתאם לאורך הסרט
        private void InlinePlayer_MediaOpened(object sender, RoutedEventArgs e) { if (InlinePlayer.NaturalDuration.HasTimeSpan) { TimelineSlider.Maximum = InlinePlayer.NaturalDuration.TimeSpan.TotalSeconds; TotalTime.Text = InlinePlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"); } }
        // פונקציה שמטפלת באירוע סיום המדיה ומעדכנת את הממשק בהתאם כדי להחזיר את הנגן למצב ההתחלתי 
        private void Timer_Tick(object sender, EventArgs e) { if (InlinePlayer.NaturalDuration.HasTimeSpan) { TimelineSlider.Value = InlinePlayer.Position.TotalSeconds; TimeStatus.Text = InlinePlayer.Position.ToString(@"mm\:ss"); } }
        // פונקציה שמטפלת באירוע שחרור העכבר על סלאיידר הזמן ומעדכנת את מיקום הניגון בהתאם למיקום החדש של הסלאיידר
        private void TimelineSlider_MouseUp(object sender, MouseButtonEventArgs e) { InlinePlayer.Position = TimeSpan.FromSeconds(TimelineSlider.Value); }
        // פונקציה שמטפלת בלחיצה על כפתור ההאצה ומעדכנת את מיקום הניגון כדי לדלג קדימה ב10 שניות
        private void Forward_Click(object sender, RoutedEventArgs e) => InlinePlayer.Position += TimeSpan.FromSeconds(10);
        // פונקציה שמטפלת בלחיצה על כפתור ההאטה ומעדכנת את מיקום הניגון כדי לדלג אחורה ב10 שניות
        private void Rewind_Click(object sender, RoutedEventArgs e) => InlinePlayer.Position -= TimeSpan.FromSeconds(10);
        // פונקציה שמטפלת באירוע התחלת הטעינה של הוידאו ומציגה את סטטוס הטעינה למשתמש
        private void InlinePlayer_BufferingStarted(object sender, RoutedEventArgs e) => LoadingStatus.Visibility = Visibility.Visible;
        // פונקציה שמטפלת באירוע סיום הטעינה של הוידאו ומסתירה את סטטוס הטעינה מהמשתמש
        private void InlinePlayer_BufferingEnded(object sender, RoutedEventArgs e) => LoadingStatus.Visibility = Visibility.Collapsed;
        // פונקציה שמטפלת בלחיצה על כפתור החזרה ומבצעת את הפעולה של חזרה לעמוד הקודם בממשק
        private void Back_Click(object sender, RoutedEventArgs e) => this.NavigationService.GoBack();
        // פונקציה שממירה מחרוזת Base64 לתמונה מסוג BitmapImage, משמשת לטעינת תמונות מהשרת או מהנתונים הקיימים של הסרט
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