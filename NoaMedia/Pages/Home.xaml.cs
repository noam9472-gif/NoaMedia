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

        public Home() { InitializeComponent(); LoadAllGenres2();}

       
        private async void LoadAllGenres2() { FillGenre2(); }



        //private async Task FillGenre2()
        //{
        //    try
        //    {
        //        // ניקוי המכולה לפני הטעינה (כדי שלא יכפיל ז'אנרים ברילווד)
        //        MainGenresContainer.Children.Clear();

        //        GenreList gList = await api.GetAllGenres();
        //        VideoList allVideos = (VideoList)await api.GetAllVideos();

        //        foreach (Genre g in gList)
        //        {
        //            StackPanel genreSection = new StackPanel { Margin = new Thickness(0, 0, 0, 30) };

        //            TextBlock tbHeader = new TextBlock
        //            {
        //                Text = g.GenreDescription,
        //                Foreground = Brushes.Red,
        //                FontSize = 24,
        //                FontWeight = FontWeights.Bold,
        //                Margin = new Thickness(10, 0, 0, 10)
        //            };
        //            genreSection.Children.Add(tbHeader);

        //            // 3. יצירת פאנל אופקי לסרטים (כדי שיעמדו בשורה)
        //            StackPanel moviesRow = new StackPanel { Orientation = Orientation.Horizontal };

        //            // אפשר לעטוף את השורה ב-ScrollViewer אופקי אם יש הרבה סרטים
        //            ScrollViewer horizontalScroll = new ScrollViewer
        //            {
        //                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
        //                Content = moviesRow
        //            };

        //            var videosInThisGenre = allVideos.FindAll(x => x.Genre.Id == g.Id);

        //            foreach (var v in videosInThisGenre)
        //            {
        //                StackPanel videoItem = new StackPanel { Margin = new Thickness(10, 0, 10, 0), Width = 120 };

        //                Image img = new Image { Width = 120, Height = 180, Stretch = Stretch.UniformToFill };
        //                string base64 = await api.GetVideoPicByte64(v.Id);

        //                if (!string.IsNullOrEmpty(base64))
        //                {
        //                    img.Source = ByteImageConverter.ByteToImage(Convert.FromBase64String(base64));
        //                }

        //                TextBlock tbTitle = new TextBlock
        //                {
        //                    Text = v.VideoName,
        //                    Foreground = Brushes.White,
        //                    HorizontalAlignment = HorizontalAlignment.Center,
        //                    TextWrapping = TextWrapping.Wrap,
        //                    TextAlignment = TextAlignment.Center
        //                };

        //                videoItem.Children.Add(img);
        //                videoItem.Children.Add(tbTitle);
        //                moviesRow.Children.Add(videoItem);
        //            }

        //            // 4. הוספת שורת הסרטים לפלח הז'אנר
        //            genreSection.Children.Add(horizontalScroll);

        //            // 5. הוספת כל הז'אנר למכולה הראשית של הדף
        //            MainGenresContainer.Children.Add(genreSection);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        //    }
        //}

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
                    // 1. יצירת פאנל לכל מקטע של ז'אנר (אנכי)
                    StackPanel genreSection = new StackPanel { Margin = new Thickness(0, 0, 0, 30) };

                    // 2. כותרת הז'אנר
                    TextBlock tbHeader = new TextBlock
                    {
                        Text = g.GenreDescription,
                        Foreground = Brushes.White,
                        FontSize = 22,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 20, 0, 10)
                    };
                    genreSection.Children.Add(tbHeader);

                    // 3. יצירת פאנל לסרטים - הפעם WrapPanel כדי שיגלשו לשורה הבאה במקום להיעלם הצידה
                    // או StackPanel אופקי פשוט אם אתה רוצה שהם פשוט ייחתכו בסוף המסך
                    WrapPanel moviesContainer = new WrapPanel { Orientation = Orientation.Horizontal };

                    var videosInThisGenre = allVideos.FindAll(x => x.Genre.Id == g.Id);

                    foreach (var v in videosInThisGenre)
                    {
                        // יצירת פריט סרט (תמונה + כפתור)
                        // כאן נשתמש ב-Template שלך אם תרצה, או ניצור ידנית כמו שעשינו קודם:
                        StackPanel videoItem = new StackPanel { Margin = new Thickness(0, 0, 15, 20) };

                        // עיצוב התמונה (Border עם פינות מעוגלות כמו ב-XAML שלך)
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
                            Cursor = Cursors.Hand
                        };

                        videoItem.Children.Add(imgBorder);
                        videoItem.Children.Add(btnWatch);

                        moviesContainer.Children.Add(videoItem);
                    }

                    // 4. הוספת מיכל הסרטים למקטע הז'אנר
                    genreSection.Children.Add(moviesContainer);

                    // 5. הוספת כל המקטע למכולה הראשית ב-XAML (MainGenresContainer)
                    MainGenresContainer.Children.Add(genreSection);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }
        }









        //private async Task FillGenre(ItemsControl control, int genreId)
        //{
        //    try
        //    {
        //       // 1.שימוש בשם הנכון: GetVideosByGenre(וודא שהוספת אותו ל - InterfaceAPI כפי ששלחתי קודם)
        //        var videos = await api.GetVideosByGenre(genreId);

        //        if (videos != null)
        //        {
        //            List<VideoDisplay> displayList = new List<VideoDisplay>();

        //            foreach (var v in videos)
        //            {
        //                VideoDisplay vd = new VideoDisplay
        //                {
        //                    Id = v.Id,
        //                    VideoName = v.VideoName
        //                };

        //              //  2.תיקון השם: GetVideoPicByte64(בלי המילה Movie - בדיוק כמו ב - InterfaceAPI שלך)
        //                string base64 = await api.GetVideoPicByte64(v.Id);

        //                if (!string.IsNullOrEmpty(base64))
        //                {
        //                    byte[] imageBytes = Convert.FromBase64String(base64);
        //                    vd.MovieImageSource = ByteImageConverter.ByteToImage(imageBytes);
        //                }

        //                displayList.Add(vd);
        //            }
        //            control.ItemsSource = displayList;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        //    }
        //}

        // אירועי הכפתורים הנוספים
        private void AddMovie_Click(object sender, RoutedEventArgs e)
        {
            // ניווט לעמוד הוספת סרט
            AddMovie addMoviePage = new AddMovie();
            if (this.NavigationService != null)
            {
                this.NavigationService.Navigate(addMoviePage);
            }
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            // כאן תכניס ניווט לדף פרופיל
        }
    }
}