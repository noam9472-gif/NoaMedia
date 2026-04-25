using ApiInterface;
using Model;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace NoaMedia.Pages
{
    public partial class AddVideoAdmin : Page
    {
        // 1. הגדרת הממשק ל-API
        InterfaceAPI api = new InterfaceAPI();
        string base64Image = ""; // כאן יישמר הטקסט של התמונה

        public AddVideoAdmin()
        {
            InitializeComponent();
            LoadData(); // טעינת הז'אנרים והגילאים מיד עם פתיחת העמוד
        }

        // 2. טעינת הנתונים מה-API לתוך ה-ComboBoxes
        private async void LoadData()
        {
            try
            {
                // מושכים את הרשימות מהשרת ומציבים אותן בתוך התיבות ב-XAML
                cbGenre.ItemsSource = await api.GetAllGenres();
                cbAge.ItemsSource = await api.GetAllAgeOfVideos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to API: " + ex.Message);
            }
        }

        // 3. לוגיקת בחירת התמונה (מה ששאלת עליו)
        private void UploadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select Movie Poster";
            op.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";

            if (op.ShowDialog() == true)
            {
                // הצגה ויזואלית למשתמש
                imgPreview.Source = new BitmapImage(new Uri(op.FileName));
                txtPlaceholder.Visibility = Visibility.Collapsed;
                lblFileName.Text = Path.GetFileName(op.FileName);

                // המרה לפורמט טקסט (Base64) כדי לשלוח ל-Database
                byte[] imageArray = File.ReadAllBytes(op.FileName);
                base64Image = Convert.ToBase64String(imageArray);
            }
        }

        // 4. שמירת הסרט - הקריאה ל-API
        private async void SaveVideo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // בדיקות תקינות (Validation)
                if (string.IsNullOrEmpty(txtVideoName.Text) || cbGenre.SelectedItem == null)
                {
                    MessageBox.Show("Please enter a name and select a genre.", "Missing Info");
                    return;
                }

                // יצירת אובייקט סרט חדש מהנתונים שהוזנו בטופס
                // בתוך AddVideoAdmin.xaml.cs
                Video newVideo = new Video
                {
                    VideoName = txtVideoName.Text,
                    Genre = cbGenre.SelectedItem as Genre,
                    AgeOfVideo = cbAge.SelectedItem as AgeOfVideos,
                    LengthInMinutes = int.TryParse(txtLength.Text, out int len) ? len : 0,
                    VideoDescription = txtDescription.Text,
                    VideoPic = base64Image,
                    VideoUploadedDate = DateTime.Now,
                    VideoAddress = "local_storage",

                    // קיבוע המנהל - פותר את הבעיה בלי להוסיף כפתורים
                    WhoUploadedTheVideo = new User { Id = 5 }
                };
                // שליחה לשרת
                int success = await api.InsertVideo(newVideo);

                if (success == 1)
                {
                    MessageBox.Show("Movie published successfully!", "Success");
                    this.NavigationService.GoBack(); // חזרה אוטומטית לעמוד הניהול
                }
                else
                {
                    MessageBox.Show("Failed to save the movie. Please check the server.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // כפתור ביטול
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }

        // מוודא שבשדה האורך יכתבו רק מספרים
        private void NumberValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }
    }
}