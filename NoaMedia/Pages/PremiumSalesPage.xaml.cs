using ApiInterface;
using Model;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NoaMedia.Pages
{
    public partial class PremiumSalesPage : Page
    {
        private readonly InterfaceAPI api = new InterfaceAPI();

        public PremiumSalesPage()
        {
            InitializeComponent();
        }

        private async void ConfirmPremium_Click(object sender, RoutedEventArgs e) 
        {
            var myApp = Application.Current as App; //  קבלת האפליקציה הנוכחית כדי לגשת למשתמש המחובר
            if (myApp?.LoggedInUser == null) return; 

            int userId = myApp.LoggedInUser.Id; 

            try
            {
                int commentsCount = await api.GetCommentsCountByUser(userId); // קבלת מספר התגובות של המשתמש
                int likesCount = await api.GetLikesCountByUser(userId); // קבלת מספר הלייקים של המשתמש

                if (commentsCount >= 5 && likesCount >= 5) // בדיקה אם המשתמש עומד בתנאי השדרוג
                {
                    int result = await api.UpgradeUserToPremium(userId); // ניסיון לשדרג את המשתמש ל-Premium

                    if (result >= 1) // אם השדרוג הצליח, מעדכנים את המשתמש ומנווטים לדף הבית
                    {
                        myApp.LoggedInUser.IsPremium = true;
                        MessageBox.Show("Success! Your account is now Premium. 👑");

                        // תיקון: שולחים את המשתמש עצמו במקום בוליאני
                        this.NavigationService.Navigate(new Home(myApp.LoggedInUser));
                    }
                    else
                    {
                        MessageBox.Show("Update failed. Please try again."); // הודעה אם השדרוג נכשל
                    }
                }
                else
                {
                    MessageBox.Show("You need 5 likes and 5 comments to upgrade!"); // הודעה למשתמש אם הוא לא עומד בתנאי השדרוג
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message); // טיפול בשגיאות אפשריות במהלך התהליך
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e) // טיפול בלחצן חזרה
        {
            if (this.NavigationService.CanGoBack)
                this.NavigationService.GoBack();
        }
    }
}