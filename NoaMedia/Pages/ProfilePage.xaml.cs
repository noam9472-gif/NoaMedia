using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Model;
using ApiInterface;

namespace NoaMedia.Pages
{
    public partial class ProfilePage : Page
    {
        private IInterfaceAPI api = new InterfaceAPI();
        private User currentUser;

        public ProfilePage(string currentName)
        {
            InitializeComponent();
            currentUser = (Application.Current as App).LoggedInUser;

            if (currentUser != null)
            {
                UserNameHeading.Text = currentUser.UserName;
                // במקום לקרוא לזה כאן, אנחנו נשתמש ב-Page_Loaded
            }

            // הוספת האירוע שטוען את התוכן בכל פעם שנכנסים לדף
            this.Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMyContent(); // עכשיו זה ירוץ בכל פעם שהדף מוצג
        }

        private async void LoadMyContent()
        {
            try
            {
                // 1. שליפת כל הנתונים מה-API
                var allVideos = (VideoList)await api.GetAllVideos();
                var allLikes = (MyLikesList)await api.GetAllLikes(); // הפונקציה החדשה שהוספנו לממשק

                // 2. סינון הסרטונים שהעליתי (הקוד הקיים שלך)
                var myVideos = allVideos.Where(v => v.WhoUploadedTheVideo != null &&
                                               v.WhoUploadedTheVideo.Id == currentUser.Id).ToList();

                MyVideosPanel.Children.Clear();
                foreach (var v in myVideos)
                {
                    MyVideosPanel.Children.Add(CreateMovieThumbnail(v));
                }

                // 3. סינון והצגת הלייקים (החלק החדש!)
                LikedVideosPanel.Children.Clear();
                if (allLikes != null)
                {
                    // מוצאים את כל הלייקים ששייכים למשתמש הנוכחי
                    // בתוך LoadMyContent
                    var myFavoriteLikes = allLikes.Where(l =>
                        l.UserId != null &&
                        currentUser != null &&
                        l.UserId.Id == currentUser.Id // השוואה ישירה של מספרים
                    ).ToList();
                    foreach (var like in myFavoriteLikes)
                    {
                        // לכל לייק יש אובייקט VideoId (שזה הסרט המלא) בזכות ה-CreateModel שבנינו בשרת
                        if (like.VideoId != null)
                        {
                            LikedVideosPanel.Children.Add(CreateMovieThumbnail(like.VideoId));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("טעינה נכשלה: " + ex.Message);
            }
        }


        private Button CreateMovieThumbnail(Video v)
        {
            Button btn = new Button
            {
                Content = v.VideoName,
                Width = 150,
                Height = 100,
                Margin = new Thickness(10),
                Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#222")),
                Foreground = System.Windows.Media.Brushes.White,
                BorderThickness = new Thickness(1),
                BorderBrush = System.Windows.Media.Brushes.Gray,
                Cursor = System.Windows.Input.Cursors.Hand,
                FontWeight = FontWeights.Bold
            };

            btn.Click += (s, e) => this.NavigationService.Navigate(new MovieDetails(v));
            return btn;
        }

        private async void LoadLikedMovies()
        {
            var myApp = Application.Current as App;
            if (myApp?.LoggedInUser == null) return;

            try
            {
                // 1. שליפה מה-API
                MyLikesList allLikes = await api.GetAllLikes();

                // 2. ניקוי הפאנל
                LikedVideosPanel.Children.Clear();

                if (allLikes != null)
                {
                    // סינון חכם יותר: בודקים אם UserId קיים ואם ה-ID תואם
                    // המרנו את ה-ID ל-int כדי לוודא שאין השוואה בין סוגים שונים
                    var myFavoriteLikes = allLikes
                        .Where(l => l.UserId != null && l.UserId.Id == myApp.LoggedInUser.Id)
                        .ToList();

                    foreach (var like in myFavoriteLikes)
                    {
                        // חשוב: אם VideoId הוא null, זה אומר שהשרת לא החזיר את פרטי הסרט
                        if (like.VideoId != null)
                        {
                            LikedVideosPanel.Children.Add(CreateMovieThumbnail(like.VideoId));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("שגיאה בטעינת לייקים: " + ex.Message);
            }
        }

       

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // ניקוי המשתמש המחובר
            if (Application.Current is App myApp)
            {
                myApp.LoggedInUser = null;
            }

            this.NavigationService.Navigate(new NoaMedia.Pages.Log_in());
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService.CanGoBack)
                this.NavigationService.GoBack();
        }

        
    }
}