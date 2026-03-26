using ApiInterface;
using Model;
using NoaMedia;
using System;
using System.Collections.Generic;
using System.IO;
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
            }

            this.Loaded += (s, e) => LoadContent();
        }

        private void CheckUserPermissions()
        {
            var myApp = Application.Current as App;
            if (myApp?.LoggedInUser != null)
            {
                // הצגת שם המשתמש
                UserNameText.Text = myApp.LoggedInUser.UserName;

                // בדיקה אם המשתמש הוא מנהל (IsAdmin)
                if (myApp.LoggedInUser.IsAdmin)
                {
                    BackToMenuButton.Visibility = Visibility.Visible;
                    AddMovieButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void UpgradeButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PremiumSalesPage()); // מעבר לעמוד רכישת פרימיום
        }

        private async void LoadContent()
        {
            try
            {
                await FillGenreData();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"General Error: {ex.Message}");
            }
        }

        private async Task FillGenreData()
        {
            if (MainGenresContainer == null) return;

            MainGenresContainer.Children.Clear();

            var genres = await api.GetAllGenres();
            var allVideosRaw = await api.GetAllVideos();
            List<Video> allVideos = (allVideosRaw as IEnumerable<Video>)?.ToList() ?? new List<Video>();

            if (genres == null) return;

            foreach (var g in genres)
            {
                if (g.GenreDescription == "Premium Only" && !_isPremium) continue;

                var genreSection = CreateGenreSection(g.GenreDescription);
                var moviesContainer = new WrapPanel { Orientation = Orientation.Horizontal };

                // סינון סרטים לפי ז'אנר
                var genreVideos = allVideos.Where(v => v != null && v.Genre?.Id == g.Id).ToList();
                foreach (var v in genreVideos)
                {
                    var videoUI = await CreateVideoItemUI(v);
                    moviesContainer.Children.Add(videoUI);
                }

                genreSection.Children.Add(moviesContainer);
                MainGenresContainer.Children.Add(genreSection);
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

        private async Task<FrameworkElement> CreateVideoItemUI(Video v)
        {
            // הקונטיינר הראשי של הסרט - רוחב קבוע לפוסטר
            var container = new StackPanel
            {
                Margin = new Thickness(0, 0, 20, 30),
                Width = 180
            };

            // יצירת המסגרת של התמונה (Portrait - 180x270)
            var border = new Border
            {
                Width = 180,
                Height = 270,
                CornerRadius = new CornerRadius(10),
                ClipToBounds = true,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222")),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 15,
                    Opacity = 0.3,
                    ShadowDepth = 2
                }
            };

            var img = new Image { Stretch = Stretch.UniformToFill };

            // תיקון השגיאה: הגדרת BitmapScalingMode בצורה נכונה כמאפיין מצורף
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);

            // טעינת התמונה
            string base64 = v.VideoPic;
            if (string.IsNullOrEmpty(base64) || base64.StartsWith("File"))
            {
                base64 = await api.GetVideoPicByte64(v.Id);
            }

            if (!string.IsNullOrEmpty(base64))
            {
                img.Source = Base64ToImage(base64);
            }

            border.Child = img;

            // יצירת כפתור שקוף שעוטף את כל הפוסטר
            var overlayButton = new Button
            {
                Content = border,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0),
                Cursor = Cursors.Hand,
                Tag = v
            };
            overlayButton.Click += WatchMovie_Click;

            // כותרת הסרט מתחת לפוסטר
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

            return container;
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

        private void WatchMovie_Click(object sender, RoutedEventArgs e)
        {
            // שליפת המידע מה-Tag של הכפתור (שהוא ה-overlayButton)
            if (sender is Button btn && btn.Tag is Video selectedVideo)
            {
                this.NavigationService.Navigate(new MovieDetails(selectedVideo));
            }
            else
            {
                MessageBox.Show("Error: Movie data is missing.");
            }
        }

        private void AddMovie_Click(object sender, RoutedEventArgs e)
        {
            if (_isPremium)
            {
                this.NavigationService.Navigate(new AddMovie());
            }
            else
            {
                MessageBox.Show("אפשרות זו שמורה למשתמשי פרימיום בלבד.");
            }
        }

        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new TransitionOptionForManager(true));
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            string status = _isPremium ? "Premium" : "User";
            this.NavigationService.Navigate(new ProfilePage(status));
        }
    }
}