using System;
using System.Linq; 
using System.Windows;
using System.Windows.Controls;
using Model;
using ApiInterface;

namespace NoaMedia.Pages
{
    public partial class ProfilePage : Page
    {
        private InterfaceAPI api = new InterfaceAPI();
        private User currentUser;

        public ProfilePage(string currentName)
        {
            InitializeComponent();

            // שליפת המשתמש - וודא שב-App.xaml.cs יש לך את המשתנה LoggedInUser
            currentUser = (Application.Current as App).LoggedInUser;

            if (currentUser != null)
            {
                UserNameHeading.Text = currentUser.UserName;
                LoadMyContent();
            }
        }

        private async void LoadMyContent()
        {
            try
            {
                // במקום פונקציה שלא קיימת, ניקח את כל הסרטונים ונסנן
                var allVideos = (VideoList)await api.GetAllVideos();

                // סינון: רק סרטונים שה-ID של המעלה שלהם שווה ל-ID של המשתמש הנוכחי
                var myVideos = allVideos.Where(v => v.WhoUploadedTheVideo != null &&
                                               v.WhoUploadedTheVideo.Id == currentUser.Id).ToList();

                // הצגה של הסרטונים שלי
                foreach (var v in myVideos)
                {
                    Button btn = new Button
                    {
                        Content = v.VideoName,
                        Margin = new Thickness(5),
                        Padding = new Thickness(10)
                    };
                    btn.Click += (s, e) => this.NavigationService.Navigate(new MovieDetails(v));
                    MyVideosPanel.Children.Add(btn);
                }

                // אם אין לך עדיין לייקים או תגובות ב-DB, פשוט נשאיר אותם ריקים כרגע
            }
            catch (Exception ex)
            {
                MessageBox.Show("טעינה נכשלה: " + ex.Message);
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // ניקוי המשתמש המחובר
            if (Application.Current is App myApp)
            {
                myApp.LoggedInUser = null;
            }

            this.NavigationService.Navigate(new NoaMedia.Pages.LogIn());
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }
    }
}