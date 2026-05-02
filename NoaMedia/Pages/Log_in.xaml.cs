using ApiInterface;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NoaMedia.Pages;

namespace NoaMedia.Pages
{
    public partial class Log_in : Page
    {
        public static User currentUser = null;
        InterfaceAPI api = new InterfaceAPI();

        public Log_in()
        {
            InitializeComponent();
        }

        private void ShowPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Visibility == Visibility.Visible)
            {
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordTextBox.Visibility = Visibility.Visible;
                ShowPasswordButton.Content = "🙈";
            }
            else
            {
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordTextBox.Visibility = Visibility.Collapsed;
                ShowPasswordButton.Content = "👁";
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = (PasswordBox.Visibility == Visibility.Visible)
                ? PasswordBox.Password
                : PasswordTextBox.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("נא להזין שם משתמש וסיסמה.");
                return;
            }

            try
            {
                UserList uList = await api.GetAllUsers();

                currentUser = uList?.FirstOrDefault(u =>
                    u.Name != null && u.Name.Trim().Equals(username, StringComparison.OrdinalIgnoreCase) &&
                    u.Pass == password);

                if (currentUser == null)
                {
                    MessageBox.Show("שם משתמש או סיסמה שגויים.");
                    return;
                }

                UserPremiumList pList = await api.GetAllUserPremiums();
                bool isPremium = (pList != null && pList.Any(p => p.Id == currentUser.Id)) || currentUser.IsPremium;

                if (isPremium && !currentUser.IsPremium)
                {
                    currentUser.IsPremium = true;
                    await api.UpdateUser(currentUser);
                }

                // עדכון האפליקציה לגבי המשתמש המחובר
                var myApp = Application.Current as App;
                if (myApp != null)
                {
                    myApp.LoggedInUser = currentUser;
                }

                if (this.NavigationService != null)
                {
                    if (currentUser.IsAdmin)
                    {
                        MessageBox.Show("שלום מנהל! עובר לדף אפשרויות ניהול...");
                        // תיקון: שליחה לדף המעבר במקום לדף הבית
                        this.NavigationService.Navigate(new TransitionOptionForManager(isPremium));
                    }
                    else
                    {
                        if (isPremium) MessageBox.Show("ברוך הבא VIP! צפייה מהנה.");
                        this.NavigationService.Navigate(new Home(currentUser));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בתהליך ההתחברות: " + ex.Message);
            }
        }

        private void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SignUp());
        }
    }
}