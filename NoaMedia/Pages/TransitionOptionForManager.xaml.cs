using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace NoaMedia.Pages
{
    public partial class TransitionOptionForManager : Page
    {
        private bool _isPremium;

        // הבנאי מקבל את הנתון האם המנהל הוא גם פרימיום
        public TransitionOptionForManager(bool isPremium)
        {
            InitializeComponent();
            _isPremium = isPremium;
        }

        // אפשרות 1: כניסה למסך המשתמש הרגיל (צפייה בסרטים)
        private void BtnUserMode_Click(object sender, RoutedEventArgs e)
        {
            // שולחים את המנהל לדף הבית עם סטטוס הפרימיום שלו
            this.NavigationService.Navigate(new Home(_isPremium));
        }

        // אפשרות 2: כניסה למסך ניהול (עריכת סרטים ומשתמשים)
        private void BtnAdminMode_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new AdminPage());
        }

        // אפשרות 3: התנתקות וחזרה למסך הכניסה
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // איפוס המשתמש המחובר ב-App
            var myApp = Application.Current as App;
            if (myApp != null) { myApp.LoggedInUser = null; }

            Log_in.currentUser = null;

            this.NavigationService.Navigate(new Log_in());
        }
    }
}