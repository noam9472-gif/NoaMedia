using ApiInterface;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
                ProfileInitialText.Text = currentUser.UserName.Substring(0, 1).ToUpper();

                // בדיקה אם המשתמש הוא פרימיום (בהנחה שיש שדה IsPremium במודל User)
                if (currentUser.IsAdmin)
                    PremiumBadge.Visibility = Visibility.Visible;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MenuListBox.SelectedIndex = 0; // טעינת ברירת המחדל
        }

        private async void LoadContent(string category)
        {
            MainDisplayPanel.Children.Clear();
            CommentsDisplayPanel.Children.Clear();
            PremiumLockPanel.Visibility = Visibility.Collapsed;
            ContentScrollViewer.Visibility = Visibility.Visible;

            try
            {
                switch (category)
                {
                    case "MyVideos":
                        SectionTitle.Text = "My Videos";
                        var allVideos = (VideoList)await api.GetAllVideos();
                        var myVideos = allVideos.Where(v => v.WhoUploadedTheVideo?.Id == currentUser.Id).ToList();
                        FillVideoPanel(myVideos);
                        break;

                    case "Liked":
                        SectionTitle.Text = "Liked Videos";
                        // *** כאן אתה מחליף את התוכן הישן בחדש ***
                        var allLikes = (MyLikesList)await api.GetAllLikes();

                        var myLikedVideos = allLikes
                            .Where(l => l.UserId?.Id == currentUser.Id)
                            .Select(l => l.VideoId)
                            .ToList();

                        FillVideoPanel(myLikedVideos);
                        break;

                    case "Watched":
                        SectionTitle.Text = "Watched History";
                        // כאן תוכל להוסיף לוגיקה של היסטוריית צפייה מה-API בעתיד
                        break;

                    case "MyList":
                        SectionTitle.Text = "My List";
                        if (!currentUser.IsAdmin)
                        {
                            ContentScrollViewer.Visibility = Visibility.Collapsed;
                            PremiumLockPanel.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            // לוגיקה לטעינת "הרשימה שלי" מה-API
                        }
                        break;

                    case "Comments":
                        SectionTitle.Text = "My Comments";
                        MainDisplayPanel.Visibility = Visibility.Collapsed;
                        CommentsDisplayPanel.Visibility = Visibility.Visible;
                        // לוגיקה לטעינת תגובות
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Loading failed: " + ex.Message);
            }
        }

        private async void FillVideoPanel(IEnumerable<Video> videos)
        {
            MainDisplayPanel.Visibility = Visibility.Visible;
            CommentsDisplayPanel.Visibility = Visibility.Collapsed;

            foreach (var v in videos)
            {
                if (v == null) continue;

                // בדיקה אם ה-Base64 חסר, בדיוק כמו ב-Home
                if (string.IsNullOrEmpty(v.VideoPic) || v.VideoPic.StartsWith("File"))
                {
                    v.VideoPic = await api.GetVideoPicByte64(v.Id);
                }

                MainDisplayPanel.Children.Add(CreateMovieThumbnail(v));
            }
        }

        private void MenuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MenuListBox.SelectedItem is TextBlock selectedItem)
            {
                LoadContent(selectedItem.Tag.ToString());
            }
        }

        private Button CreateMovieThumbnail(Video v)
        {
            Button btn = new Button
            {
                Width = 150,
                Height = 220,
                Margin = new Thickness(10),
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };

            // הגדרת גודל ושיטת מתיחה לתמונה
            Image movieImg = new Image
            {
                Stretch = Stretch.UniformToFill,
                Width = 150,
                Height = 220
            };

            try
            {
                if (v != null && !string.IsNullOrEmpty(v.VideoPic))
                {
                    // שימוש בשיטה מה-Home: המרה מ-Base64
                    movieImg.Source = Base64ToImage(v.VideoPic);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Image Load Error: " + ex.Message);
            }

            Border mask = new Border
            {
                CornerRadius = new CornerRadius(10),
                ClipToBounds = true,
                Child = movieImg
            };

            btn.Content = mask;
            btn.Click += (s, e) => this.NavigationService.Navigate(new MovieDetails(v));

            return btn;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current is App myApp) myApp.LoggedInUser = null;
            this.NavigationService.Navigate(new NoaMedia.Pages.Log_in());
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService.CanGoBack) this.NavigationService.GoBack();
        }

        public BitmapImage Base64ToImage(string base64String)
        {
            try
            {
                if (string.IsNullOrEmpty(base64String) || base64String.StartsWith("File")) return null;
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