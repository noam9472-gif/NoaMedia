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
        InterfaceAPI api = new InterfaceAPI();
        string base64Image = "";
        string picName = "";

        public AddVideoAdmin()
        {
            InitializeComponent();
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                cbGenre.ItemsSource = await api.GetAllGenres();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to API: " + ex.Message);
            }
        }

        private void UploadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select Movie Poster";
            op.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png"; // אפשר לסנן רק קבצי תמונה

            if (op.ShowDialog() == true) // אם המשתמש בחר קובץ
            {
                imgPreview.Source = new BitmapImage(new Uri(op.FileName)); // הצגת התמונה שנבחרה
                txtPlaceholder.Visibility = Visibility.Collapsed; // הסתרת הטקסט המכוון
                lblFileName.Text = Path.GetFileName(op.FileName); // הצגת שם הקובץ

                byte[] imageArray = File.ReadAllBytes(op.FileName); // קריאת התמונה למערך בייטים
                base64Image = Convert.ToBase64String(imageArray); // המרת התמונה למחרוזת Base64
                picName = op.FileName; // שמירת שם הקובץ
            }
        }

        private async void SaveVideo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // בדיקה אם כל השדות מלאים, כולל כתובת הווידאו
                if (string.IsNullOrEmpty(txtVideoName.Text) || cbGenre.SelectedItem == null || string.IsNullOrEmpty(txtVideoAddress.Text))
                {
                    MessageBox.Show("Please fill all fields, including the video address.");
                    return;
                }
                // יצירת אובייקט וידאו חדש עם כל המידע הדרוש
                Video videoToInsert = new Video
                {
                    VideoName = txtVideoName.Text,
                    Genre = cbGenre.SelectedItem as Genre,
                    LengthInMinutes = int.TryParse(txtLength.Text, out int len) ? len : 0,
                    VideoDescription = txtDescription.Text,
                    VideoPic = picName, //base64Image,
                    VideoUploadedDate = DateTime.Now,
                    VideoAddress = txtVideoAddress.Text,



                    // יצירת אובייקט משתמש מלא כדי למנוע דחייה מהשרת
                    WhoUploadedTheVideo = new User
                    {
                        Id = 5,
                        UserName = "AdminUser",
                        Name = "Admin",
                        Pass = "1234",
                        Mail = "admin@noamedia.com",
                        DateOfBirth = DateTime.Now,
                        IsAdmin = true,
                        IsPremium = true
                    }
                }; 

                int success = await api.InsertVideo(videoToInsert);

                if (success == 1)
                {
                    MessageBox.Show("Movie published successfully!", "Success");
                    this.NavigationService.GoBack();
                }
                else
                {
                    MessageBox.Show("The server received the request but failed to save. Check if the database is locked or if the Image string is too long.", "Server Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }
        // פונקציה לאימות שהקלט הוא מספר בלבד
        private void NumberValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }
    }
}