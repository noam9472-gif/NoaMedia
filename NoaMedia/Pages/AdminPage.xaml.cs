using ApiInterface;
using Model;
using System;
using System.Linq; // הוספנו את זה בשביל הסינון (Where)
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
            this.Loaded += AdminPage_Loaded;
        }

        private void AdminPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAllData();// טעינת כל הנתונים הדרושים לטבלאות השונות בדף הניהול
        }

        private async void LoadAllData() // פונקציה שמטעינה את כל הטבלאות בדף הניהול
        {
            try
            {
                // 1. טעינת כל המשתמשים מה-API פעם אחת
                var allUsers = await api.GetAllUsers();
                dgAllUsers.ItemsSource = allUsers;

                // 2. עדכון משתמשי פרימיום - סינון מתוך הרשימה הכללית לפי השדה IsPremium
                if (allUsers != null)
                {
                    dgPremiumUsers.ItemsSource = allUsers.Where(u => u.IsPremium).ToList();
                }

                // 3. טעינת סרטים
                dgMovies.ItemsSource = await api.GetAllVideos();

                // 4. טעינת ביקורות
                dgComments.ItemsSource = await api.GetAllVideoReviews();

                // 5. טעינת ז'אנרים
                dgGenres.ItemsSource = await api.GetAllGenres();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading admin data: " + ex.Message);
            }
        }


        // פונקציה שמופעלת ברגע שסיימת לשנות תא (למשל הורדת V מפרימיום)
        private async void dgAllUsers_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // אנחנו משתמשים ב-Dispatcher כדי לתת ל-WPF לסיים לעדכן את האובייקט בזיכרון לפני שנשלח אותו
            await Dispatcher.BeginInvoke(new Action(async () =>
            {
                // שליפת המשתמש מהשורה שנערכה
                if (e.Row.Item is User user)
                {
                    try
                    {
                        // 1. שליחת העדכון למסד הנתונים דרך ה-API
                        // הערה: וודא שיש לך פונקציית UpdateUser ב-InterfaceAPI שמקבלת אובייקט User
                        int success = await api.UpdateUser(user);

                        if (success > 0)
                        {
                            // 2. קריאה מחדש לנתונים - זה יגרום ל-dgPremiumUsers להסתנכרן מיד
                            LoadAllData();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error updating user: " + ex.Message);
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
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
            string fromInput = Microsoft.VisualBasic.Interaction.InputBox("Enter SOURCE Genre ID (e.g. 14):", "Move Movies Bulk Action", "");
            if (string.IsNullOrEmpty(fromInput)) return;

            string toInput = Microsoft.VisualBasic.Interaction.InputBox("Enter TARGET Genre ID:", "Move Movies Bulk Action", "");
            if (string.IsNullOrEmpty(toInput)) return;

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
            var video = (sender as Button)?.DataContext as Video;
            if (video == null) return;

            string input = Microsoft.VisualBasic.Interaction.InputBox($"Enter New Genre ID for '{video.VideoName}':", "Change Movie Genre", "");

            if (int.TryParse(input, out int newGenreId))
            {
                try
                {
                    int result = await api.UpdateSingleMovieGenre(video.Id, newGenreId);
                    if (result > 0)
                    {
                        MessageBox.Show("Genre updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadAllData(); // רענון הטבלה
                    }
                    else
                    {
                        MessageBox.Show("Failed to update. Make sure the Genre ID exists in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            // פתיחת תיבת קלט
            string genreName = Microsoft.VisualBasic.Interaction.InputBox("Enter New Genre Name:", "Add Genre", "");

            // בדיקה שהמשתמש לא לחץ Cancel ולא השאיר ריק
            if (string.IsNullOrWhiteSpace(genreName))
            {
                return; // פשוט יוצאים בלי לעשות כלום
            }

            try
            {
                // יצירת האובייקט
                Genre newGenre = new Genre { GenreDescription = genreName };

                // שליחה ל-API
                int result = await api.InsertGenre(newGenre);

                if (result > 0)
                {
                    MessageBox.Show($"Genre '{genreName}' added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadAllData(); // ריענון הטבלה כדי לראות את הז'אנר החדש
                }
                else
                {
                    MessageBox.Show("The server refused to add the genre. It might already exist.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                // זה יתפוס שגיאות תקשורת או שגיאות ב-Internal Server
                MessageBox.Show("Connection Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}