using ApiInterface;
using Model;
using NoaMedia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
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
            this._isPremium = isPremium;

            // 1. עדכון שם המשתמש (מה שדיברנו עכשיו)
            var currentUser = (Application.Current as App).LoggedInUser;
            if (currentUser != null && !string.IsNullOrEmpty(currentUser.UserName))
            {
                UserNameText.Text = currentUser.UserName;
            }

            // 2. הבדיקה שלך בין פרימיום לרגיל (החזרתי אותה למקומה)
            if (_isPremium)
            {
                // פרימיום: רואה הוספת סרט, לא רואה כפתור שדרוג
                AddMovieButton.Visibility = Visibility.Visible;
                UpgradeButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                // רגיל: לא רואה הוספת סרט, כן רואה כפתור שדרוג
                AddMovieButton.Visibility = Visibility.Collapsed;
                UpgradeButton.Visibility = Visibility.Visible;
            }

            this.Loaded += (s, e) => LoadContent();
        }

        // אל תשכח להוסיף את הפונקציה למעבר לעמוד השדרוג
        private void UpgradeButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PremiumSalesPage());
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

        private async Task<StackPanel> CreateVideoItemUI(Video v)
        {
            var container = new StackPanel { Margin = new Thickness(0, 0, 15, 20) };

            var border = new Border
            {
                Width = 220,
                Height = 125,
                CornerRadius = new CornerRadius(8),
                ClipToBounds = true,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222"))
            };

            var img = new Image { Stretch = Stretch.UniformToFill };

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

            var btn = new Button
            {
                Content = "Watch Now",
                Height = 35,
                Margin = new Thickness(0, 8, 0, 0),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E50914")),
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                BorderThickness = new Thickness(0),
                Tag = v 
            };
            btn.Click += WatchMovie_Click;

            container.Children.Add(border);
            container.Children.Add(new TextBlock { Text = v.VideoName, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 5, 0, 0) });
            container.Children.Add(btn);
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
            Button btn = sender as Button;
            if (btn != null)
            {
                Video selectedVideo = btn.Tag as Video;

                if (selectedVideo != null)
                {
                    this.NavigationService.Navigate(new MovieDetails(selectedVideo));
                }
                else
                {
                    MessageBox.Show("Error: Movie data is missing.");
                }
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

        // הכפתור ששאלת עליו - הוא נשאר כאן!
        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            // אנחנו שולחים לדף הפרופיל את הסטטוס כדי שגם שם הוא ידע אם להציג "Premium"
            string status = _isPremium ? "Premium" : "User";
            this.NavigationService.Navigate(new ProfilePage(status));
        }
    }
}

