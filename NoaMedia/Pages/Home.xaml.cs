using ApiInterface;
using Model; // מוודא שזה ה-Namespace של ה-Model המקורי שבו נמצא Video
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NoaMedia.Pages
{
    public partial class Home : Page
    {
        // שימוש בשם המחלקה המדויק שלך
        InterfaceAPI api = new InterfaceAPI();

        public Home()
        {
            InitializeComponent();
            LoadAllGenres();
        }

        private async void LoadAllGenres()
        {
            try
            {
                // הרצה של כל 13 הז'אנרים במקביל כדי שהדף ייטען מהר
                // המספרים (1-13) חייבים להתאים ל-ID של הז'אנרים במסד הנתונים שלך
                await Task.WhenAll(
                    FillGenre(ActionMoviesItems, 1),
                    FillGenre(RomanceMoviesItems, 2),
                    FillGenre(HorrorMoviesItems, 3),
                    FillGenre(ComedyMoviesItems, 4),
                    FillGenre(DramaMoviesItems, 5),
                    FillGenre(SciFiMoviesItems, 6),
                    FillGenre(FantasyMoviesItems, 7),
                    FillGenre(ThrillerMoviesItems, 8),
                    FillGenre(AnimationMoviesItems, 9),
                    FillGenre(DocumentaryMoviesItems, 10),
                    FillGenre(CrimeMoviesItems, 11),
                    FillGenre(MysteryMoviesItems, 12),
                    FillGenre(AdventureMoviesItems, 13)
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"General Error: {ex.Message}");
            }
        }

        private async Task FillGenre(ItemsControl control, int genreId)
        {
            try
            {
                // 1. שימוש בשם הנכון: GetVideosByGenre (וודא שהוספת אותו ל-InterfaceAPI כפי ששלחתי קודם)
                var videos = await api.GetVideosByGenre(genreId);

                if (videos != null)
                {
                    List<VideoDisplay> displayList = new List<VideoDisplay>();

                    foreach (var v in videos)
                    {
                        VideoDisplay vd = new VideoDisplay
                        {
                            Id = v.Id,
                            VideoName = v.VideoName
                        };

                        // 2. תיקון השם: GetVideoPicByte64 (בלי המילה Movie - בדיוק כמו ב-InterfaceAPI שלך)
                        string base64 = await api.GetVideoPicByte64(v.Id);

                        if (!string.IsNullOrEmpty(base64))
                        {
                            byte[] imageBytes = Convert.FromBase64String(base64);
                            vd.MovieImageSource = ByteImageConverter.ByteToImage(imageBytes);
                        }

                        displayList.Add(vd);
                    }
                    control.ItemsSource = displayList;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }
        }

        // אירועי הכפתורים הנוספים
        private void AddMovie_Click(object sender, RoutedEventArgs e)
        {
            // כאן תכניס ניווט לדף הוספת סרט
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            // כאן תכניס ניווט לדף פרופיל
        }
    }
}