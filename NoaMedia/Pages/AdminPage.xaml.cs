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

                // 2. טעינת משתמשי פרימיום
                dgPremiumUsers.ItemsSource = await api.GetAllUserPremiums();

                // 3. טעינת סרטים
                dgMovies.ItemsSource = await api.GetAllVideos();

                // 4. טעינת ביקורות
                dgComments.ItemsSource = await api.GetAllVideoReviews();

                // 5. טעינת ז'אנרים - זה השורה החדשה שלך
                dgGenres.ItemsSource = await api.GetAllGenres();
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
    var user = (sender as Button)?.DataContext as User;
    if (user == null) return;

    // מניעת מחיקת המנהל המחובר
    var myApp = Application.Current as App;
    if (myApp?.LoggedInUser != null && user.Id == myApp.LoggedInUser.Id)
    {
        MessageBox.Show("You cannot delete yourself!", "Stop", MessageBoxButton.OK, MessageBoxImage.Hand);
        return;
    }

    if (MessageBox.Show($"Are you sure you want to delete {user.UserName}?\nThis is a deep delete.", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
    {
        try 
        {
            // קריאה לפונקציית הניקוי המסיבי
            await api.ForceClearUserEverything(user.Id);

            // ניסיון מחיקה סופי
            int success = await api.DeleteUser(user.Id);
            
            if (success == 1)
            {
                MessageBox.Show("User deleted successfully!");
                LoadAllData();
            }
            else
            {
                // אם הגעת לכאן, פתח את ה-Access/SQL שלך ידנית ובדוק איזה טבלאות נוספות יש שם
                MessageBox.Show("The Database still refuses.\n\n" + 
                                "Please open your Access file and check for a table called 'Orders', 'Payments' or 'WatchList'.", 
                                "Critical DB Constraint", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error during delete: " + ex.Message);
        }
    }
}
        private async void DeleteMovie_Click(object sender, RoutedEventArgs e)
        {
            var video = (sender as Button)?.DataContext as Video;
            if (video == null) return;

            if (MessageBox.Show($"Delete '{video.VideoName}' and all its reviews?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await api.ForceClearVideo(video.Id); // ניקוי מקדים
                int success = await api.DeleteVideo(video.Id); // מחיקה סופית

                if (success == 1)
                {
                    MessageBox.Show("Video deleted successfully.");
                    LoadAllData();
                }
                else
                {
                    MessageBox.Show("The Database still refuses to delete the Video.");
                }
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
            this.NavigationService.Navigate(new AddUserAdmin());
        }

        private void AddMovie_Click(object sender, RoutedEventArgs e)
        {
            // ניווט לעמוד הוספת הסרט החדש שיצרת
            this.NavigationService.Navigate(new AddVideoAdmin());
        }

        private async void DeleteGenre_Click(object sender, RoutedEventArgs e)
        {
            var genre = (sender as Button).DataContext as Genre;
            if (genre == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete the genre: {genre.GenreDescription}?\nNote: This might affect movies assigned to this genre.",
                                         "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                int success = await api.DeleteGenre(genre.Id);
                if (success == 1)
                {
                    LoadAllData(); // ריענון
                }
                else
                {
                    MessageBox.Show("Could not delete genre. It might be in use by some movies.");
                }
            }
        }

        private async void AddGenre_Click(object sender, RoutedEventArgs e)
        {
            // נשתמש בתיבת טקסט פשוטה (אפשר גם ליצור חלון קטן, אבל לצורך הלמידה):
            string genreName = Microsoft.VisualBasic.Interaction.InputBox("Enter New Genre Name:", "Add Genre", "");

            if (!string.IsNullOrEmpty(genreName))
            {
                Genre newGenre = new Genre { GenreDescription = genreName };
                int result = await api.InsertGenre(newGenre);

                if (result == 1)
                {
                    MessageBox.Show("Genre added successfully!");
                    LoadAllData(); // ריענון הטבלה
                }
                else
                {
                    MessageBox.Show("Error adding genre.");
                }
            }
        }
    }
}