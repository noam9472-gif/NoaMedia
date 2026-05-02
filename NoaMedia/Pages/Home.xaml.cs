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
        private List<Video> _allVideos = new List<Video>();
        private User _currentUser;

        public Home(User user)
        {
            InitializeComponent();
            this._currentUser = user;
            this._isPremium = user.IsPremium;

            CheckUserPermissions();

            // שימוש ב-Loaded כדי להבטיח שהאלמנטים הגרפיים קיימים
            this.Loaded += Home_Loaded;
        }

        private void Home_Loaded(object sender, RoutedEventArgs e)
        {
            LoadContent();
        }

        private void CheckUserPermissions()
        {
            if (_currentUser != null)
            {
                if (UserNameText != null) UserNameText.Text = _currentUser.Name;
                if (AddMovieButton != null)
                    AddMovieButton.Visibility = (_currentUser.IsAdmin || _isPremium) ? Visibility.Visible : Visibility.Collapsed;
                if (UpgradeButton != null)
                    UpgradeButton.Visibility = (_isPremium || _currentUser.IsAdmin) ? Visibility.Collapsed : Visibility.Visible;
                if (BackToMenuButton != null)
                    BackToMenuButton.Visibility = _currentUser.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private async void LoadContent()
        {
            try
            {
                // קבלת כל הסרטים מה-API
                var allVideosRaw = await api.GetAllVideos();
                _allVideos = (allVideosRaw as IEnumerable<Video>)?.ToList() ?? new List<Video>();

                await DisplayMovies(_allVideos);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading movies: {ex.Message}");
            }
        }

        private async Task DisplayMovies(List<Video> moviesToDisplay)
        {
            if (MainGenresContainer == null) return;
            MainGenresContainer.Children.Clear();

            var genres = await api.GetAllGenres();
            if (genres == null) return;

            foreach (var g in genres)
            {
                // סינון ז'אנרים למנויי פרימיום/מנהלים בלבד
                if (g.GenreDescription == "Premium Only" && !_isPremium && !_currentUser.IsAdmin) continue;

                var genreVideos = moviesToDisplay.Where(v => v != null && v.Genre?.Id == g.Id).ToList();

                if (genreVideos.Any())
                {
                    var genreSection = CreateGenreSection(g.GenreDescription);
                    var moviesContainer = new WrapPanel { Orientation = Orientation.Horizontal };

                    foreach (var v in genreVideos)
                    {
                        var videoUI = await CreateVideoItemUI(v);
                        moviesContainer.Children.Add(videoUI);
                    }

                    genreSection.Children.Add(moviesContainer);
                    MainGenresContainer.Children.Add(genreSection);
                }
            }
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

            // לוגיקת טעינת תמונה
            string base64 = v.VideoPic;

            // אם ה-Base64 ריק באובייקט הסרט, ננסה למשוך אותו ספציפית לפי ID
            if (string.IsNullOrEmpty(base64))
            {
                base64 = await api.GetVideoPicByte64(v.Id);
            }

            if (!string.IsNullOrEmpty(base64))
            {
                var bitmap = Base64ToImage(base64);
                if (bitmap != null)
                {
                    img.Source = bitmap;
                }
            }

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
                Margin = new Thickness(2, 10, 0, 0),
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            container.Children.Add(overlayButton);
            container.Children.Add(titleText);
            return container;
        }

        public BitmapImage Base64ToImage(string base64String)
        {
            if (string.IsNullOrWhiteSpace(base64String)) return null;

            try
            {
                // 1. ניקוי רווחים ותווים לא חוקיים שיכולים להגיע מה-DB
                base64String = base64String.Trim();

                // 2. טיפול בתחיליות (Data URI) אם קיימות
                if (base64String.Contains(","))
                {
                    base64String = base64String.Split(',')[1];
                }

                // 3. בדיקה אם אורך המחרוזת חוקי ל-Base64 (חייב להיות כפולה של 4)
                // אם חסר Padding (=), זה יכול להפיל את ההמרה
                base64String = base64String.Replace("\r", "").Replace("\n", "");
                if (base64String.Length % 4 != 0)
                {
                    base64String = base64String.PadRight(base64String.Length + (4 - base64String.Length % 4) % 4, '=');
                }

                byte[] imageBytes = Convert.FromBase64String(base64String);

                using (var ms = new System.IO.MemoryStream(imageBytes))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    // הגדרה קריטית כדי שהתמונה תישמר בזיכרון גם אחרי שה-Stream נסגר
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    image.Freeze(); // הופך את האובייקט ל-Thread-safe (חשוב מאוד ב-WPF)
                    return image;
                }
            }
            catch (Exception ex)
            {
                // הדפסה לחלון ה-Output ב-Visual Studio כדי שתראה מה השגיאה
                System.Diagnostics.Debug.WriteLine($"CRITICAL IMAGE ERROR: {ex.Message}");
                return null;
            }
        }

        private void WatchMovie_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Video selectedVideo)
                this.NavigationService.Navigate(new MovieDetails(selectedVideo));
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text?.ToLower() ?? "";
            if (string.IsNullOrWhiteSpace(searchText))
                await DisplayMovies(_allVideos);
            else
            {
                var filtered = _allVideos.Where(v => v.VideoName.ToLower().Contains(searchText)).ToList();
                await DisplayMovies(filtered);
            }
        }

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
            });
            return section;
        }

        private void UpgradeButton_Click(object sender, RoutedEventArgs e) => this.NavigationService.Navigate(new PremiumSalesPage());
        private void AddMovie_Click(object sender, RoutedEventArgs e) => this.NavigationService.Navigate(new AddMovie(this._currentUser));
        private void BackToMenu_Click(object sender, RoutedEventArgs e) => this.NavigationService.Navigate(new TransitionOptionForManager(true));
        private void Profile_Click(object sender, RoutedEventArgs e) => this.NavigationService.Navigate(new ProfilePage(_isPremium ? "Premium" : "User"));
    }
}