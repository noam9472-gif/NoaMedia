using ApiInterface;
using Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NoaMedia.Pages
{
    public partial class AdminPage : Page
    {
        InterfaceAPI api = new InterfaceAPI();

        public AdminPage()
        {
            InitializeComponent();
            LoadAllData();
        }

        private async void LoadAllData()
        {
            try
            {
                // 1. טעינת משתמשים
                dgAllUsers.ItemsSource = await api.GetAllUsers();

                // 2. טעינת משתמשי פרימיום - מוודא שהפונקציה קיימת ב-api
                dgPremiumUsers.ItemsSource = await api.GetAllUserPremiums();

                // 3. טעינת סרטים
                dgMovies.ItemsSource = await api.GetAllVideos();

                // 4. טעינת ביקורות
                dgComments.ItemsSource = await api.GetAllVideoReviews();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading admin data: " + ex.Message);
            }
        }

        // כפתור חזרה לעמוד האפשרויות
        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            // אנחנו שולחים true כי למנהל מגיעה גישה של פרימיום
            this.NavigationService.Navigate(new TransitionOptionForManager(true));
        }

        // ניווט למשתמש
        private void dgAllUsers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgAllUsers.SelectedItem is User clickedUser)
            {
                this.NavigationService.Navigate(new UserDetailsPage(clickedUser));
            }
        }

        // ניווט לסרט
        private void dgMovies_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgMovies.SelectedItem is Video clickedVideo)
            {
                this.NavigationService.Navigate(new VideoDetailsPage(clickedVideo));
            }
        }

        // --- פעולות מחיקה ---

        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var user = (sender as Button).DataContext as User;
            if (MessageBox.Show($"Are you sure you want to delete {user.UserName}?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await api.DeleteUser(user.Id);
                LoadAllData(); // ריענון הטבלה
            }
        }

        private async void DeleteMovie_Click(object sender, RoutedEventArgs e)
        {
            var video = (sender as Button).DataContext as Video;
            if (MessageBox.Show($"Delete {video.VideoName}?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await api.DeleteVideo(video.Id);
                LoadAllData();
            }
        }

        private async void DeleteReview_Click(object sender, RoutedEventArgs e)
        {
            var review = (sender as Button).DataContext as VideoReview;
            if (MessageBox.Show("Delete this review?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await api.DeleteVideoReview(review.Id);
                LoadAllData();
            }
        }

        // --- פעולות הוספה ---

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            // הנחה שיש לך עמוד הרשמה או הוספה
            // this.NavigationService.Navigate(new RegisterPage()); 
        }

        private void AddMovie_Click(object sender, RoutedEventArgs e)
        {
            // כאן תוכל לנווט לעמוד הוספת סרט חדש
            // this.NavigationService.Navigate(new AddVideoPage());
        }
    }
}