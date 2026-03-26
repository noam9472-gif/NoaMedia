using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Model;
using ApiInterface;
using System.Collections.Generic;

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
                        var allLikes = (MyLikesList)await api.GetAllLikes();
                        var myLikes = allLikes.Where(l => l.UserId?.Id == currentUser.Id).Select(l => l.VideoId).ToList();
                        FillVideoPanel(myLikes);
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

        private void FillVideoPanel(IEnumerable<Video> videos)
        {
            MainDisplayPanel.Visibility = Visibility.Visible;
            CommentsDisplayPanel.Visibility = Visibility.Collapsed;
            foreach (var v in videos)
            {
                if (v != null) MainDisplayPanel.Children.Add(CreateMovieThumbnail(v));
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
            // עיצוב כפתור הסרט (כמו שקיים אצלך, אפשר להוסיף פה תמונה בעתיד)
            Button btn = new Button
            {
                Content = v.VideoName,
                Width = 180,
                Height = 250, // גודל אנכי לפוסטר
                Margin = new Thickness(0, 0, 20, 20),
                Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#222")),
                Foreground = System.Windows.Media.Brushes.White,
                Cursor = System.Windows.Input.Cursors.Hand,
                FontWeight = FontWeights.Bold
            };
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
    }
}