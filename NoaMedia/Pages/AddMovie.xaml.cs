using ApiInterface;
using Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks; // נוסף בשביל ה-Task

namespace NoaMedia.Pages
{
    public partial class AddMovie : Page
    {
        InterfaceAPI api = new InterfaceAPI();

        public AddMovie()
        {
            InitializeComponent();
        }

        private async void SaveMovie_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. איסוף נתונים
                string name = MovieNameTextBox.Text;
                string genreName = GenreTextBox.Text;

                if (!int.TryParse(DurationTextBox.Text, out int duration))
                {
                    MessageBox.Show("Please enter a valid number for Duration.");
                    return;
                }

                if (!int.TryParse(AgeRatingTextBox.Text, out int ageValue))
                {
                    MessageBox.Show("Please enter a valid number for Age Rating.");
                    return;
                }

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(genreName))
                {
                    MessageBox.Show("Movie Name and Genre are required.");
                    return;
                }

                // 2. מציאת ה-Genre
                GenreList allGenres = await api.GetAllGenres();
                Genre selectedGenre = allGenres.FirstOrDefault(g => g.GenreDescription.Equals(genreName, StringComparison.OrdinalIgnoreCase));

                if (selectedGenre == null)
                {
                    MessageBox.Show("Genre not found in database.");
                    return;
                }

                // 3. יצירת אובייקט הסרט עם המרה בטוחה ל-Enum
                Video newVideo = new Video();
                newVideo.VideoName = name;
                newVideo.LengthInMinutes = duration;
                newVideo.Genre = selectedGenre;

                // תיקון שגיאת ה-AgeOfVideo: המרה מפורשת לטיפוס ה-Enum
                newVideo.AgeOfVideo = (AgeOfVideos)Enum.ToObject(typeof(AgeOfVideos), ageValue);

                // 4. שליחה לשרת - תיקון שגיאת ה-success
                // אם InsertVideo מחזיר Task ללא ערך, נשתמש בזה ככה:
                await api.InsertVideo(newVideo);

                // אם הגעת לכאן בלי שקפצה שגיאה - סימן שזה הצליח
                MessageBox.Show("Movie saved successfully to the database!");
                this.NavigationService.Navigate(new Home());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }
        private void MovieNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // אפשר להשאיר ריק כרגע או להוסיף לוגיקה
        }
    }
}