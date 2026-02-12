using ApiInterface;
using Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NoaMedia.Pages
{
    public partial class Home : Page
    {
        InterfaceAPI api = new InterfaceAPI();

        public Home()
        {
            InitializeComponent();
            LoadAllGenres2();
        }

        private async void LoadAllGenres2()
        {
            await FillGenre2();
        }

        private async Task FillGenre2()
        {
            try
            {
                // ניקוי המכולה הראשית לפני טעינה מחדש
                MainGenresContainer.Children.Clear();

                GenreList gList = await api.GetAllGenres();
                VideoList allVideos = (VideoList)await api.GetAllVideos();

                foreach (Genre g in gList)
                {
                    // פאנל ז'אנרים אנכי
                    StackPanel genreSection = new StackPanel { Margin = new Thickness(0, 0, 0, 30) };

                    // כותרת הז'אנר
                    TextBlock tbHeader = new TextBlock
                    {
                        Text = g.GenreDescription,
                        Foreground = Brushes.White,
                        FontSize = 22,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 20, 0, 10)
                    };
                    genreSection.Children.Add(tbHeader);

                    // פאנל לסרטים אופקי
                    WrapPanel moviesContainer = new WrapPanel { Orientation = Orientation.Horizontal };

                    var videosInThisGenre = allVideos.FindAll(x => x.Genre.Id == g.Id);

                    foreach (var v in videosInThisGenre)
                    {
                        // יצירת סרט יחיד
                        StackPanel videoItem = new StackPanel { Margin = new Thickness(0, 0, 15, 20) };

                        // עיצוב התמונה
                        Border imgBorder = new Border
                        {
                            Width = 220,
                            Height = 125,
                            CornerRadius = new CornerRadius(8),
                            ClipToBounds = true,
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222"))
                        };

                        Image img = new Image { Stretch = Stretch.UniformToFill };
                        string base64 = await api.GetVideoPicByte64(v.Id);
                        if (!string.IsNullOrEmpty(base64))
                        {
                            img.Source = ByteImageConverter.ByteToImage(Convert.FromBase64String(base64));
                        }
                        imgBorder.Child = img;

                        // כפתור צפייה 
                        Button btnWatch = new Button
                        {
                            Content = "Watch Now",
                            Height = 35,
                            Margin = new Thickness(0, 8, 0, 0),
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E50914")),
                            Foreground = Brushes.White,
                            FontWeight = FontWeights.Bold,
                            BorderThickness = new Thickness(0),
                            Cursor = Cursors.Hand,

                            
                            Tag = v // שומרים את אובייקט הסרט בתוך הכפתור
                        };

                       
                        btnWatch.Click += Movie_Click;

                        videoItem.Children.Add(imgBorder);
                        videoItem.Children.Add(btnWatch);

                        moviesContainer.Children.Add(videoItem);
                    }

                    genreSection.Children.Add(moviesContainer);

               
                    MainGenresContainer.Children.Add(genreSection);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }
        }

        // פונקציית הניווט החדשה לסרט ספציפי
        private void Movie_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Video selectedVideo = (Video)btn.Tag; // שליפת הסרט מה-Tag

            if (selectedVideo != null)
            {
                // ניווט לעמוד ה-MovieDetails ושליחת הסרט ב-Constructor
                this.NavigationService.Navigate(new MovieDetails(selectedVideo));
            }
        }

        private void AddMovie_Click(object sender, RoutedEventArgs e)
        {
            AddMovie addMoviePage = new AddMovie();
            if (this.NavigationService != null)
            {
                this.NavigationService.Navigate(addMoviePage);
            }
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            string currentName = UserNameText.Text;
            this.NavigationService.Navigate(new ProfilePage(currentName));
        }
    }
}