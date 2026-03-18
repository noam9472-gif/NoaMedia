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

        private async void LoadAllData()// פונקציה שמטעינה את כל הטבלאות בדף הניהול
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


       private async void DeleteUser_Click(object sender, RoutedEventArgs e)// פונקציה שמוחקת משתמש
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
                    await api.ForceClearUserEverything(user.Id);// ניקוי מקדים של כל הנתונים הקשורים למשתמש לפני המחיקה הסופית

                    int success = await api.DeleteUser(user.Id);// המחיקה הסופית של המשתמש

                    if (success == 1)
                    {
                        MessageBox.Show("User deleted successfully!");// הודעת הצלחה
                        LoadAllData();
                    }
                    else
                    {
                        MessageBox.Show("The Database still refuses.\n\n" + 
                                "Please open your Access file and check for a table called 'Orders', 'Payments' or 'WatchList'.", 
                                "Critical DB Constraint", MessageBoxButton.OK, MessageBoxImage.Error);// הודעת שגיאה עם הסבר
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error during delete: " + ex.Message);
                }
            }
        }
        private async void DeleteMovie_Click(object sender, RoutedEventArgs e)// פונקציה שמוחקת סרט
        {
            var video = (sender as Button)?.DataContext as Video;// שליפת הסרט מהשורה שנבחרה
            if (video == null) return;

            if (MessageBox.Show($"Delete '{video.VideoName}' and all its reviews?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await api.ForceClearVideo(video.Id); // ניקוי מקדים
                int success = await api.DeleteVideo(video.Id); // מחיקה סופית

                if (success == 1)
                {
                    MessageBox.Show("Video deleted successfully.");// הודעת הצלחה
                    LoadAllData();
                }
                else
                {
                    MessageBox.Show("The Database still refuses to delete the Video.");// הודעת שגיאה עם הסבר
                }
            }
        }
        private async void DeleteReview_Click(object sender, RoutedEventArgs e)// פונקציה שמוחקת ביקורת
        {
            var review = (sender as Button)?.DataContext as VideoReview;// שליפת הביקורת מהשורה שנבחרה
            if (review == null) return;

            // הצגת הודעת אישור
            var result = MessageBox.Show($"Are you sure you want to delete the review by '{review.WhoUpdatedTheReview?.UserName}'?",
                                         "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    int success = await api.DeleteVideoReview(review.Id);

                    if (success > 0)
                    {
                        MessageBox.Show("Review deleted successfully.");
                        LoadAllData(); // ריענון הטבלאות
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete the review. It might have been already deleted.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error connecting to server: " + ex.Message);
                }
            }
        }

        private async void DeleteGenre_Click(object sender, RoutedEventArgs e)// פונקציה שמוחקת ז'אנר
        {
            //  קבלת הז'אנר שנבחר מהרשימה
            Genre genre = (sender as Button)?.DataContext as Genre;
            if (genre == null) return;

            if (genre.Id == 14) // ID=14 זה ז'אנר ברירת מחדל אסור למחוק
            {
                MessageBox.Show("Cannot delete the default 'No Genre' category.");
                return;
            }

            //  אישור מהמשתמש
            var result = MessageBox.Show($"Are you sure you want to delete '{genre.GenreDescription}'? " +
                "All movies in this genre will move to 'No Genre'.", "Confirm Delete", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                int success = await api.DeleteGenre(genre.Id);

                if (success > 0)
                {
                    MessageBox.Show("Genre deleted successfully!");
                    // רענון הרשימה 
                    LoadAllData();
                }
                else
                {
                    MessageBox.Show("Delete failed. Check server logs.");
                }
            }
        }


        private void AddUser_Click(object sender, RoutedEventArgs e)// ניווט לעמוד הוספת משתמש
        {
            this.NavigationService.Navigate(new AddUserAdmin());
        }

        private void AddMovie_Click(object sender, RoutedEventArgs e)// ניווט לעמוד הוספת סרט
        {
            this.NavigationService.Navigate(new AddVideoAdmin());
        }


        // פונקציה להעברה גורפת של סרטים בין קטגוריות
        private async void MoveMovies_Click(object sender, RoutedEventArgs e)
        {
            // קולטים מהמשתמש מאיזה מספר ז'אנר הוא רוצה להעביר
            string fromInput = Microsoft.VisualBasic.Interaction.InputBox("Enter SOURCE Genre ID (e.g. 14):", "Move Movies Bulk Action", "");
            if (string.IsNullOrEmpty(fromInput)) return;

            // קולטים מהמשתמש לאיזה מספר ז'אנר הוא רוצה להעביר
            string toInput = Microsoft.VisualBasic.Interaction.InputBox("Enter TARGET Genre ID:", "Move Movies Bulk Action", "");
            if (string.IsNullOrEmpty(toInput)) return;

            // בדיקת תקינות
            if (int.TryParse(fromInput, out int fromId) && int.TryParse(toInput, out int toId))
            {
                if (fromId == toId)
                {
                    MessageBox.Show("Source and Target cannot be the same!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var confirm = MessageBox.Show($"Are you sure you want to move ALL movies from Genre ID {fromId} to Genre ID {toId}?",
                                              "Confirm Migration", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirm == MessageBoxResult.Yes)
                {
                    try
                    {
                        // קריאה לפונקציה שמבצעת את ההעברה במסד הנתונים(InterfaceAPI.cs)
                        int movedCount = await api.MoveMoviesBetweenGenres(fromId, toId);

                        if (movedCount >= 0)
                        {
                            MessageBox.Show($"Successfully moved {movedCount} movies!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadAllData(); // רענון כל הטבלאות
                        }
                        else
                        {
                            MessageBox.Show("No movies were found in the source genre, or an error occurred.", "No Action Taken", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Communication Error: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please enter valid numeric IDs only.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // פונקציה לשינוי ז'אנר של סרט בודד
        private async void ChangeMovieGenre_Click(object sender, RoutedEventArgs e)
        {
            // שליפת הסרט מהשורה שנלחצה
            var video = (sender as Button)?.DataContext as Video;
            if (video == null) return;

            // מקבלים מהמשתמש את מספר הז'אנר החדש לסרט הזה
            string input = Microsoft.VisualBasic.Interaction.InputBox($"Enter New Genre ID for '{video.VideoName}':", "Change Movie Genre", "");

            if (int.TryParse(input, out int newGenreId))// בדיקת תקינות
            {
                try
                {
                    int result = await api.UpdateSingleMovieGenre(video.Id, newGenreId);// קריאה לפונקציה שמבצעת את העדכון במסד הנתונים(InterfaceAPI.cs)
                    if (result > 0)
                    {
                        MessageBox.Show("Genre updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);//הודעת הצלחה
                        LoadAllData(); // רענון הטבלה
                    }
                    else
                    {
                        MessageBox.Show("Failed to update. Make sure the Genre ID exists in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);// הודעת שגיאה
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating genre: " + ex.Message);
                }
            }
        }

        private async void AddGenre_Click(object sender, RoutedEventArgs e)
        {
            string genreName = Microsoft.VisualBasic.Interaction.InputBox("Enter New Genre Name:", "Add Genre", "");// קבלת שם הז'אנר החדש מהמשתמש

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