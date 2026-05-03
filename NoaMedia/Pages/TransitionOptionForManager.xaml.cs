using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Model;

namespace NoaMedia.Pages
{
    public partial class TransitionOptionForManager : Page
    {
        // שמירת המשתמש במקום רק את הסטטוס
        private User _managerUser;

        public TransitionOptionForManager(bool isPremium)
        {
            InitializeComponent();
            // שליפת המשתמש המחובר מהאפליקציה
            var myApp = Application.Current as App;
            _managerUser = myApp?.LoggedInUser;
        }

        private void BtnUserMode_Click(object sender, RoutedEventArgs e)
        {
            //  שולחים את אובייקט המשתמש לדף הבית
            if (_managerUser != null)
            {
                this.NavigationService.Navigate(new Home(_managerUser));
            }
        }
        // ניווט לדף הניהול
        private void BtnAdminMode_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new AdminPage());
        }
        // התנתקות והחזרה לדף הכניסה
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var myApp = Application.Current as App;
            if (myApp != null) { myApp.LoggedInUser = null; }

            Log_in.currentUser = null;
            this.NavigationService.Navigate(new Log_in());
        }
    }
}