using ApiInterface;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace NoaMedia.Pages
{
    public partial class Home : Page
    {
        private readonly InterfaceAPI api = new InterfaceAPI();
        private bool _isPremium;
        private List<Video> _allVideos = new List<Video>(); // רשימת כל הסרטים לחיפוש מהיר

        public Home(bool isPremium)
        {
            InitializeComponent();
            CheckUserPermissions();
            this._isPremium = isPremium;

            if (AddMovieButton != null && UpgradeButton != null)
            {
                if (_isPremium)
                {
                    AddMovieButton.Visibility = Visibility.Visible;
                    UpgradeButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    AddMovieButton.Visibility = Visibility.Collapsed;
                    UpgradeButton.Visibility = Visibility.Visible;
                }
            } this.Loaded += (s, e) => LoadContent(); }
        private void CheckUserPermissions()
        {
            var myApp = Application.Current as App;
            if (myApp?.LoggedInUser != null)
            {
                UserNameText.Text = myApp.LoggedInUser.UserName;
                if (myApp.LoggedInUser.IsAdmin)
                {
                    BackToMenuButton.Visibility = Visibility.Visible;
                    AddMovieButton.Visibility = Visibility.Visible;   }   }  }
        private async void LoadContent()
        {
            try
            {
                // טעינת כל הסרטים פעם אחת למשתנה גלובלי
                var allVideosRaw = await api.GetAllVideos();
                _allVideos = (allVideosRaw as IEnumerable<Video>)?.ToList() ?? new List<Video>();

                await DisplayMovies(_allVideos);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"General Error: {ex.Message}");   } }
               private async Task DisplayMovies(List<Video> moviesToDisplay)  // פונקציה שמציגה את הסרטים לפי רשימה 
        {
            if (MainGenresContainer == null) return;
            MainGenresContainer.Children.Clear();

            var genres = await api.GetAllGenres();
            if (genres == null) return;

            foreach (var g in genres)
            {
                if (g.GenreDescription == "Premium Only" && !_isPremium) continue;

                // סינון הסרטים ששייכים לז'אנר הזה מתוך הרשימה שקיבלנו
                var genreVideos = moviesToDisplay.Where(v => v != null && v.Genre?.Id == g.Id).ToList();

                if (genreVideos.Any()) // מציגים ז'אנר רק אם יש בו סרטים
                {
                    var genreSection = CreateGenreSection(g.GenreDescription);
                    var moviesContainer = new WrapPanel { Orientation = Orientation.Horizontal };

                    foreach (var v in genreVideos)
                    {
                        var videoUI = await CreateVideoItemUI(v);
                        moviesContainer.Children.Add(videoUI);
                    }

                    genreSection.Children.Add(moviesContainer);
                    MainGenresContainer.Children.Add(genreSection);   }  }  }
        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) // אירוע המתרחש בכל שינוי בטקסט של החיפוש
        {
            string searchText = SearchTextBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                await DisplayMovies(_allVideos); // אם החיפוש ריק, הצג הכל
            }
            else
            {
                // סינון סרטים לפי שם
                var filteredMovies = _allVideos.Where(v => v.VideoName.ToLower().Contains(searchText)).ToList();
                await DisplayMovies(filteredMovies); } }
        private StackPanel CreateGenreSection(string title)
        {
            var section = new StackPanel { Margin = new Thickness(0, 0, 0, 30) };
            section.Children.Add(new TextBlock
            {
                Text = title,
                Foreground = Brushes.White,
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 20, 0, 10)
            });  return section;
        } 
        private async Task<FrameworkElement> CreateVideoItemUI(Video v)
        {
            var container = new StackPanel { Margin = new Thickness(0, 0, 20, 30), Width = 180 };
            var border = new Border
            {
                Width = 180,
                Height = 270,
                CornerRadius = new CornerRadius(10),
                ClipToBounds = true,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222"))
            };
            var img = new Image { Stretch = Stretch.UniformToFill };
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);
            string base64 = v.VideoPic;
            if (string.IsNullOrEmpty(base64) || base64.StartsWith("File"))
            {
                base64 = await api.GetVideoPicByte64(v.Id); }
            if (!string.IsNullOrEmpty(base64)) img.Source = Base64ToImage(base64);
            border.Child = img;
            var overlayButton = new Button
            {
                Content = border,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                Tag = v
            };
            overlayButton.Click += WatchMovie_Click;
            var titleText = new TextBlock
            {
                Text = v.VideoName,
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(2, 10, 0, 0),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            container.Children.Add(overlayButton);
            container.Children.Add(titleText);
            return container; }
        public BitmapImage Base64ToImage(string base64String)
        {
            try
            {
                if (string.IsNullOrEmpty(base64String)) return null;

                // הסרת תחיליות נפוצות אם קיימות
                if (base64String.Contains(","))
                {
                    base64String = base64String.Split(',')[1];
                }
                byte[] imageBytes = Convert.FromBase64String(base64String);
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(imageBytes))
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = ms;
                    image.CacheOption = BitmapCacheOption.OnLoad; // חשוב מאוד ב-MemoryStream
                    image.EndInit();
                    image.Freeze(); // מאפשר שימוש ב-UI Thread אחר במידת הצורך
                    return image;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error converting image: {ex.Message}");
                return null;
            }
        }
        private void WatchMovie_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Video selectedVideo)
                this.NavigationService.Navigate(new MovieDetails(selectedVideo)); }
        private void UpgradeButton_Click(object sender, RoutedEventArgs e) => this.NavigationService.Navigate(new PremiumSalesPage());
        private void AddMovie_Click(object sender, RoutedEventArgs e) => this.NavigationService.Navigate(new AddMovie());
        private void BackToMenu_Click(object sender, RoutedEventArgs e) => this.NavigationService.Navigate(new TransitionOptionForManager(true));
        private void Profile_Click(object sender, RoutedEventArgs e) => this.NavigationService.Navigate(new ProfilePage(_isPremium ? "Premium" : "User")); }}