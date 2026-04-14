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
        // רשימה שתשמור את ההיסטוריה המלאה כדי שלא נצטרך לקרוא ל-API שוב בלחיצה על "הצג הכל"
        private List<Video> fullHistoryList = new List<Video>();

        public ProfilePage(string currentName)
        {
            InitializeComponent();
            currentUser = (Application.Current as App).LoggedInUser;

            if (currentUser != null)
            {
                UserNameHeading.Text = currentUser.UserName;
                ProfileInitialText.Text = currentUser.UserName.Substring(0, 1).ToUpper();

                if (currentUser.IsAdmin)
                    PremiumBadge.Visibility = Visibility.Visible;
            }
        }

        private async void LoadContent(string category)
        {
            MainDisplayPanel.Children.Clear();
            CommentsDisplayPanel.Children.Clear();
            PremiumLockPanel.Visibility = Visibility.Collapsed;
            ContentScrollViewer.Visibility = Visibility.Visible;
            ShowAllHistoryButton.Visibility = Visibility.Collapsed; // הסתרה כברירת מחדל

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
                        var myLikedVideos = allLikes
                            .Where(l => l.UserId?.Id == currentUser.Id)
                            .Select(l => l.VideoId)
                            .ToList();
                        FillVideoPanel(myLikedVideos);
                        break;

                    case "Watched":
                        SectionTitle.Text = "Watched History";

                        var historyResponse = await api.GetAllMyHistory();
                        if (historyResponse == null) break;

                        List<MyHistory> allHistoryRecords = historyResponse.ToList();

                        // יצירת הרשימה המלאה (בלי ה-Take 5)
                        fullHistoryList = allHistoryRecords
                            .Where(h => h.UserId?.Id == currentUser.Id)
                            .OrderByDescending(h => h.Id)
                            .Select(h => h.VideoId)
                            .Where(v => v != null)
                            .GroupBy(v => v.Id)
                            .Select(g => g.First())
                            .ToList();

                        // הצגת רק 5 ראשונים בתחילה
                        var partialHistory = fullHistoryList.Take(5).ToList();
                        FillVideoPanel(partialHistory);

                        // אם יש יותר מ-5 סרטים, נציג את הכפתור
                        if (fullHistoryList.Count > 5)
                        {
                            ShowAllHistoryButton.Visibility = Visibility.Visible;
                        }
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
                            var allWatchList = (MyWatchListList)await api.GetAllMyWatchList();
                            var myPersonalList = allWatchList
                                .Where(w => w.UserId?.Id == currentUser.Id)
                                .Select(w => w.VideoId)
                                .Where(v => v != null)
                                .ToList();
                            FillVideoPanel(myPersonalList);
                        }
                        break;

                    case "Comments":
                        SectionTitle.Text = "My Comments";
                        MainDisplayPanel.Visibility = Visibility.Collapsed;
                        CommentsDisplayPanel.Visibility = Visibility.Visible;
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Loading failed: " + ex.Message);
            }
        }

        // פונקציית הלחיצה על הכפתור החדש
        private void ShowAllHistory_Click(object sender, RoutedEventArgs e)
        {
            // ניקוי הפאנל וטעינת כל הרשימה ששמרנו מראש
            MainDisplayPanel.Children.Clear();
            FillVideoPanel(fullHistoryList);

            // הסתרת הכפתור לאחר הלחיצה כי כבר רואים הכל
            ShowAllHistoryButton.Visibility = Visibility.Collapsed;
        }

        private async void FillVideoPanel(IEnumerable<Video> videos)
        {
            MainDisplayPanel.Visibility = Visibility.Visible;
            CommentsDisplayPanel.Visibility = Visibility.Collapsed;

            foreach (var v in videos)
            {
                if (v == null) continue;

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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (MenuListBox.SelectedItem is TextBlock selectedItem)
            {
                LoadContent(selectedItem.Tag.ToString());
            }
            else
            {
                MenuListBox.SelectedIndex = 0;
            }
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