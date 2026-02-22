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

namespace NoaMedia.Pages
{
   public partial class Home : Page
    {
        private readonly InterfaceAPI api = new InterfaceAPI();

        public Home()
        {
            InitializeComponent();
            this.Loaded += (s, e) => LoadContent();
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
            //if (MainGenresContainer == null) return;

            MainGenresContainer.Children.Clear();

            var genres = await api.GetAllGenres();
            var allVideosRaw = await api.GetAllVideos();
            List<Video> allVideos = (allVideosRaw as IEnumerable<Video>)?.ToList() ?? new List<Video>();

            if (genres == null) return;

            foreach (var g in genres)
            {
                var genreSection = CreateGenreSection(g.GenreDescription);
                var moviesContainer = new WrapPanel { Orientation = Orientation.Horizontal };
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

            // עיצוב התמונה
            var border = new Border
            {
                Width = 220,
                Height = 125,
                CornerRadius = new CornerRadius(8),
                ClipToBounds = true,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222"))
            };

            var img = new Image { Stretch = Stretch.UniformToFill };
            string base64 = await api.GetVideoPicByte64(v.Id);
            if (!string.IsNullOrEmpty(base64))
            {
                try
                {
                    img.Source = ByteImageConverter.ByteToImage(Convert.FromBase64String(base64));
                }
                catch {  }
            }
            border.Child = img;

            // כפתור צפייה
            var btn = new Button
            {
                Content = "Watch Now",
                Height = 35,
                Margin = new Thickness(0, 8, 0, 0),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E50914")),
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                BorderThickness = new Thickness(0),
                Tag = v // שמירת הסרט בתוך הכפתור
            };
            btn.Click += Movie_Click;

            container.Children.Add(border);
            container.Children.Add(btn);
            return container;
        }


        private void Movie_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Video v)
            {
                MessageBox.Show($"Starting: {v.VideoName}");
            }
        }

        private void AddMovie_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new AddMovie());
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            // העברת המשתמש המחובר לעמוד הפרופיל כדי להציג את פרטיו
            this.NavigationService.Navigate(new ProfilePage(this.Name));
        }
    }
}