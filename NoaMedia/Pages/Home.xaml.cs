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

        public Home() { InitializeComponent(); LoadAllGenres2(); }

        private async void LoadAllGenres()
        {
            try
            {
                // הרצה של כל 13 הז'אנרים במקביל כדי שהדף ייטען מהר
                // המספרים (1-13) חייבים להתאים ל-ID של הז'אנרים במסד הנתונים שלך
                await Task.WhenAll(
                    FillGenre2(),
                    FillGenre2(),
                    FillGenre2(),
                    FillGenre2(),
                    FillGenre2(),
                    FillGenre2( ),
                    FillGenre2(),
                    FillGenre2( ),
                    FillGenre2(),
                    FillGenre2(),
                    FillGenre2(),
                    FillGenre2(),
                    FillGenre2()
                );
            }
            catch (Exception ex) {  System.Diagnostics.Debug.WriteLine($"General Error: {ex.Message}");  }
        }
        private async void LoadAllGenres2() { FillGenre2(); }


        private async Task FillGenre2()
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            VideoList displayList;
            try
            {
                GenreList gList=await api.GetAllGenres();
                foreach (Genre g in gList)

                {
                    TextBlock tb = new TextBlock();
                    tb.Text = g.GenreDescription;
                    panel.Children.Add(tb);

                    displayList = ((VideoList)await api.GetAllVideos());
                    var allVideosByGenre = displayList.FindAll(x => x.Genre.Id == g.Id);
                    foreach (var v in allVideosByGenre)
                    {
                        string videoName = v.VideoName;
                        TextBlock tbVideo = new TextBlock();
                        tbVideo.Text = videoName;
                        panel.Children.Add(tbVideo);
                        string base64 = await api.GetVideoPicByte64(v.Id);
                        Image image = new Image();
                        if (!string.IsNullOrEmpty(base64))
                        {
                            byte[] imageBytes = Convert.FromBase64String(base64);
                            var img = ByteImageConverter.ByteToImage(imageBytes);
                            image.Source = img;
                            panel.Children.Add(image);
                        }
                        baseSPhorror.Children.Add(panel);
                    }
                }
            }
            
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}"); }
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
            //AddMovie addMoviePage = new AddMovie();
            //if (this.NavigationService != null)
            //{
            //    this.NavigationService.Navigate(addMoviePage);
            //}
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            // כאן תכניס ניווט לדף פרופיל
        }
    }
}